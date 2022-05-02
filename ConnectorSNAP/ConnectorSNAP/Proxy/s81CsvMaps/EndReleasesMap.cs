using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal class EndReleasesMap : ClassMap<EndReleases>
  {
    public EndReleasesMap()
    {
      //Reading & writing
      Map(m => m.Name).Index(0).Convert(r => r.Row.GetField(0).Split('/').Last().Trim());
      Map(m => m.Restraints).Index(1).TypeConverter<BoolArrConverter>();

      //Writing-only overrides
      Map(m => m.Name).Convert(o => Keyword.BD.GetStringValue() + " / " + o.Value.Name);
      Map(m => m.Restraints).TypeConverter<BoolArrConverter>();
      Map(m => m.defaultRemaining).TypeConverter<ObjectArrConverter>();
    }
  }
}
