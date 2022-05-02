using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal class SectionMap : ClassMap<Section>
  {

    public SectionMap()
    {
      //Reading
      Map(m => m.Name).Index(0).Convert(r => r.Row.GetField(0).Split('/').Last().Trim());
      Map(m => m.SectionType).Index(3).Convert(r => Enum.TryParse(r.Row.GetField(3), out SectionType st) ? st : SectionType.HSection);
      Map(m => m.SectionRelevance).Index(4).Convert(r => Enum.TryParse(r.Row.GetField(4), out SectionRelevance sr) ? sr : SectionRelevance.EntireLength);
      Map(m => m.CustomCatalogueField1).Index(5);
      Map(m => m.CustomCatalogueField2).Index(6);
      Map(m => m.CatalogueItemName).Index(7);
      Map(m => m.StandardDimensionWidth).Index(10);
      Map(m => m.StandardDimensionDepth).Index(11);
      Map(m => m.StandardDimensionWeb).Index(12);
      Map(m => m.StandardDimensionFlange).Index(13);
      Map(m => m.Material).Index(17);

      //Writing
      Map(m => m.Name).Convert(o => Keyword.DGRS.GetStringValue() + " / " + o.Value.Name);
      Map().Index(1).Constant("");
      Map().Index(2).Constant("");
      Map(m => m.SectionType).Index(3).Convert(o => ((int)o.Value.SectionType).ToString());
      Map(m => m.SectionRelevance).Index(4).Convert(o => ((int)o.Value.SectionRelevance).ToString());
      Map(m => m.CustomCatalogueField1);
      Map(m => m.CustomCatalogueField2);
      Map(m => m.CatalogueItemName);
      Map().Index(8).Constant("0");
      Map().Index(9).Constant("0");
      Map(m => m.StandardDimensionWidth);
      Map(m => m.StandardDimensionDepth);
      Map(m => m.StandardDimensionWeb);
      Map(m => m.StandardDimensionFlange);
      Map().Index(14).Constant("0");
      Map().Index(15).Constant("0");
      Map().Index(16).Constant("0");
      Map(m => m.Material).Index(17);
      Map().Constant("0");
    }
  }
}

