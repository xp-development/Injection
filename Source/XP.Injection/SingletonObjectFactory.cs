using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class SingletonObjectFactory : ObjectFactoryBase
  {
      private readonly MethodBuilder _methodBuilder;

    public SingletonObjectFactory( IContainerConstruction containerConstruction, Type valueType, TypeBuilder typeBuilder, MethodBuilder methodBuilder )
      : base(containerConstruction, typeBuilder)
    {
        _methodBuilder = methodBuilder;

      AddConstructor(valueType, typeBuilder);
      AddSingletonFactoryCreateMethod(valueType);
    }

    private void AddSingletonFactoryCreateMethod(Type valueType)
    {
      var fieldBuilder = TypeBuilder.DefineField("_singleton", valueType, FieldAttributes.Private);

      var ilGenerator = _methodBuilder.GetILGenerator();
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

      var createMethod = typeof(IFactory).GetRuntimeMethod(nameof(IFactory.Get), new Type[0]);
      TypeBuilder.DefineMethodOverride(_methodBuilder, createMethod);
    }
  }
}