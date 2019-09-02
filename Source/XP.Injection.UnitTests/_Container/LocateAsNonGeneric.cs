using System;
using FluentAssertions;
using XP.Injection.UnitTests.TestsClasses;
using Xunit;

namespace XP.Injection.UnitTests._Container
{
  public class LocateAsNonGeneric
  {
    [Theory]
    [InlineData(typeof(ISimpleClass1), typeof(SimpleClass1))]
    [InlineData(typeof(ISimpleClass2), typeof(SimpleClass2))]
    public void ShouldLocateTransientObject(Type interfaceType, Type classType)
    {
      var container = new Container();
      container.Register(interfaceType, classType);

      var locatedObject1 = container.Locate(interfaceType);
      var locatedObject2 = container.Locate(interfaceType);

      locatedObject1.Should().BeOfType(classType);
      locatedObject2.Should().BeOfType(classType);
      locatedObject1.Should().NotBeSameAs(locatedObject2);
    }

    [Theory]
    [InlineData(typeof(ISimpleClass1), typeof(SimpleClass1))]
    [InlineData(typeof(ISimpleClass2), typeof(SimpleClass2))]
    public void ShouldLocateSingletonObject(Type interfaceType, Type classType)
    {
      var container = new Container();
      container.RegisterSingleton(interfaceType, classType);

      var locatedObject1 = container.Locate(interfaceType);
      var locatedObject2 = container.Locate(interfaceType);

      locatedObject1.Should().BeOfType(classType);
      locatedObject2.Should().BeOfType(classType);
      locatedObject1.Should().BeSameAs(locatedObject2);
    }

    [Fact]
    public void ShouldLocateTransientObjectWithAnotherTransientObject()
    {
      var container = new Container();
      container.Register(typeof(ICtorInjectionClass), typeof(CtorInjectionClass));
      container.Register(typeof(ISimpleClass1), typeof(SimpleClass1));
      container.Register(typeof(ISimpleClass2), typeof(SimpleClass2));

      var locatedObject1 = container.Locate(typeof(ICtorInjectionClass));
      var locatedObject2 = container.Locate(typeof(ICtorInjectionClass));

      locatedObject1.Should().BeOfType<CtorInjectionClass>();
      locatedObject2.Should().BeOfType<CtorInjectionClass>();
      locatedObject1.Should().NotBeSameAs(locatedObject2);
      ((ICtorInjectionClass)locatedObject1).SimpleClass1.Should().NotBeSameAs(((ICtorInjectionClass)locatedObject2).SimpleClass1);
      ((ICtorInjectionClass)locatedObject1).SimpleClass2.Should().NotBeSameAs(((ICtorInjectionClass)locatedObject2).SimpleClass2);
    }

    [Fact]
    public void ShouldLocateTransientObjectWithAnotherSingletonObject()
    {
      var container = new Container();
      container.Register<ICtorInjectionClass, CtorInjectionClass>();
      container.RegisterSingleton<ISimpleClass1, SimpleClass1>();
      container.RegisterSingleton<ISimpleClass2, SimpleClass2>();

      var locatedObject1 = container.Locate<ICtorInjectionClass>();
      var locatedObject2 = container.Locate<ICtorInjectionClass>();

      locatedObject1.Should().BeOfType<CtorInjectionClass>();
      locatedObject2.Should().BeOfType<CtorInjectionClass>();
      locatedObject1.Should().NotBeSameAs(locatedObject2);
      locatedObject1.SimpleClass1.Should().BeSameAs(locatedObject2.SimpleClass1);
      locatedObject1.SimpleClass2.Should().BeSameAs(locatedObject2.SimpleClass2);
    }
  }
}