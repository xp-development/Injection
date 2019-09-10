using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  public interface IFactoryBuilder
  {
    TypeBuilder TypeBuilder { get; }
    MethodBuilder MethodBuilder { get; }

    object CreateObject();
    void InitializeFactory( Type keyType, Type valueType, FactoryType factoryType );
    Type GetValueType();
  }
}