using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public class FactoryBuilder : IFactoryBuilder
  {
    public FactoryBuilder(IContainerConstruction construction, TypeBuilder typeBuilder, MethodBuilder methodBuilder)
    {
      _construction = construction;
      TypeBuilder = typeBuilder;
      MethodBuilder = methodBuilder;
    }

    public TypeBuilder TypeBuilder { get; }
    public MethodBuilder MethodBuilder { get; }

    public IFactory CreateFactory(Type keyType, Type valueType, FactoryType factoryType)
    {
      ValueType = valueType;
      IObjectFactory factory;
      switch (factoryType)
      {
        case FactoryType.ForTransientObject:
          factory = new TransientObjectFactory(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        case FactoryType.ForSingletonObject:
          factory = new SingletonObjectFactory(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(factoryType), factoryType, null);
      }

       return factory.Create(keyType);
    }

    public Type ValueType { get; private set; }

    private readonly IContainerConstruction _construction;
  }
}