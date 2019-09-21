using System;
using System.Linq;
using System.Reflection;

namespace XP.Injection
{
  public static class Extensions
  {
    public static ConstructorInfo GetPublicConstructor(this Type type)
    {
      return type.GetTypeInfo().DeclaredConstructors.First(x => x.IsPublic);
    }

    public static Type[] GetConstructorParameterTypes(this ConstructorInfo constructorInfo)
    {
      return constructorInfo.GetParameters().Select(x => x.ParameterType).ToArray();
    }
  }
}