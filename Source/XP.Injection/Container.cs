using System;
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

    public IFactoryBuilder<T> GetOrAddFactoryBuilder<T>()
    {
      Type keyType = typeof(T);
      if (_factoryBuilders.TryGetValue(keyType, out var factoryBuilder))
        return (IFactoryBuilder<T>) factoryBuilder;

      var factoryTypeBuilder = _moduleBuilder.DefineType("TypeFactory" + _uniqueIdentifier++ + keyType.Name, TypeAttributes.Public, null, new[] {typeof(IFactory<>).MakeGenericType(keyType)});
      var factoryMethodBuilder = factoryTypeBuilder.DefineMethod("IFactory.Get", MethodAttributes.Public | MethodAttributes.Virtual, keyType, new Type[0]);
      var builder = new FactoryBuilder<T>(this, factoryTypeBuilder, factoryMethodBuilder);
      _factoryBuilders.Add(keyType, (IFactoryBuilder<object>) builder);
      return builder;
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
        foreach (var registryEntry in _registry.ToList().Where(x => !x.MissingInjectionTypes.Any()))
        {
          var builder = (IFactoryBuilder<object>)GetType().GetTypeInfo().GetDeclaredMethod(nameof(GetOrAddFactoryBuilder)).MakeGenericMethod(registryEntry.KeyType).Invoke(this, new object[0]);
          _factories.Add(registryEntry.KeyType, builder.CreateFactory(registryEntry.KeyType, registryEntry.ValueType, registryEntry.FactoryType));
          _registry.Remove(registryEntry);
          foreach (var entry in _registry)
            entry.MissingInjectionTypes.Remove(registryEntry.KeyType);

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
      registryEntry.MissingInjectionTypes.AddRange(valueType.GetPublicConstructor().GetConstructorParameterTypes().Where(x => !_factories.ContainsKey(x)));

      _registry.Add(registryEntry);
    }

    public TKey Locate<TKey>()
    {
      return (TKey) Locate(typeof(TKey));
    }

    public object Locate(Type keyType)
    {
      return _factories[keyType].Get();
    }

    private static int _uniqueIdentifier;

    private readonly Dictionary<Type, IFactoryBuilder<object>> _factoryBuilders = new Dictionary<Type, IFactoryBuilder<object>>();
    private readonly Dictionary<Type, IFactory<object>> _factories = new Dictionary<Type, IFactory<object>>();
    private readonly IList<RegistryEntry> _registry = new List<RegistryEntry>();
    private readonly ModuleBuilder _moduleBuilder;
  }
}