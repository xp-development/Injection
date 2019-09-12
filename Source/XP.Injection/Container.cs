using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class Container : IContainerConstruction
  {
    public Container()
    {
      if (_moduleBuilder == null)
      {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
      }
    }

    public IFactoryBuilder GetOrAddFactoryBuilder(Type keyType)
    {
      if (_factoryBuilders.TryGetValue(keyType, out var factoryBuilder))
        return factoryBuilder;

      var factoryTypeBuilder = _moduleBuilder.DefineType("TypeFactory" + _uniqueIdentifier++ + keyType.Name, TypeAttributes.Public, null, new[] {typeof(IFactory<>).MakeGenericType(keyType)});
      var factoryMethodBuilder = factoryTypeBuilder.DefineMethod("IFactory.Get", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), new Type[0]);
      return _factoryBuilders.GetOrAdd(keyType, new FactoryBuilder(this, factoryTypeBuilder, factoryMethodBuilder));
    }

    public void Register<TKey, TValue>()
      where TValue : TKey
    {
      Register(typeof(TKey), typeof(TValue));
    }

    public void Register(Type keyType, Type valueType)
    {
      var builder = GetOrAddFactoryBuilder(keyType);
      builder.InitializeFactory(keyType, valueType, FactoryType.ForTransientObject);
    }

    public void RegisterSingleton<TKey, TValue>()
    {
      RegisterSingleton(typeof(TKey), typeof(TValue));
    }

    public void RegisterSingleton(Type keyType, Type valueType)
    {
      var builder = GetOrAddFactoryBuilder(keyType);
      builder.InitializeFactory(keyType, valueType, FactoryType.ForSingletonObject);
    }

    public TKey Locate<TKey>()
    {
      return (TKey) Locate(typeof(TKey));
    }

    public object Locate(Type keyType)
    {
      return _factoryBuilders[keyType].CreateObject();
    }

    private static int _uniqueIdentifier;

    private readonly ConcurrentDictionary<Type, IFactoryBuilder> _factoryBuilders = new ConcurrentDictionary<Type, IFactoryBuilder>();
    private readonly ModuleBuilder _moduleBuilder;
  }
}