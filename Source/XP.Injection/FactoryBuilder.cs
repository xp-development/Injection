using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class FactoryBuilder : IFactoryBuilder
  {
    private IObjectFactory _factory;

    public TypeBuilder TypeBuilder { get; }
    public MethodBuilder MethodBuilder { get; }

    public void InitializeFactory(IContainerConstruction containerConstruction, Type keyType, Type valueType, FactoryType factoryType)
    {
      switch (factoryType)
      {
        case FactoryType.ForTransientObject:
          _factory = new TransientObjectFactory(containerConstruction, valueType, TypeBuilder, MethodBuilder);
          break;
        case FactoryType.ForSingletonObject:
          _factory = new SingletonObjectFactory(containerConstruction, valueType, TypeBuilder, MethodBuilder);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(factoryType), factoryType, null);
      }
    }

    public object CreateObject()
    {
      return _factory.GetOrCreate().Create();
    }

    public FactoryBuilder(TypeBuilder typeBuilder, MethodBuilder methodBuilder)
    {
      TypeBuilder = typeBuilder;
      MethodBuilder = methodBuilder;
    }
  }
}