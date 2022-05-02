using ConnectorSNAP.Cache;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public class SnapCache : ISNAPCache
  {
    private List<SnapCacheRecord> records = new List<SnapCacheRecord>();
    private Dictionary<Type, List<int>> recordIndicesByType = new Dictionary<Type, List<int>>();

    public bool GetNatives(out List<object> snapRecords)
    {
      /*
      var namedRecords = new List<object>();
      var otherRecords = new List<object>();
      var namedIntType = typeof(ISnapRecordNamed);
      var namedTypes = recordIndicesByType.Keys.Where(k => k.InheritsOrImplements(namedIntType)).ToList();

      foreach (var nt in namedTypes)
      {
        var orderedObjs = recordIndicesByType[nt].OrderBy(i => ((ISnapRecordNamed)records[i].SnapRecord)
      }
      */
      snapRecords = records.Select(cr => cr.SnapRecord).ToList();
      return true;
    }

    public bool GetNatives<T>(out List<object> snapRecords)
    {
      var t = typeof(T);
      snapRecords = records.Where(cr => cr.SnapRecord.GetType() == t).Select(cr => cr.SnapRecord).ToList();
      return true;
    }

    public bool Upsert(List<object> nativeObjects)
    { 
      foreach (var no in nativeObjects)
      {
        var newIndex = records.Count;
        var t = no.GetType();
        if (!recordIndicesByType.ContainsKey(t))
        {
          recordIndicesByType.Add(t, new List<int>());
        }
        recordIndicesByType[t].Add(newIndex);
        records.Add(new SnapCacheRecord() {  SnapRecord = no });
      }
      return true;
    }

    public bool Upsert(object nativeObject)
    {
      records.Add(new SnapCacheRecord() { SnapRecord = nativeObject });
      return true;
    }

    public bool Contains<T>(string name, out object snapRecord) where T : ISnapRecordNamed
    {
      var t = typeof(T);
      if (recordIndicesByType.ContainsKey(t))
      {
        var typeRecords = recordIndicesByType[t].Select(i => records[i].SnapRecord).ToList();
        var matchedByName = typeRecords.FirstOrDefault(r => ((ISnapRecordNamed)r).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (matchedByName != null)
        {
          snapRecord = matchedByName;
          return true;
        }
      }
      snapRecord = null;
      return false;
    }

    public void Clear()
    {
      records.Clear();
      recordIndicesByType.Clear();
    }
  }
}
