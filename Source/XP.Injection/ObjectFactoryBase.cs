using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public abstract class ObjectFactoryBase<T> : IObjectFactory<T>
  {
    protected TypeBuilder TypeBuilder { get; }

    protected ObjectFactoryBase(IContainerConstruction containerConstruction, TypeBuilder typeBuilder)
    {
      ContainerConstruction = containerConstruction;
      TypeBuilder = typeBuilder;
    }

    public IFactory<object> Create(Type keyType)
    {
      var factoryBuilder = ContainerConstruction.GetType().GetTypeInfo().GetDeclaredMethod("GetOrAddFactoryBuilder").MakeGenericMethod(keyType).Invoke(ContainerConstruction, new object[0]);
      var constructorArgs = ((Type)factoryBuilder.GetType().GetTypeInfo().GetDeclaredProperty("ValueType").GetMethod.Invoke(factoryBuilder, new object[0])).GetPublicConstructor().GetConstructorParameterTypes().Select(Create).Cast<object>().ToArray();
      return (IFactory<object>) Activator.CreateInstance(((TypeBuilder)factoryBuilder.GetType().GetTypeInfo().GetDeclaredProperty("TypeBuilder").GetMethod.Invoke(factoryBuilder, new object[0])).CreateTypeInfo().AsType(), constructorArgs.Length == 0 ? null : constructorArgs);
    }

    protected void AddConstructor(Type valueType, TypeBuilder typeBuilder)
    {
      var constructorParameterTypes = valueType.GetPublicConstructor().GetConstructorParameterTypes();
      if (constructorParameterTypes.Length == 0)
        return;

      var constructorTypes = constructorParameterTypes.Select(x =>
                                                              {
                                                                var factoryBuilder = ContainerConstruction.GetType().GetTypeInfo().GetDeclaredMethod("GetOrAddFactoryBuilder").MakeGenericMethod(x).Invoke(ContainerConstruction, new object[0]);
                                                                return new {ConstructorType = x, FactoryType = ((TypeBuilder)factoryBuilder.GetType().GetTypeInfo().GetDeclaredProperty("TypeBuilder").GetMethod.Invoke(factoryBuilder, new object[0])).AsType()};
                                                              }).ToArray();

      var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorTypes.Select(x => x.FactoryType).ToArray());
      var ilGenerator = constructorBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(valueType);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.First());
      var parameterCounter = 0;
      foreach (var constructorType in constructorTypes)
      {
        var fieldBuilder = typeBuilder.DefineField($"_field{++parameterCounter}", constructorType.FactoryType, FieldAttributes.Private);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_S, parameterCounter);
        ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        ConstructorFieldBuilders.Add(constructorType.ConstructorType, fieldBuilder);
      }

      ilGenerator.Emit(OpCodes.Ret);
    }

    protected readonly Dictionary<Type, FieldBuilder> ConstructorFieldBuilders = new Dictionary<Type, FieldBuilder>();
    protected readonly IContainerConstruction ContainerConstruction;
  }
}