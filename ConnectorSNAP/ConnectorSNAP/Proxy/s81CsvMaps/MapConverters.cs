using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Speckle.SNAP.API;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal class EnumStringConverter<T> : DefaultTypeConverter where T : struct, IConvertible
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => (Enum.TryParse(text, true, out T result)) ? result : default(T);

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ((T)value).GetStringValue(true);
  }

  internal class EnumIntConverter<T> : DefaultTypeConverter where T : struct, IConvertible
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => (Enum.TryParse(text, true, out T result)) ? result : default(T);

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ((int)value).ToString();
  }

  internal class IntBoolConverter : DefaultTypeConverter
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => ToBool(text);
    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ToString((bool)value);
    public static bool ToBool(string v) => (v != "0");
    public static string ToString(bool v) => v ? "1" : "0";
  }

  internal class BoolArrConverter : DefaultTypeConverter
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => ToBoolArr(text);
    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ToString((bool[])value);
    public static bool[] ToBoolArr(string v)
    {
      var retArr = new bool[6];
      var chars = v.Take(6).ToArray();
      for (int i = 0; i < chars.Length; i++)
      {
        retArr[i] = (char.IsDigit(chars[i]) && (chars[1] != '0')) ? true : false;
      }
      return retArr;
    }
    public static string ToString(bool[] v) => new string(v.Select(b => b ? '1' : '0').ToArray());
  }

  internal class ObjectArrConverter : DefaultTypeConverter
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => ToObjArr(text);
    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ToString((object[])value);
    public static object[] ToObjArr(string v) => v.Split(',').ToArray();

    public static string ToString(object[] v) => String.Join(",", v.Select(o => (o is null) ? "" : o.ToString()));
  }
}
