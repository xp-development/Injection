﻿using FluentAssertions;
using XP.Injection.UnitTests.TestsClasses;
using Xunit;

namespace XP.Injection.UnitTests._Container
{
  public class Locate
  {
    [Fact]
    public void ShouldLocateSingletonObject()
    {
      var container = new Container();
      container.RegisterSingleton<ISimpleClass1, SimpleClass1>();

      var locatedObject1 = container.Locate<ISimpleClass1>();
      var locatedObject2 = container.Locate<ISimpleClass1>();

      locatedObject1.Should().BeOfType<SimpleClass1>();
      locatedObject2.Should().BeOfType<SimpleClass1>();
      locatedObject1.Should().BeSameAs(locatedObject2);
    }

    [Fact]
    public void ShouldLocateTransientObject()
    {
      var container = new Container();
      container.Register<ISimpleClass1, SimpleClass1>();

      var locatedObject1 = container.Locate<ISimpleClass1>();
      var locatedObject2 = container.Locate<ISimpleClass1>();

      locatedObject1.Should().BeOfType<SimpleClass1>();
      locatedObject2.Should().BeOfType<SimpleClass1>();
      locatedObject1.Should().NotBeSameAs(locatedObject2);
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

    [Fact]
    public void ShouldLocateTransientObjectWithAnotherTransientObject()
    {
      var container = new Container();
      container.Register<ICtorInjectionClass, CtorInjectionClass>();
      container.Register<ISimpleClass1, SimpleClass1>();
      container.Register<ISimpleClass2, SimpleClass2>();

      var locatedObject1 = container.Locate<ICtorInjectionClass>();
      var locatedObject2 = container.Locate<ICtorInjectionClass>();

      locatedObject1.Should().BeOfType<CtorInjectionClass>();
      locatedObject2.Should().BeOfType<CtorInjectionClass>();
      locatedObject1.Should().NotBeSameAs(locatedObject2);
      locatedObject1.SimpleClass1.Should().NotBeSameAs(locatedObject2.SimpleClass1);
      locatedObject1.SimpleClass2.Should().NotBeSameAs(locatedObject2.SimpleClass2);
    }

    [Fact]
    public void ShouldLocateSingletonObjectWithAnotherSingletonObject()
    {
      var container = new Container();
      container.RegisterSingleton<ICtorInjectionClass, CtorInjectionClass>();
      container.RegisterSingleton<ISimpleClass1, SimpleClass1>();
      container.RegisterSingleton<ISimpleClass2, SimpleClass2>();

      var locatedObject1 = container.Locate<ICtorInjectionClass>();
      var locatedObject2 = container.Locate<ICtorInjectionClass>();

      locatedObject1.Should().BeOfType<CtorInjectionClass>();
      locatedObject2.Should().BeOfType<CtorInjectionClass>();
      locatedObject1.Should().BeSameAs(locatedObject2);
      locatedObject1.SimpleClass1.Should().BeSameAs(locatedObject2.SimpleClass1);
      locatedObject1.SimpleClass2.Should().BeSameAs(locatedObject2.SimpleClass2);
    }

    [Fact]
    public void ShouldLocateSingletonObjectWithAnotherTransientObject()
    {
      var container = new Container();
      container.RegisterSingleton<ICtorInjectionClass, CtorInjectionClass>();
      container.Register<ISimpleClass1, SimpleClass1>();
      container.Register<ISimpleClass2, SimpleClass2>();

      var locatedObject1 = container.Locate<ICtorInjectionClass>();
      var locatedObject2 = container.Locate<ICtorInjectionClass>();

      locatedObject1.Should().BeOfType<CtorInjectionClass>();
      locatedObject2.Should().BeOfType<CtorInjectionClass>();
      locatedObject1.Should().BeSameAs(locatedObject2);
      locatedObject1.SimpleClass1.Should().BeSameAs(locatedObject2.SimpleClass1);
      locatedObject1.SimpleClass2.Should().BeSameAs(locatedObject2.SimpleClass2);
    }
  }
}