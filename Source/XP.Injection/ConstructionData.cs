using System;
using System.Reflection.Emit;

namespace XP.Injection
{
  internal class ConstructionData
  {
    public TypeBuilder FactoryTypeBuilder { get; set; }
    public MethodBuilder FactoryMethodBuilder { get; set; }
    public IFactory Factory { get; set; }
    public Type ValueType { get; set; }

    public ConstructionData(TypeBuilder factoryTypeBuilder = null, MethodBuilder factoryMethodBuilder = null, IFactory factory = null)
    {
      FactoryTypeBuilder = factoryTypeBuilder;
      FactoryMethodBuilder = factoryMethodBuilder;
      Factory = factory;
    }
  }
}