using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API
{
  public static class Extensions
  {
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetStringValue(this IConvertible value, bool basicConvertIfNotFound = false)
    {
      // Get the type
      var type = value.GetType();

      // Get fieldinfo for this type
      var fieldInfo = type.GetField(value.ToString());

      // Get the stringvalue attributes
      var attribs = fieldInfo.GetCustomAttributes(
          typeof(StringValue), false) as StringValue[];

      // Return the first if there was a match.
      return attribs.Length > 0 ? attribs[0].Value : basicConvertIfNotFound ? value.ToString() : null;
    }

    public static bool TryParseStringValue<T>(this string v, out T value) where T : IConvertible
    {
      if (!typeof(T).IsEnum)
      {
        throw new ArgumentException("T must be an enumerated type");
      }
      var enumValues = typeof(T).GetEnumValues().OfType<T>().ToDictionary(ev => GetStringValue(ev), ev => ev);
      if (enumValues.Keys.Any(k => k.Equals(v, StringComparison.InvariantCultureIgnoreCase)))
      {
        value = enumValues[v];
        return true;
      }
      value = default(T);
      return false;
    }

    public static double ToDouble(this string v) => double.TryParse(v, out double result) ? result : 0;

    public static double ToInt(this string v) => int.TryParse(v, out int result) ? result : 0;
  }
}
