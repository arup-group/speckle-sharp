using DesktopUI2.Models;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public static class Commands
  {

    public static bool OpenFile(string filePath, bool visible)
    {
      Instance.SnapModel.Proxy = new SnapProxy(); //Use a real proxy
      var opened = ((SnapProxy)Instance.SnapModel.Proxy).OpenFile(filePath);
      if (!opened)
      {
        return false;
      }
      return true;
    }

    internal static void LoadDataFromFile(IProgress<string> gwaLoggingProgress = null, IEnumerable<ResultGroup> resultGroups = null, IEnumerable<ResultType> resultTypes = null)
    {
      var errored = new Dictionary<int, object>();

      try
      {
        if (((SnapProxy)Instance.SnapModel.Proxy).GetRecords(out var records))
        {
          for (int i = 0; i < records.Count(); i++)
          {
            if (!Instance.SnapModel.Cache.Upsert(records[i]))
            {
              errored.Add(i, records[i]);
            }
          }
        }
      }
      catch
      {

      }
    }

    public static async Task<bool> Receive(string commitId, ITransport transport, List<Base> topLevelObjects)
    {
      var errors = new List<Exception>();
      var commitObject = await Operations.Receive(
          commitId,
          transport,
          onErrorAction: (s, e) =>
          {
            errors.Add(e);
          },
          disposeTransports: true
          );

      if (commitObject != null)
      {
        foreach (var prop in commitObject.GetDynamicMembers().Where(m => commitObject[m] is Base))
        {
          topLevelObjects.Add((Base)commitObject[prop]);
        }
        return true;
      }
      return false;
    }

    public static bool ConvertToNative(List<Base> objects, ISpeckleConverter converter) //Includes writing to Cache
    {
      Instance.SnapModel.Cache.Clear();
      try
      {
        var nativeObjects = converter.ConvertToNative(objects).ToList();
        //((SnapCache)Instance.SnapModel.Cache).Upsert(nativeObjects);
      }
      catch (Exception ex)
      {
        converter.Report.LogOperationError(new Exception("Unable to convert one or more received objects: " + ex.Message));
      }

      return true;
    }

    public static List<Base> FlattenCommitObject(object obj, Func<Base, bool> IsSingleObjectFn)
    {
      //This is needed because with GSA models, there could be a design and analysis layer with objects appearing in both, so only include the first
      //occurrence of each object (distinguished by the ID returned by the Base.GetId() method) in the list returned
      var uniques = new Dictionary<Type, HashSet<string>>();
      return FlattenCommitObject(obj, IsSingleObjectFn, uniques);
    }


    private static List<Base> FlattenCommitObject(object obj, Func<Base, bool> IsSingleObjectFn, Dictionary<Type, HashSet<string>> uniques)
    {
      List<Base> objects = new List<Base>();

      if (obj is Base @base)
      {
        if (IsSingleObjectFn(@base))
        {
          var t = obj.GetType();
          var id = (string.IsNullOrEmpty(@base.id)) ? @base.GetId() : @base.id;
          if (!uniques.ContainsKey(t))
          {
            uniques.Add(t, new HashSet<string>() { id });
            objects.Add(@base);
          }
          if (!uniques[t].Contains(id))
          {
            uniques[t].Add(id);
            objects.Add(@base);
          }

          return objects;
        }
        else
        {
          foreach (var prop in @base.GetDynamicMembers())
          {
            objects.AddRange(FlattenCommitObject(@base[prop], IsSingleObjectFn, uniques));
          }
          foreach (var kvp in @base.GetMembers())
          {
            var prop = kvp.Key;
            objects.AddRange(FlattenCommitObject(@base[prop], IsSingleObjectFn, uniques));
          }
          return objects;
        }
      }

      if (obj is List<object> list)
      {
        foreach (var listObj in list)
        {
          objects.AddRange(FlattenCommitObject(listObj, IsSingleObjectFn, uniques));
        }
        return objects;
      }
      else if (obj is List<Base> baseObjList)
      {
        foreach (var baseObj in baseObjList)
        {
          objects.AddRange(FlattenCommitObject(baseObj, IsSingleObjectFn, uniques));
        }
        return objects;
      }
      else if (obj is IDictionary dict)
      {
        foreach (DictionaryEntry kvp in dict)
        {
          objects.AddRange(FlattenCommitObject(kvp.Value, IsSingleObjectFn, uniques));
        }
        return objects;
      }

      return objects;
    }
  }
}
