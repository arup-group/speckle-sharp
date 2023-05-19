﻿using Speckle.GSA.API;
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

      // Ensure record.Type correctly set
      FromGwaByFuncs(remainingItems, out remainingItems, AddName, (v) => v.TryParseStringValue(out record.Type)); 

      if (record.Type == ListType.Node)
      {
        FromGwaByFuncs(remainingItems, out remainingItems, (v) => AddNodes(v, out record.Definition));
      }
      else
      {
        var gsaLayer = record.Type == ListType.Member ? GSALayer.Design : GSALayer.Analysis;
        FromGwaByFuncs(remainingItems, out remainingItems, (v) => AddEntities(v, gsaLayer, out record.Definition));
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
    #endregion
  }
}
