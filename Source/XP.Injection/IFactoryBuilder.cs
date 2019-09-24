using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public interface IFactoryBuilder<out T>
  {
    TypeBuilder TypeBuilder { get; }
    MethodBuilder MethodBuilder { get; }

    IFactory<T> CreateFactory( Type keyType, Type valueType, FactoryType factoryType );
    Type ValueType { get; }
  }
}