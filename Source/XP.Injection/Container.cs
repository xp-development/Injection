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
    private static int _uniqueIdentifier;

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
      Register(typeof(T), typeof(T1));
    }

    public void Register(Type t, Type t1)
    {
      var typeBuilder = CreateFactoryTypeBuilder(t);
      AddFactoryCreateMethod(typeBuilder, t, t1);
      AddToCache(typeBuilder, t);
    }

    public T Locate<T>()
    {
      return (T) Locate(typeof(T));
    }

    public object Locate(Type interfaceType)
    {
      return ((IFactory)_cache[interfaceType]).Create();
    }

    private static void AddFactoryCreateMethod(TypeBuilder typeBuilder, Type t, Type t1)
    {
      var methodBuilder = typeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual,
        typeof(object), new Type[0]);
      var ilGenerator = methodBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(t1);
      ilGenerator.Emit(OpCodes.Newobj, t1.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Stloc_0);
      ilGenerator.Emit(OpCodes.Ldloc_0);
      ilGenerator.Emit(OpCodes.Ret);
      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      typeBuilder.DefineMethodOverride(methodBuilder, createMethod);
    }

    private TypeBuilder CreateFactoryTypeBuilder(Type t)
    {
      return _moduleBuilder.DefineType("TypeFactory" + _uniqueIdentifier++ + t.Name, TypeAttributes.Public, null, new[] {typeof(IFactory<>).MakeGenericType(t)});
    }

    private void AddToCache(TypeBuilder typeBuilder, Type t)
    {
      _cache.Add(t, Activator.CreateInstance(typeBuilder.CreateTypeInfo().AsType()));
    }
  }
}
