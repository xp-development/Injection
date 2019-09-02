using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class Container
  {
    private readonly ConcurrentDictionary<Type, ConstructionData> _construction = new ConcurrentDictionary<Type, ConstructionData>();
    private readonly ModuleBuilder _moduleBuilder;
    private static int _uniqueIdentifier;

    private readonly Dictionary<Type, FieldBuilder> _constructorFieldBuilders = new Dictionary<Type, FieldBuilder>();

    public Container()
    {
      if (_moduleBuilder == null)
      {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
      }
    }

    public void Register<TKey, TValue>()
      where TValue : TKey
    {
      Register(typeof(TKey), typeof(TValue));
    }

    public void Register(Type keyType, Type valueType)
    {
      var builders = GetOrAddBuilders(keyType);
      builders.ValueType = valueType;
      AddConstructor(valueType, builders.FactoryTypeBuilder);
      AddTransientFactoryCreateMethod(keyType, valueType);
    }

    public void RegisterSingleton<TKey, TValue>()
    {
      RegisterSingleton(typeof(TKey), typeof(TValue));
    }

    public void RegisterSingleton(Type keyType, Type valueType)
    {
      var builders = GetOrAddBuilders(keyType);
      builders.ValueType = valueType;
      AddConstructor(valueType, builders.FactoryTypeBuilder);
      AddSingletonFactoryCreateMethod(keyType, valueType);
    }

    public TKey Locate<TKey>()
    {
      return (TKey) Locate(typeof(TKey));
    }

    public object Locate(Type keyType)
    {
      return GetOrCreateFactory(keyType).Create();
    }

    private void AddTransientFactoryCreateMethod(Type keyType, Type valueType)
    {
      var construction = _construction[keyType];
      var ilGenerator = construction.FactoryMethodBuilder.GetILGenerator();

      foreach (var constructorParameterType in _constructorFieldBuilders)
      {
        var constructorParameterBuilders = GetOrAddBuilders(constructorParameterType.Key);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, constructorParameterType.Value);
        ilGenerator.Emit(OpCodes.Callvirt, constructorParameterBuilders.FactoryMethodBuilder);
        ilGenerator.Emit(OpCodes.Castclass, constructorParameterType.Key);
      }

      _constructorFieldBuilders.Clear();
      ilGenerator.Emit(OpCodes.Newobj, valueType.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      construction.FactoryTypeBuilder.DefineMethodOverride(construction.FactoryMethodBuilder, createMethod);
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

    private void AddConstructor(Type valueType, TypeBuilder factoryTypeBuilder)
    {
      var constructorParameterTypes = GetConstructorParameterTypes(valueType);
      if (constructorParameterTypes.Length == 0)
        return;

      var constructorTypes = constructorParameterTypes.Select(x => new { ConstructorType = x, FactoryType = GetOrAddBuilders(x).FactoryTypeBuilder.AsType()}).ToArray();

      var constructorBuilder = factoryTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorTypes.Select(x => x.FactoryType).ToArray());
      var ilGenerator = constructorBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(valueType);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.First());
      var parameterCounter = 0;
      foreach (var constructorType in constructorTypes)
      {
        var fieldBuilder = factoryTypeBuilder.DefineField($"_field{++parameterCounter}", constructorType.FactoryType, FieldAttributes.Private);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_S, parameterCounter);
        ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        _constructorFieldBuilders.Add(constructorType.ConstructorType, fieldBuilder);
      }

      ilGenerator.Emit(OpCodes.Ret);
    }

    private static Type[] GetConstructorParameterTypes(Type valueType)
    {
      var constructorParameterTypes = valueType.GetTypeInfo().DeclaredConstructors.First().GetParameters()
        .Select(x => x.ParameterType).ToArray();
      return constructorParameterTypes;
    }

    private ConstructionData GetOrAddBuilders(Type keyType)
    {
      if (_construction.TryGetValue(keyType, out var constructionData))
      {
        return constructionData;
      }

      var factoryTypeBuilder = _moduleBuilder.DefineType("TypeFactory" + _uniqueIdentifier++ + keyType.Name, TypeAttributes.Public, null, new[] {typeof(IFactory<>).MakeGenericType(keyType)});
      var factoryMethodBuilder = factoryTypeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), new Type[0]);
      return _construction.GetOrAdd(keyType, new ConstructionData(factoryTypeBuilder, factoryMethodBuilder));
    }

    private IFactory GetOrCreateFactory(Type keyType)
    {
      var construction = _construction[keyType];
      if (construction.Factory != null)
        return construction.Factory;

      var constructorArgs = GetConstructorParameterTypes(construction.ValueType).Select(GetOrCreateFactory).Cast<object>().ToArray();
      construction.Factory = (IFactory) Activator.CreateInstance(construction.FactoryTypeBuilder.CreateTypeInfo().AsType(), constructorArgs.Length == 0 ? null : constructorArgs);
      return construction.Factory;
    }
  }
}
