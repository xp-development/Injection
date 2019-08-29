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
      AddTransientFactoryCreateMethod(typeBuilder, t, t1);
      AddToCache(typeBuilder, t);
    }

    public void RegisterSingleton<T, T1>()
    {
      RegisterSingleton(typeof(T), typeof(T1));
    }

    public void RegisterSingleton(Type t, Type t1)
    {
      var typeBuilder = CreateFactoryTypeBuilder(t);
      var fieldBuilder = typeBuilder.DefineField("_singleton", t, FieldAttributes.Private);
      AddSingletonFactoryCreateMethod(typeBuilder, fieldBuilder, t, t1);
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

    private static void AddSingletonFactoryCreateMethod(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, Type t,
      Type t1)
    {
      var methodBuilder = typeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual,
        typeof(object), new Type[0]);
      var ilGenerator = methodBuilder.GetILGenerator();
      var label = ilGenerator.DefineLabel();

      ilGenerator.DeclareLocal(t1);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      ilGenerator.Emit(OpCodes.Brtrue_S, label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Newobj, t1.GetTypeInfo().DeclaredConstructors.First());
      ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
      ilGenerator.MarkLabel(label);
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
      ilGenerator.Emit(OpCodes.Ret);

      var createMethod = typeof(IFactory).GetRuntimeMethod("Create", new Type[0]);
      typeBuilder.DefineMethodOverride(methodBuilder, createMethod);
    }

    private static void AddTransientFactoryCreateMethod(TypeBuilder typeBuilder, Type t, Type t1)
    {
      var methodBuilder = typeBuilder.DefineMethod("IFactory.Create", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), new Type[0]);
      var ilGenerator = methodBuilder.GetILGenerator();
      ilGenerator.DeclareLocal(t1);
      ilGenerator.Emit(OpCodes.Newobj, t1.GetTypeInfo().DeclaredConstructors.First());
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
