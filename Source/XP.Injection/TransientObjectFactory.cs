using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class TransientObjectFactory : ObjectFactoryBase
  {
    private readonly IContainerConstruction _containerConstruction;
    private readonly TypeBuilder _typeBuilder;
    private readonly MethodBuilder _methodBuilder;
    private readonly Dictionary<Type, FieldBuilder> _constructorFieldBuilders = new Dictionary<Type, FieldBuilder>();


    internal TransientObjectFactory(IContainerConstruction containerConstruction, Type valueType, TypeBuilder typeBuilder, MethodBuilder methodBuilder)
      : base(containerConstruction, typeBuilder, valueType)
    {
      _containerConstruction = containerConstruction;
      _typeBuilder = typeBuilder;
      _methodBuilder = methodBuilder;

      AddConstructor(valueType, typeBuilder);
      AddTransientFactoryCreateMethod(valueType);
    }

    private void AddConstructor(Type valueType, TypeBuilder factoryTypeBuilder)
    {
      var constructorParameterTypes = GetConstructorParameterTypes();
      if (constructorParameterTypes.Length == 0)
        return;

      var constructorTypes = constructorParameterTypes.Select(x => new { ConstructorType = x, FactoryType = _containerConstruction.GetOrAddFactoryBuilder(x).TypeBuilder.AsType()}).ToArray();

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

    private void AddTransientFactoryCreateMethod(Type valueType)
    {
      var ilGenerator = _methodBuilder.GetILGenerator();
      foreach (var constructorParameterType in _constructorFieldBuilders)
      {
        var constructorParameterBuilders = _containerConstruction.GetOrAddFactoryBuilder(constructorParameterType.Key);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, constructorParameterType.Value);
        ilGenerator.Emit(OpCodes.Callvirt, constructorParameterBuilders.MethodBuilder);
        ilGenerator.Emit(OpCodes.Castclass, constructorParameterType.Key);
      }

      _constructorFieldBuilders.Clear();
      ilGenerator.Emit(OpCodes.Newobj, valueType.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      _typeBuilder.DefineMethodOverride(_methodBuilder, createMethod);
    }
  }
}