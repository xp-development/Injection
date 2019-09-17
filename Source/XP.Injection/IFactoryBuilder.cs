using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public interface IFactoryBuilder
  {
    TypeBuilder TypeBuilder { get; }
    MethodBuilder MethodBuilder { get; }

    IFactory CreateFactory( Type keyType, Type valueType, FactoryType factoryType );
    Type ValueType { get; }
  }
}