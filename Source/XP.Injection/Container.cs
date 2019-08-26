using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class Container
  {
    private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();
    private readonly ModuleBuilder _moduleBuilder;

    public Container()
    {
      if (_moduleBuilder == null)
      {
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        _moduleBuilder = dynamicAssembly.DefineDynamicModule("DynamicModule");
      }
    }

    public void Register<T, T1>()
      where T1 : T
    {
      var typeBuilder = CreateFactoryTypeBuilder<T>();
      AddFactoryCreateMethod<T, T1>(typeBuilder);
      AddToCache<T>(typeBuilder);
    }

    public T Locate<T>()
    {
      return (T) ((IFactory)_cache[typeof(T)]).Create();
    }

    private static void AddFactoryCreateMethod<T, T1>(TypeBuilder typeBuilder) where T1 : T
    {
      var methodBuilder = typeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual,
        typeof(object), new Type[0]);
      var ilGenerator = methodBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(typeof(T1));
      ilGenerator.Emit(OpCodes.Newobj, typeof(T1).GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Stloc_0);
      ilGenerator.Emit(OpCodes.Ldloc_0);
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      typeBuilder.DefineMethodOverride(methodBuilder, createMethod);
    }

    private TypeBuilder CreateFactoryTypeBuilder<T>()
    {
      return _moduleBuilder.DefineType("TypeFactory", TypeAttributes.Public, null, new[] {typeof(IFactory)});
    }

    private void AddToCache<T>(TypeBuilder typeBuilder)
    {
      _cache.Add(typeof(T), Activator.CreateInstance(typeBuilder.CreateTypeInfo().AsType()));
    }
  }
}
