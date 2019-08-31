﻿using System;
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
  }
}