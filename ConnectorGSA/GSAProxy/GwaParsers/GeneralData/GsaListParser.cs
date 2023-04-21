using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.LIST, GwaSetCommandType.Set, true)]
  public class GsaListParser : GwaParser<GsaList>
  {
    public GsaListParser(GsaList gsaList) : base(gsaList) { }

    public GsaListParser() : base(new GsaList()) { }
    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }

      var items = remainingItems;

      if (items[1].ToUpper() == "NODE") // Add nodes currently gets all nodes only.
        FromGwaByFuncs(items, out remainingItems, AddName, (v) => v.TryParseStringValue(out record.Type), (v) => AddNodes(v, out record.Definition));

      else
      {
        // Element/case come from analysis
        var gsaLayer = items[1].ToUpper() == "MEMBER" ? GSALayer.Design : GSALayer.Analysis;
        FromGwaByFuncs(items, out remainingItems, AddName, (v) => v.TryParseStringValue(out record.Type), (v) => AddEntities(v, gsaLayer, out record.Definition));
      }

      // Record with null definition determined invalid. Record will still be valid and converted if definition is empty.
      return record.Definition != null;
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      gwa = new List<string>();

      if (!InitialiseGwa(includeSet, out var items) || string.IsNullOrEmpty(record.Name))
        return false;

      // LIST.1 | num | name | type | list (definition)
      var allItemsAdded = AddItems(ref items, record.Name, record.Type.GetStringValue(), string.Join(" ", record.Definition));

      if (!allItemsAdded)
        return false;

      if (Join(items, out var gwaLine))
        gwa.Add(gwaLine);

      return gwa.Count > 0;
    }

    #region From GWA
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    //protected bool AddType(string v)
    //{
    //  if (Enum.TryParse(v, true, out ListType type))
    //  {
    //    record.Type = type;
    //    return true;
    //  }

    //  else if (string.Equals(v, "UNDEF", StringComparison.OrdinalIgnoreCase))
    //  {
    //    record.Type = ListType.Unspecified;
    //    return true;
    //  }

    //  return false;
    //}

    #endregion
  }
}
