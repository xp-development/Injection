using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class SingletonObjectFactory : ObjectFactoryBase
  {
    public SingletonObjectFactory(IContainerConstruction containerConstruction, Type valueType, TypeBuilder typeBuilder, MethodBuilder methodBuilder)
      : base(containerConstruction, typeBuilder)
    {
      _methodBuilder = methodBuilder;

      AddConstructor(valueType, typeBuilder);
      AddSingletonFactoryCreateMethod(valueType);
    }

    private void AddSingletonFactoryCreateMethod(Type valueType)
    {
      var singletonFieldBuilder = TypeBuilder.DefineField("_singleton", valueType, FieldAttributes.Private);
      var ilGenerator = _methodBuilder.GetILGenerator();
      var label = ilGenerator.DefineLabel();
      ilGenerator.DeclareLocal(valueType);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, singletonFieldBuilder);
      ilGenerator.Emit(OpCodes.Brtrue_S, label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      foreach (var constructorParameterType in ConstructorFieldBuilders)
      {
        var constructorParameterBuilders = ContainerConstruction.GetOrAddFactoryBuilder(constructorParameterType.Key);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, constructorParameterType.Value);
        ilGenerator.Emit(OpCodes.Callvirt, constructorParameterBuilders.MethodBuilder);
        ilGenerator.Emit(OpCodes.Castclass, constructorParameterType.Key);
      }

      ilGenerator.Emit(OpCodes.Newobj, valueType.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Stfld, singletonFieldBuilder);
      ilGenerator.MarkLabel(label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, singletonFieldBuilder);
      ilGenerator.Emit(OpCodes.Ret);

      var createMethod = typeof(IFactory).GetRuntimeMethod(nameof(IFactory.Get), new Type[0]);
      TypeBuilder.DefineMethodOverride(_methodBuilder, createMethod);
    }

    private readonly MethodBuilder _methodBuilder;
  }
}