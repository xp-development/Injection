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

    public void InitializeFactory(Type keyType, Type valueType, FactoryType factoryType)
    {
      _valueType = valueType;
      switch (factoryType)
      {
        case FactoryType.ForTransientObject:
          _factory = new TransientObjectFactory(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        case FactoryType.ForSingletonObject:
          _factory = new SingletonObjectFactory(_construction, valueType, TypeBuilder, MethodBuilder);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(factoryType), factoryType, null);
      }

       var factory = _factory.Create(keyType);
       _factory.SetFactory(factory);
    }

    public Type GetValueType()
    {
      return _valueType;
    }

    public object CreateObject()
    {
      return _factory.Get();
    }

    private readonly IContainerConstruction _construction;
    private IObjectFactory _factory;
    private Type _valueType;
  }
}