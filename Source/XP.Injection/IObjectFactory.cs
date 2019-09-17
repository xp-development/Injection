using System;

namespace XP.Injection
{
  public interface IObjectFactory
  {
    IFactory Create( Type valueType );
  }
}