using Objects.Structural.Geometry;
using Objects.Structural.Materials;
using Objects.Structural.Properties;
using Objects.Structural.Properties.Profiles;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.GSA.Loading;
using Objects.Structural.GSA.Properties;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Structural.Results;
using Objects.Structural.Analysis;
using Objects.Structural.GSA.Analysis;
using Objects.Structural.GSA.Materials;
using Objects.Structural.GSA.Bridge;
using Objects.Structural.Loading;
using System.Threading.Tasks;
using Speckle.Core.Transports;
using Speckle.Core.Serialisation;
using Speckle.Newtonsoft.Json;

namespace ConverterGSA
{
  //Container for highest level conversion functionality, for both directions (to-Speckle and to-native)
  public partial class ConverterGSA : ISpeckleConverter
  {
    #region Section Mapping
    const string MappingStreamId = "Default Section Mapping Stream";

    private static SQLiteTransport MappingStorage = new SQLiteTransport(scope: "Mappings");

    private bool _useMappings;
    private bool UseMappings
    {
      get { return Settings.ContainsKey("section-mapping") && Settings["section-mapping"] != null; }
    }

    private Base _mappingData;
    private Base MappingData
    {
      get
      {
        if (_mappingData == null)
        {
          // get from settings
          _mappingData = UseMappings ? GetMappingData() : null;
        }
        return _mappingData;
      }
    }

    private string GetProfileNameFromMapping(string family, string type, bool isFraming = true, string target = "grs")
    {
      var targetSection = MappingData[$"{target}"] as Base;
      var sectionList = ((List<object>)targetSection["data"]).Select(m => m as Dictionary<string, object>).ToList();
      var sectionDict = sectionList.Select(m => m as Dictionary<string, object>).ToList();

      var key = isFraming ? $"familyFraming" : "familyColumn";
      var section = sectionDict.Where(x => (string)x[key] == family && (string)x["familyType"] == type).FirstOrDefault();
      var profileName = section != null ? (string)section["speckleSection"] : null;

      return profileName;
    }

    private Dictionary<string, string> GetMappingFromProfileName(string name, bool isFraming = true, string target = "gsa")
    {
      Dictionary<string, string> mappingData = new Dictionary<string, string>();

      var key = Settings["section-mapping"];
      var hash = $"{key}-mappings";
      var objString = MappingStorage.GetObject(hash);

      var objBase = JsonConvert.DeserializeObject<Base>(objString);
      var serializerV2 = new BaseObjectDeserializerV2();
      var data = serializerV2.Deserialize(objString);

      //var mappings = MappingData["mappings"];

      var mappingsList = ((List<object>)data["data"]).Select(m => m as Dictionary<string, object>).ToList();
      var mappingDict = mappingsList.Select(m => m as Dictionary<string, object>).ToList();
      var mapping = mappingDict.Where(x => (string)x["section"] == name).FirstOrDefault();
      if (mapping != null && mapping.ContainsKey(target))
      {
        var targetSection = MappingData[target] as Base;
        var sectionList = ((List<object>)targetSection["data"]).Select(m => m as Dictionary<string, object>).ToList();
        var sectionDict = sectionList.Select(m => m as Dictionary<string, object>).ToList();
        var section = sectionDict.Where(x => (long)x["key"] == (long)mapping[target]).FirstOrDefault();

        //var targetFamily = isFraming ? section["familyFraming"] : section["familyColumn"];
        var targetFamilyType = section["familyType"];
        mappingData["familyFraming"] = section["familyFraming"] as String;
        mappingData["familyColumn"] = section["familyColumn"] as String;
        mappingData["familyType"] = section["familyType"] as String;
        mappingData["profileType"] = section["profileType"] as String;
      }
      else
      {
        return null;
      }

      return mappingData;
    }

    private Base GetMappingData()
    {
      var key = Settings["section-mapping"];
      const string mappingsBranch = "mappings";
      const string sectionBranchPrefix = "sections";

      var mappingData = new Base();

      var hashes = MappingStorage.GetAllHashes();
      var matches = hashes.Where(h => h.Contains(key)).ToList();
      foreach (var match in matches)
      {
        var objString = MappingStorage.GetObject(match);
        var serializerV2 = new BaseObjectDeserializerV2();
        var data = serializerV2.Deserialize(objString);

        if (match.Contains($"{key}-{mappingsBranch}"))
        {
          mappingData[$"{mappingsBranch}"] = data;
        }
        else if (match.Contains(sectionBranchPrefix))
        {
          var name = match.Replace($"{key}-{sectionBranchPrefix}/", "");
          mappingData[$"{name}"] = data;
        }
      }

      Report.Log($"Using section mapping data from stream: {key}");

      return mappingData;
    }

    #endregion

  }
}
