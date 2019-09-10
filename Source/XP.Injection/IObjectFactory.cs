using System;

namespace XP.Injection
{
  public interface IObjectFactory
  {
    IFactory Create( Type valueType );
    object Get();
    void SetFactory( IFactory factory );
  }
}