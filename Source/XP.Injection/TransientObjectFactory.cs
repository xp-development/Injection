using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class TransientObjectFactory<T> : ObjectFactoryBase<T>
  {
    public TransientObjectFactory(IContainerConstruction containerConstruction, Type valueType, TypeBuilder typeBuilder, MethodBuilder methodBuilder)
      : base(containerConstruction, typeBuilder)
    {
      _typeBuilder = typeBuilder;
      _methodBuilder = methodBuilder;

      AddConstructor(valueType, typeBuilder);
      AddTransientFactoryCreateMethod(valueType);
    }

    private void AddTransientFactoryCreateMethod(Type valueType)
    {
      var ilGenerator = _methodBuilder.GetILGenerator();
      foreach (var constructorParameterType in ConstructorFieldBuilders)
      {
        var factoryBuilder = ContainerConstruction.GetType().GetTypeInfo().GetDeclaredMethod("GetOrAddFactoryBuilder").MakeGenericMethod(constructorParameterType.Key).Invoke(ContainerConstruction, new object[0]);
        var methodBuilder = ((MethodBuilder) factoryBuilder.GetType().GetTypeInfo().GetDeclaredProperty("MethodBuilder").GetMethod.Invoke(factoryBuilder, new object[0]));
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, constructorParameterType.Value);
        ilGenerator.Emit(OpCodes.Callvirt, methodBuilder);
        ilGenerator.Emit(OpCodes.Castclass, constructorParameterType.Key);
      }

      ilGenerator.Emit(OpCodes.Newobj, valueType.GetPublicConstructor());
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory<T>).GetRuntimeMethod("Get", new Type[0]);
      _typeBuilder.DefineMethodOverride(_methodBuilder, createMethod);
    }

    private readonly MethodBuilder _methodBuilder;
    private readonly TypeBuilder _typeBuilder;
  }
}