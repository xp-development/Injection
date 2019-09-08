using System;

namespace XP.Injection
{
  public interface IContainerConstruction
  {
    IFactoryBuilder GetOrAddFactoryBuilder(Type keyType);
  }
}