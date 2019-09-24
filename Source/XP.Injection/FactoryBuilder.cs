using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class FactoryBuilder<T> : IFactoryBuilder<T>
  {
    public FactoryBuilder(IContainerConstruction construction, TypeBuilder typeBuilder, MethodBuilder methodBuilder)
    {
      _construction = construction;
      TypeBuilder = typeBuilder;
      MethodBuilder = methodBuilder;
    }

    public TypeBuilder TypeBuilder { get; }
    public MethodBuilder MethodBuilder { get; }

    public IFactory<T> CreateFactory(Type keyType, Type valueType, FactoryType factoryType)
    {
      ValueType = valueType;
      IObjectFactory<T> factory;
      switch (factoryType)
      {
        case FactoryType.ForTransientObject:
          factory = new TransientObjectFactory<T>(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        case FactoryType.ForSingletonObject:
          factory = new SingletonObjectFactory<T>(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(factoryType), factoryType, null);
      }

       return (IFactory<T>) factory.Create(keyType);
    }

    public Type ValueType { get; private set; }

    private readonly IContainerConstruction _construction;
  }
}