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

      if (items[1] == "MEMBER")
        FromGwaByFuncs(items, out remainingItems, AddName, AddType, (v) => AddEntities(v, GSA.API.GSALayer.Design, out record.Definition));
      
      else if (items[1] == "NODE") // Add nodes currently gets all nodes only.
        FromGwaByFuncs(items, out remainingItems, AddName, AddType, (v) => AddNodes(v, out record.Definition));

      // Element, Case
      else
        FromGwaByFuncs(items, out remainingItems, AddName, AddType, (v) => AddEntities(v, GSA.API.GSALayer.Analysis, out record.Definition));

        return true;
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      throw new NotImplementedException();
    }

    #region From GWA
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    protected bool AddType(string v)
    {
      record.Type = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    #endregion

  }
}
