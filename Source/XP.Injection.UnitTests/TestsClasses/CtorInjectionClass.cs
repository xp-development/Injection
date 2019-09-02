namespace XP.Injection.UnitTests.TestsClasses
{
  public class CtorInjectionClass : ICtorInjectionClass
  {
    public ISimpleClass1 SimpleClass1 { get; }
    public ISimpleClass2 SimpleClass2 { get; }

    public CtorInjectionClass(ISimpleClass1 simpleClass1, ISimpleClass2 simpleClass2)
    {
      SimpleClass1 = simpleClass1;
      SimpleClass2 = simpleClass2;
    }
  }
}