using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class Container
  {
    private readonly Dictionary<Type, ConstructionData> _construction = new Dictionary<Type, ConstructionData>();
    private readonly ModuleBuilder _moduleBuilder;
    private static int _uniqueIdentifier;

    public Container()
    {
      if (_moduleBuilder == null)
      {
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        _moduleBuilder = dynamicAssembly.DefineDynamicModule("DynamicModule");
      }
    }

    public void Register<TKey, TValue>()
      where TValue : TKey
    {
      Register(typeof(TKey), typeof(TValue));
    }

    public void Register(Type keyType, Type valueType)
    {
      AddBuilders(keyType);
      AddTransientFactoryCreateMethod(keyType, valueType);
      InstantiateFactory(keyType);
    }

    public void RegisterSingleton<TKey, TValue>()
    {
      RegisterSingleton(typeof(TKey), typeof(TValue));
    }

    public void RegisterSingleton(Type keyType, Type valueType)
    {
      AddBuilders(keyType);
      AddSingletonFactoryCreateMethod(keyType, valueType);
      InstantiateFactory(keyType);
    }

    public TKey Locate<TKey>()
    {
      return (TKey) Locate(typeof(TKey));
    }

    public object Locate(Type keyType)
    {
      return _construction[keyType].Factory.Create();
    }

    private void AddSingletonFactoryCreateMethod(Type keyType, Type valueType)
    {
      var construction = _construction[keyType];
      var fieldBuilder = construction.FactoryTypeBuilder.DefineField("_singleton", keyType, FieldAttributes.Private);

      var ilGenerator = construction.FactoryMethodBuilder.GetILGenerator();
      var label = ilGenerator.DefineLabel();

      ilGenerator.DeclareLocal(valueType);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      ilGenerator.Emit(OpCodes.Brtrue_S, label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Newobj, valueType.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
      ilGenerator.MarkLabel(label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      ilGenerator.Emit(OpCodes.Ret);

      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      construction.FactoryTypeBuilder.DefineMethodOverride(construction.FactoryMethodBuilder, createMethod);
    }

    private void AddTransientFactoryCreateMethod(Type keyType, Type valueType)
    {
      var construction = _construction[keyType];

//      var constructorParameterTypes = valueType.GetTypeInfo().DeclaredConstructors.First().GetParameters().Select(x => x.ParameterType).ToArray();
//      typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
//        constructorParameterTypes);

      var ilGenerator = construction.FactoryMethodBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(valueType);
      ilGenerator.Emit(OpCodes.Newobj, valueType.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      construction.FactoryTypeBuilder.DefineMethodOverride(construction.FactoryMethodBuilder, createMethod);
    }

    private void AddBuilders(Type keyType)
    {
      var factoryTypeBuilder = _moduleBuilder.DefineType("TypeFactory" + _uniqueIdentifier++ + keyType.Name, TypeAttributes.Public, null, new[] {typeof(IFactory<>).MakeGenericType(keyType)});
      var factoryMethodBuilder = factoryTypeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), new Type[0]);
      _construction.Add(keyType, new ConstructionData(factoryTypeBuilder, factoryMethodBuilder));
    }

    private void InstantiateFactory(Type keyType)
    {
      var construction = _construction[keyType];
      construction.Factory = (IFactory) Activator.CreateInstance(construction.FactoryTypeBuilder.CreateTypeInfo().AsType());
    }
  }

  internal class ConstructionData
  {
    public TypeBuilder FactoryTypeBuilder { get; set; }
    public MethodBuilder FactoryMethodBuilder { get; set; }
    public IFactory Factory { get; set; }

    public ConstructionData(TypeBuilder factoryTypeBuilder = null, MethodBuilder factoryMethodBuilder = null, IFactory factory = null)
    {
      FactoryTypeBuilder = factoryTypeBuilder;
      FactoryMethodBuilder = factoryMethodBuilder;
      Factory = factory;
    }
  }
}
