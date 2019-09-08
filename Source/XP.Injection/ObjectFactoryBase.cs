using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace XP.Injection
{
  public abstract class ObjectFactoryBase : IObjectFactory
  {
    private readonly IContainerConstruction _containerConstruction;
    public Type ValueType { get; }
    public TypeBuilder TypeBuilder { get; }
    private IFactory _factory;

    protected ObjectFactoryBase(IContainerConstruction containerConstruction, TypeBuilder typeBuilder, Type valueType)
    {
      _containerConstruction = containerConstruction;
      ValueType = valueType;
      TypeBuilder = typeBuilder;
    }

    public IFactory GetOrCreate()
    {
      return GetOrCreate(TypeBuilder.CreateTypeInfo().AsType());
    }

    private IFactory GetOrCreate(Type keyType)
    {
      var constructorArgs = GetConstructorParameterTypes().Select(GetOrCreate).Cast<object>().ToArray();
      return _factory ?? (_factory = (IFactory) Activator.CreateInstance(TypeBuilder.CreateTypeInfo().AsType(), constructorArgs.Length == 0 ? null : constructorArgs));
    }

    protected Type[] GetConstructorParameterTypes()
    {
      var constructorParameterTypes = ValueType.GetTypeInfo().DeclaredConstructors.First().GetParameters()
        .Select(x => x.ParameterType).ToArray();
      return constructorParameterTypes;
    }
  }
}