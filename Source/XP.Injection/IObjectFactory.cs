using System;

namespace XP.Injection
{
  public interface IObjectFactory<out T>
  {
    IFactory<object> Create( Type valueType );
  }
}