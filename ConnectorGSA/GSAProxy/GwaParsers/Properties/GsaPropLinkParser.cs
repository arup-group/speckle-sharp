using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.PROP_LINK, GwaSetCommandType.Set, true)]
  public class GsaPropLinkParser : GwaParser<GsaPropLink>
  {
    public GsaPropLinkParser(GsaPropLink GsaPropLink) : base(GsaPropLink) { }

    public GsaPropLinkParser() : base(new GsaPropLink()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }

      //PROP_LINK.2 | num | name | colour | type | x | y | z | xx | yy | zz
      //PROP_LINK.1 | num | name | type | x | y | z | xx | yy | zz
      return FromGwaByFuncs(remainingItems, out remainingItems, AddName, null, AddType); //Skip colour;
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //PROP_LINK.2 | num | name | colour | type | x | y | z | xx | yy | zz
      //PROP_LINK.1 | num | name | type | x | y | z | xx | yy | zz
      AddItems(ref items, record.Name, "NO_RGB", AddType());

      gwa = Join(items, out var gwaLine) ? new List<string>() { gwaLine } : new List<string>();
      return (gwa.Count() > 0);
    }

    #region from_gwa_fns
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    private bool AddType(string v)
    {
      if (Enum.TryParse<LinkType>(v, true, out var t))
      {
        record.Type = t;
      }
      else if (IsLink(v))
      {
        /* Convert explicit definition of the link type into a Dictionary
         * 
         * Custom links are written in the form constrained_node:primary_node-consatrained_node:primary_node 
         * e.g. X:XYY-Y:YXX give a linkage so that the constrained node x displacement depends on the 
         * primary node x and yy displacements and the constrained node y displacement depends on the 
         * primary node y and xx displacements
         */
        record.Type = LinkType.Custom;
        record.LinkedCondition = new Dictionary<AxisDirection6, List<AxisDirection6>>();
        var constraints = v.Split('-');
        foreach (var constraint in constraints)
        {
          var c = constraint.Split(':');

          //Link key
          Enum.TryParse<AxisDirection6>(c[0], out var cKey);

          //Link value
          var cValue = new List<AxisDirection6>();
          var constrainedDirections = SplitStringByRepeatedCharacters(c[1]);
          foreach (var cDir in constrainedDirections)
          {
            Enum.TryParse<AxisDirection6>(cDir, out var d);
            cValue.Add(d);
          }
          record.LinkedCondition.Add(cKey, cValue);
        }
      }
      else
      {
        record.Type = LinkType.NotSet;
        record.LinkedCondition = null;
      }
      return true;
    }

    #endregion

    #region to_gwa_fns
    private string AddType()
    {
      if (record.Type.Equals(LinkType.NotSet))
      {
        return "";
      }
      else if (record.Type.Equals(LinkType.Custom))
      {
        string v = "";
        foreach (var key in record.LinkedCondition.Keys)
        {
          v += key.ToString() + ":" + string.Join("", record.LinkedCondition[key].ConvertAll(f => f.ToString())) + "-";
        }
        return (v.Length > 0) ? v.Remove(v.Length - 1) : v;
      }
      else
      {
        return record.Type.ToString();
      }
    }
    #endregion

    #region helper
    private bool IsLink(string v)
    {
      string allowableLetters = "XYZ:-";

      foreach (char c in v)
      {
        if (!allowableLetters.Contains(c.ToString()))
          return false;
      }
      return true;
    }

    private List<string> SplitStringByRepeatedCharacters(string v)
    {
      var result = new List<string>();
      if (!string.IsNullOrWhiteSpace(v))
      {
        result.Add(v[0].ToString());
        for (int i = 1; i < v.Length; i++)
        {
          var thisChar = v[i];
          var prevChar = v[i - 1];
          if (!thisChar.Equals(prevChar))
          {
            result.Add("");
          }
          result[result.Count - 1] += thisChar;
        }
      }
      return result;
    }
    #endregion
  }
}
