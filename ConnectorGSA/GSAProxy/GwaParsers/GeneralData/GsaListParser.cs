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

      FromGwaByFuncs(items, out remainingItems, AddName, AddType, AddDefinition);

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

    protected bool AddDefinition(string v)
    {
      var allDefinitions = new List<string>();

      var definitionsArray = v.Split(' ');

      // Find indices of keyword "to" if existing
      var indicesOfKeyword = Enumerable.Range(0, definitionsArray.Count()).Where(i => definitionsArray[i].ToLower() == "to").ToList();

      // User error in gsa list definition, "to" cannot be first in definition
      if (indicesOfKeyword.Any(x => x < 1))
      {
        return false;
      }

      var beginningIndicesOfRanges = indicesOfKeyword.Select(i => i - 1);

      for (var i = 0; i < definitionsArray.Count(); i++)
      {
        // index is part of "to" range
        if (beginningIndicesOfRanges.Contains(i))
        {

          ParseListDefinitionParameter(definitionsArray[i], out var prefix, out var startIndex);
          ParseListDefinitionParameter(definitionsArray[i + 2], out var _, out var endIndex);

          for (var j = startIndex; j < endIndex; j++)
          {
            allDefinitions.Add(prefix + Convert.ToString(j));
          }

          i += 2;
        }

        else
        {
          allDefinitions.Add(definitionsArray[i]);
        }
      }

      record.Definition = allDefinitions;

      return true;
    }

    /// <summary>
    /// Transform list definition value to prefix and index
    /// </summary>
    /// <param name="parameter">e.g. "P1"</param>
    /// <param name="prefix">"P"</param>
    /// <param name="index">1</param>
    protected void ParseListDefinitionParameter(string parameter, out string prefix, out int index)
    {
      var indexString = "";
      prefix = "";

      foreach (var ch in parameter.ToCharArray())
      {
        if (Char.IsDigit(ch))
        {
          indexString += ch;
        }

        else
        {
          prefix += ch;
        }
      }

      index = Convert.ToInt32(indexString);
    }
    #endregion

  }
}
