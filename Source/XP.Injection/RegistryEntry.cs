using System;
using System.Collections.Generic;

namespace XP.Injection
{
  public class RegistryEntry
  {
    public Type KeyType { get; set; }
    public Type ValueType { get; set; }
    public FactoryType FactoryType { get; set; }
    public List<Type> MissingInjectionTypes { get; } = new List<Type>();
  }
}