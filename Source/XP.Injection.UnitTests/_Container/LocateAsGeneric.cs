using FluentAssertions;
using XP.Injection.UnitTests.TestsClasses;
using Xunit;

namespace XP.Injection.UnitTests._Container
{
  public class Locate
  {
    [Fact]
    public void ShouldLocateObjectAsTransientObjectWithGenericRegister()
    {
      var container = new Container();
      container.Register<ISimpleClass, SimpleClass>();

      var locatedObject1 = container.Locate<ISimpleClass>();
      var locatedObject2 = container.Locate<ISimpleClass>();

      locatedObject1.Should().BeOfType<SimpleClass>();
      locatedObject2.Should().BeOfType<SimpleClass>();
      locatedObject1.Should().NotBeSameAs(locatedObject2);
    }
  }
}