using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
      AddToRegistry(keyType, valueType, FactoryType.ForTransientObject);
      InitializeAvailableFactories();
    }

    private void InitializeAvailableFactories()
    {
      bool factoryInitialized;
      do
      {
        factoryInitialized = false;
        foreach (var registryEntry in _registry.ToList())
        {
          if (registryEntry.MissingInjectionTypes.Any())
          {
            continue;
          }
          var builder = GetOrAddFactoryBuilder(registryEntry.KeyType);
          builder.InitializeFactory(registryEntry.KeyType, registryEntry.ValueType, registryEntry.FactoryType);
          _registry.Remove(registryEntry);
          foreach (var entry in _registry)
          {
            entry.MissingInjectionTypes.Remove(registryEntry.KeyType);
          }
          factoryInitialized = true;
        }
      } while (factoryInitialized);
    }

    public void RegisterSingleton<TKey, TValue>()
    {
      RegisterSingleton(typeof(TKey), typeof(TValue));
    }

    public void RegisterSingleton(Type keyType, Type valueType)
    {
      AddToRegistry(keyType, valueType, FactoryType.ForSingletonObject);
      InitializeAvailableFactories();
    }

    private void AddToRegistry(Type keyType, Type valueType, FactoryType factoryType)
    {
      var registryEntry = new RegistryEntry { KeyType = keyType, ValueType = valueType, FactoryType = factoryType};
      registryEntry.MissingInjectionTypes.AddRange(GetConstructorParameterTypes(valueType));

      _registry.Add(registryEntry);
    }

    private Type[] GetConstructorParameterTypes(Type type)
    {
      return type.GetTypeInfo().DeclaredConstructors.First().GetParameters().Select(x => x.ParameterType).ToArray();
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
    private readonly IList<RegistryEntry> _registry = new List<RegistryEntry>();
    private readonly ModuleBuilder _moduleBuilder;
  }
}