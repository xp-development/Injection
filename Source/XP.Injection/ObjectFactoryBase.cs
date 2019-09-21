using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public abstract class ObjectFactoryBase : IObjectFactory
  {
    protected TypeBuilder TypeBuilder { get; }

    protected ObjectFactoryBase(IContainerConstruction containerConstruction, TypeBuilder typeBuilder)
    {
      ContainerConstruction = containerConstruction;
      TypeBuilder = typeBuilder;
    }

    public IFactory Create(Type keyType)
    {
      var factoryBuilder = ContainerConstruction.GetOrAddFactoryBuilder(keyType);
      var constructorArgs = factoryBuilder.ValueType.GetPublicConstructor().GetConstructorParameterTypes().Select(Create).Cast<object>().ToArray();
      return (IFactory) Activator.CreateInstance(factoryBuilder.TypeBuilder.CreateTypeInfo().AsType(), constructorArgs.Length == 0 ? null : constructorArgs);
    }

    protected void AddConstructor(Type valueType, TypeBuilder typeBuilder)
    {
      var constructorParameterTypes = valueType.GetPublicConstructor().GetConstructorParameterTypes();
      if (constructorParameterTypes.Length == 0)
        return;

      var constructorTypes = constructorParameterTypes.Select(x => new {ConstructorType = x, FactoryType = ContainerConstruction.GetOrAddFactoryBuilder(x).TypeBuilder.AsType()}).ToArray();

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