using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal sealed class GirderMap : ClassMap<Girder>
  {
    public GirderMap()
    {
      //Reading & writing
      Map(m => m.NodeI).Index(0).Convert(r => (r.Row.GetField(0).Split('/').Last().Trim()));
      Map(m => m.NodeJ).Index(1);
      //Map(m => m.Material).Index(38);

      //Writing
      Map(m => m.NodeI).Convert(o => Keyword.GR.GetStringValue() + " / " + o.Value.NodeI);
      Map(m => m.NodeJ).Convert(o => (o.Value.NodeJ));
      Map().Constant("0,,,1");
      Map(m => m.CrossSection);
      Map().Constant("0,2,,,");
      Map(m => m.BoundaryConditionI);
      Map(m => m.BoundaryConditionJ);
      //Map().Constant("0,0,0,0,0,0,1,0,0,0,0,0,,,,,,,0,0,0,0,0,0,");
      //Map(m => m.Material);
      //Map().Constant(",,,,1,1,1,1,1,1,1,1,1,1,1,1,,,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,1,0,0");
      Map().Constant("0,0,0,0,0,0,1,0,0,0,0,0,,,,,,,0,0,0,0,0,0,,,,,,,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,,,0,0,1,1,0,0,1,0,0");
    }
  }
}
