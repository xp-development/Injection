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
      _containerConstruction = containerConstruction;
      TypeBuilder = typeBuilder;
    }

    public IFactory Create(Type keyType)
    {
      var factoryBuilder = _containerConstruction.GetOrAddFactoryBuilder(keyType);
      var constructorArgs = GetConstructorParameterTypes(factoryBuilder.GetValueType()).Select(Create).Cast<object>().ToArray();
      return (IFactory) Activator.CreateInstance(factoryBuilder.TypeBuilder.CreateTypeInfo().AsType(), constructorArgs.Length == 0 ? null : constructorArgs);
    }

    public object Get()
    {
      return _factory.Get();
    }

    public void SetFactory(IFactory factory)
    {
      _factory = factory;
    }

    protected Type[] GetConstructorParameterTypes(Type type)
    {
      var constructorParameterTypes = type.GetTypeInfo().DeclaredConstructors.First().GetParameters()
                                          .Select(x => x.ParameterType).ToArray();
      return constructorParameterTypes;
    }

    protected void AddConstructor(Type valueType, TypeBuilder typeBuilder)
    {
      var constructorParameterTypes = GetConstructorParameterTypes(valueType);
      if (constructorParameterTypes.Length == 0)
        return;

      var constructorTypes = constructorParameterTypes.Select(x => new {ConstructorType = x, FactoryType = _containerConstruction.GetOrAddFactoryBuilder(x).TypeBuilder.AsType()}).ToArray();

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
        _constructorFieldBuilders.Add(constructorType.ConstructorType, fieldBuilder);
      }

      ilGenerator.Emit(OpCodes.Ret);
    }

    protected readonly Dictionary<Type, FieldBuilder> _constructorFieldBuilders = new Dictionary<Type, FieldBuilder>();
    private readonly IContainerConstruction _containerConstruction;
    private IFactory _factory;
  }
}