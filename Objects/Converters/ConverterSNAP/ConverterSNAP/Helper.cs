using Objects.Structural.Analysis;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConverterSNAP
{
  public class Helper
  {
    //TEMP: Not sure where to put this yet 
    public static List<List<Type>> SpeckleDependencyTree()
    {
      var assembly = Assembly.GetAssembly(typeof(Model));

      var structuralTypes = new HashSet<Type>(assembly.GetTypes().Where(t => t != null && !string.IsNullOrEmpty(t.Namespace) && t.Namespace.ToLower().Contains("structural")));
      var tree = new TypeTreeCollection<Type>(structuralTypes);

      var typeChildren = new Dictionary<Type, List<Type>>();
      var baseType = typeof(Base);
      foreach (var t in structuralTypes)
      {
        var baseClasses = t.GetBaseClasses().Where(bc => structuralTypes.Any(st => st == bc) && bc.InheritsOrImplements(baseType) && bc != baseType);
        foreach (var p in baseClasses)
        {
          typeChildren.UpsertDictionary(p, t);
        }
      }

      foreach (var t in structuralTypes)
      {
        var referencedStructuralTypes = new List<Type>();
        var propertyInfos = t.GetProperties();

        foreach (var pi in propertyInfos)
        {
          Type typeToAdd = null;
          if (pi.IsList(out Type listType))
          {
            if (structuralTypes.Contains(listType))
            {
              typeToAdd = listType;
            }
          }
          else if (structuralTypes.Contains(pi.PropertyType))
          {
            typeToAdd = pi.PropertyType;
          }
          if (typeToAdd != null)
          {
            if (typeChildren.ContainsKey(typeToAdd))
            {
              foreach (var c in typeChildren[typeToAdd])
              {
                if (!referencedStructuralTypes.Contains(c))
                {
                  referencedStructuralTypes.Add(c);
                }
              }
            }
            if (!referencedStructuralTypes.Contains(typeToAdd))
            {
              referencedStructuralTypes.Add(typeToAdd);
            }
          }
        }
        tree.Integrate(t, referencedStructuralTypes.ToArray());
      }
      return tree.Generations();
    }
  }
}
