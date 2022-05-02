using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConverterSNAP
{
  public static class Extensions
  {
    public const double DefaultTolerance = 0.01;

    public static bool EqualsWithinTolerance(this double a, double b) => Math.Abs(a - b) < DefaultTolerance;
    public static bool EqualsWithinTolerance(this double a, double b, double tol) => Math.Abs(a - b) < tol;

    public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(target));
      }
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }
      foreach (var element in source)
      {
        target.Add(element);
      }
    }

    public static void AddRangeIfNotNull<T>(this ICollection<T> target, IEnumerable<T> source)
    {
      if (target != null && source != null && source.Count() > 0)
      {
        target.AddRange(source);
      }
    }

    public static bool IsList(this PropertyInfo pi, out Type listType)
    {
      if (pi.PropertyType.GetTypeInfo().IsGenericType)
      {
        var isList = pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
        listType = isList ? pi.PropertyType.GenericTypeArguments.First() : null;
        return isList;
      }
      listType = null;
      return false;
    }

    public static IEnumerable<Type> GetBaseClasses(this Type type)
    {
      return type.BaseType == typeof(object)
          ? type.GetInterfaces()
          : Enumerable
              .Repeat(type.BaseType, 1)
              .Concat(type.BaseType.GetBaseClasses())
              .Distinct();
    }

    public static void UpsertDictionary<T, U>(this Dictionary<T, List<U>> d, T key, IEnumerable<U> values)
    {
      if (!d.ContainsKey(key))
      {
        d.Add(key, values.ToList());
      }
      foreach (var v in values)
      {
        if (!d[key].Contains(v))
        {
          d[key].Add(v);
        }
      }
    }

    public static void UpsertDictionary<T, U>(this Dictionary<T, List<U>> d, T key, U value)
    {
      if (!d.ContainsKey(key))
      {
        d.Add(key, new List<U>());
      }
      if (!d[key].Contains(value))
      {
        d[key].Add(value);
      }
    }

    //https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
    public static bool InheritsOrImplements(this Type child, Type parent)
    {
      parent = ResolveGenericTypeDefinition(parent);

      var currentChild = child.IsGenericType
                             ? child.GetGenericTypeDefinition()
                             : child;

      while (currentChild != typeof(object))
      {
        if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
          return true;

        currentChild = currentChild.BaseType != null
                       && currentChild.BaseType.IsGenericType
                           ? currentChild.BaseType.GetGenericTypeDefinition()
                           : currentChild.BaseType;

        if (currentChild == null)
          return false;
      }
      return false;
    }

    private static bool HasAnyInterfaces(Type parent, Type child)
    {
      return child.GetInterfaces()
          .Any(childInterface =>
          {
            var currentInterface = childInterface.IsGenericType
              ? childInterface.GetGenericTypeDefinition()
              : childInterface;

            return currentInterface == parent;
          });
    }

    private static Type ResolveGenericTypeDefinition(Type parent)
    {
      var shouldUseGenericType = true;
      if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent)
        shouldUseGenericType = false;

      if (parent.IsGenericType && shouldUseGenericType)
        parent = parent.GetGenericTypeDefinition();
      return parent;
    }

    public static bool ToBoolean(this double dv)
    {
      return (dv == 0) ? false : true;
    }
  }
}
