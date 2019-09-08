using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public interface IFactoryBuilder
  {
    TypeBuilder TypeBuilder { get; }
    MethodBuilder MethodBuilder { get; }

    object CreateObject();
    void InitializeFactory(IContainerConstruction containerConstruction, Type keyType, Type valueType, FactoryType factoryType);
  }
}