using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Speckle.ConnectorGSA.Proxy.Cache
{
  public class GsaCache : IGSACache
  {
    #region GsaRecord_cache
    public int NumRecords { get => ValidRecords.Where(r => r.Latest).Count(); }

    private readonly List<GsaCacheRecord> records = new List<GsaCacheRecord>();
    private readonly List<string> foundStreamIds = new List<string>();  // To avoid storing stream ID strings multiple times
    private Dictionary<GSALayer, HashSet<int>> nodeIndicesByLayer = new Dictionary<GSALayer, HashSet<int>>() { { GSALayer.Design, new HashSet<int>() }, { GSALayer.Analysis, new HashSet<int>() }, { GSALayer.Both, new HashSet<int>() } };

    //Performance-enhancing index tables for fast lookup
    private readonly Dictionary<Type, HashSet<int>> recordIndicesBySchemaType = new Dictionary<Type, HashSet<int>>();
    private readonly Dictionary<Type, Dictionary<int, HashSet<int>>> recordIndicesBySchemaTypeGsaId = new Dictionary<Type, Dictionary<int, HashSet<int>>>();
    private readonly Dictionary<string, HashSet<int>> recordIndicesByApplicationId = new Dictionary<string, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> recordIndicesByStreamIdIndex = new Dictionary<int, HashSet<int>>();

    //Provisional allocation of GSA record numbers - it's mainly to avoid clashing of GSA record numbers - doesn't care about streams or latest/previous states
    // of records
    // < GSA schema type , { < index, app_id >, < index, app_id >, ... } >
    private readonly Dictionary<Type, IPairCollectionComparable<int, string>> provisionals = new Dictionary<Type, IPairCollectionComparable<int, string>>();
    #endregion

    #region SpeckleObject_cache
    private readonly List<object> objects = new List<object>();
    private readonly Dictionary<GSALayer, HashSet<int>> objectIndicesByLayer = new Dictionary<GSALayer, HashSet<int>>();
    private readonly Dictionary<Type, HashSet<int>> objectIndicesByType = new Dictionary<Type, HashSet<int>>();
    private readonly Dictionary<Type, Dictionary<string, HashSet<int>>> objectIndicesByTypeAppId = new Dictionary<Type, Dictionary<string, HashSet<int>>>();
    private readonly Dictionary<Type, Dictionary<int, HashSet<int>>> objectIndicesBySchemaTypesGsaId = new Dictionary<Type, Dictionary<int, HashSet<int>>>();
    #endregion 

    private List<GsaCacheRecord> ValidRecords { get => records.Where(r => r != null).ToList(); }

    public int NumSpeckleObjects { get => objects.Count(); }

    public int NumNatives { get => records.Where(r => r.Latest).Count(); }

    private object cacheLock = new object();

    public GsaCache() { }

    #region upsert

    #region native
    //These first two Upsert methods are only called when hydrating the cache from the GSA instance
    public bool Upsert(GsaRecord gsaRecord) => UpsertInternal(gsaRecord, true, out int? _);

    public bool Upsert(IEnumerable<GsaRecord> gsaRecords)
    {
      int failed = 0;
      foreach (var r in gsaRecords)
      {
        //if (!Upsert(r))
        if (!UpsertInternal(r, true, out int? _))
        {
          failed++;
        }
      }
      return (failed == 0);
    }

    //Called by the kit
    //Not every record has stream IDs (like generated nodes)
    public bool Upsert(GsaRecord record, bool? latest = true) => UpsertInternal(record, latest, out int? _);

    #endregion

    #region speckle
    //When receiving, this is where the speckle objects are inserted into the cache, ready to be converted
    public bool Upsert(Dictionary<string, object> objectsByApplicationId, GSALayer layer = GSALayer.Both)
    {
      foreach (var kvp in objectsByApplicationId)
      {
        UpsertInternal(kvp.Value, kvp.Value.GetType(), kvp.Key, out int? upsertedIndex, layer);
      }
      return true;
    }

    //Application ID is separate because this assembly doesn't yet reference the Core library
    public bool SetNatives(Type speckleType, string applicationId, IEnumerable<GsaRecord> natives)
    {
      var t = speckleType;
      if (!ValidSpeckleObjectTypeApplicationId(speckleType, applicationId))
      {
        return false;
      }

      foreach (var n in natives)
      {
        if (!UpsertInternal(n, true, out int? upsertedIndex) && upsertedIndex.HasValue)
        {
          return false;
        }
      }
      return true;
    }

    public bool SetSpeckleObjects(GsaRecord gsaRecord, Dictionary<string, object> speckleObjects, GSALayer layer = GSALayer.Both)
    {
      var t = gsaRecord.GetType();
      if (!gsaRecord.Index.HasValue)
      {
        return false;
      }
      var gsaIndex = gsaRecord.Index.Value;
      if (!ValidSchemaTypeGsaIndex(t, gsaIndex))
      {
        return false;
      }

      lock (cacheLock)
      {
        if (!objectIndicesBySchemaTypesGsaId.ContainsKey(t))
        {
          objectIndicesBySchemaTypesGsaId.Add(t, new Dictionary<int, HashSet<int>>());
        }
        if (!objectIndicesBySchemaTypesGsaId[t].ContainsKey(gsaIndex))
        {
          objectIndicesBySchemaTypesGsaId[t].Add(gsaIndex, new HashSet<int>());
        }
      }

      foreach (var appId in speckleObjects.Keys)
      {
        var speckleType = speckleObjects[appId].GetType();
        if (UpsertInternal(speckleObjects[appId], speckleType, appId, out int? upsertedIndex, layer) && upsertedIndex.HasValue)
        {
          lock (cacheLock)
          {
            objectIndicesBySchemaTypesGsaId[t][gsaIndex].Add(upsertedIndex.Value);
          }
        }
      }
      return true;
    }
    #endregion

    private bool UpsertInternal(object speckleObject, Type speckleType, string appId, out int? upsertedIndex, GSALayer layer)
    {
      int newIndex = -1;
      lock (cacheLock)
      {
        newIndex = objects.Count();
        objects.Add(speckleObject);

        objectIndicesByType.UpsertDictionary(speckleType, newIndex);

        if (!objectIndicesByTypeAppId.ContainsKey(speckleType))
        {
          objectIndicesByTypeAppId.Add(speckleType, new Dictionary<string, HashSet<int>>());
        }
        objectIndicesByTypeAppId[speckleType].UpsertDictionary(appId, newIndex);

        objectIndicesByLayer.UpsertDictionary(layer, newIndex);
        if (layer == GSALayer.Both)
        {
          objectIndicesByLayer.UpsertDictionary(GSALayer.Design, newIndex);
          objectIndicesByLayer.UpsertDictionary(GSALayer.Analysis, newIndex);
        }
      }
      upsertedIndex = newIndex;
      return true;
    }

    private bool UpsertInternal(GsaRecord record, bool? latest, out int? upsertedIndex)
    {
      var t = record.GetType();
      try
      {
        if (record.Index.HasValue)
        {
          var matchingRecordsByIndex = new Dictionary<int, GsaCacheRecord>();
          lock (cacheLock)
          {
            if (GetAllRecords(t, record.Index.Value, out var foundRecordsByIndex) && foundRecordsByIndex != null && foundRecordsByIndex.Count > 0)
            {
              matchingRecordsByIndex = foundRecordsByIndex;
            }
          }

          if (matchingRecordsByIndex.Count() > 0)
          {
            var equalRecords = matchingRecordsByIndex.Where(kvp => Equals(kvp.Value.GsaRecord, record)).ToList();
            if (equalRecords.Count() == 1)
            {
              //There should just be one equal record

              //There is no change to the record but it clearly means it's part of the latest
              if (latest.HasValue)
              {
                lock (cacheLock)
                {
                  equalRecords.First().Value.Latest = latest.Value;
                }
              }
              upsertedIndex = equalRecords.First().Key;
              return true;
            }
            else if (equalRecords.Count() == 0)
            {
              lock (cacheLock)
              {
                //These will be return at the next call to GetToBeDeletedGwa() and removed at the next call to Snapshot()
                foreach (var r in matchingRecordsByIndex.Values)
                {
                  r.Latest = false;
                }
              }
            }
            else if (equalRecords.Count() > 1)
            {
              throw new Exception("Unexpected multiple matches found in upsert of cache records");
            }
          }
        }

        //if there is no matching or no equal records, then add it as a new one
        Add(t, record, out int addedIndex, true);
        upsertedIndex = addedIndex;
        return true;
      }
      catch (Exception ex)
      {
        upsertedIndex = null;
        return false;
      }
    }

    private bool Add(Type t, GsaRecord record, out int addedIndex, bool? latest = true)
    {
      lock (cacheLock)
      {
        addedIndex = records.Count();
        records.Add(new GsaCacheRecord(record));

        recordIndicesBySchemaType.UpsertDictionary(t, addedIndex);

        if (record.Index.HasValue)
        {
          if (!recordIndicesBySchemaTypeGsaId.ContainsKey(t))
          {
            recordIndicesBySchemaTypeGsaId.Add(t, new Dictionary<int, HashSet<int>>());
          }
          recordIndicesBySchemaTypeGsaId[t].UpsertDictionary(record.Index.Value, addedIndex);

          AddNodesToLayer(t, record);
        }

        if (!string.IsNullOrEmpty(record.ApplicationId))
        {
          var trimmedAppId = record.ApplicationId.Replace(" ", "");
          if (!string.IsNullOrEmpty(trimmedAppId))
          {
            recordIndicesByApplicationId.UpsertDictionary(trimmedAppId, addedIndex);
          }
        }

        if (!string.IsNullOrEmpty(record.StreamId))
        {
          if (!foundStreamIds.Contains(record.StreamId))
          {
            foundStreamIds.Add(record.StreamId);
          }
          var streamIdIndex = foundStreamIds.IndexOf(record.StreamId);
          recordIndicesByStreamIdIndex.UpsertDictionary(streamIdIndex, addedIndex);
        }

        if (provisionals.ContainsKey(t))
        {
          if (!string.IsNullOrEmpty(record.ApplicationId) && provisionals[t].ContainsRight(record.ApplicationId))
          {
            provisionals[t].RemoveRight(record.ApplicationId);
          }
          if (record.Index.HasValue)
          {
            //In most cases where there is an Application ID and the provisional index matches the one of the new record, the call above will have removed
            //its index from the provisionals table.  But in the odd case where a different index is specified than the one that was assigned to the application ID,
            //the existing one at that index needs to be moved
            if (provisionals[t].ContainsLeft(record.Index.Value))
            {
              //Only move the reservation if there is an Application ID involved
              if (provisionals[t].FindRight(record.Index.Value, out string right) && !string.IsNullOrEmpty(right))
              {
                provisionals[t].RemoveLeft(record.Index.Value);
                var newIndex = FindNextFreeIndexForProvisional(t);
                UpsertProvisional(t, newIndex, right);
              }
            }
          }

          if (provisionals[t].Count() == 0)
          {
            provisionals.Remove(t);
          }
        }
      }
      return true;
    }

    private void AddNodesToLayer(Type t, GsaRecord record)
    {
      var recordIndices = new List<int>() { record.Index.Value };
      var referenceRecords = new List<GsaRecord>();

      if (t == typeof(GsaMemb) || t == typeof(GsaEl))
      {
        GSALayer layer = t == typeof(GsaEl) ? GSALayer.Analysis : GSALayer.Design;

        if (t == typeof(GsaMemb))
        {
          GetPrecedentNatives<GsaMemb>(recordIndices, out referenceRecords);
        }
        else if (t == typeof(GsaEl))
        {
          GetPrecedentNatives<GsaEl>(recordIndices, out referenceRecords);
        }

        var referenceNodeIndices = referenceRecords.Where(r => r != null).Where(rec => rec.GetType() == typeof(GsaNode)).Select(rec => rec.Index);

        foreach (var nodeIndex in referenceNodeIndices)
        {
          nodeIndicesByLayer.UpsertDictionary(layer, nodeIndex.Value);
        }
      }
    }

    private bool Equals(GsaRecord a, GsaRecord b)
    {
      //TO DO
      return false;
    }

    private void UpsertProvisional(Type t, int index, string applicationId = null)
    {
      if (!provisionals.ContainsKey(t))
      {
        provisionals.Add(t, new PairCollectionComparable<int, string>());
      }
      if (!provisionals[t].ContainsLeft(index))
      {
        provisionals[t].Add(index, string.IsNullOrEmpty(applicationId) ? null : applicationId);
      }
    }
    #endregion

    #region lookup
    public bool GetNatives(out List<GsaRecord> gsaRecords)
    {
      lock (cacheLock)
      {
        gsaRecords = records.Where(r => r.Latest).Select(r => r.GsaRecord).ToList();
      }
      return true;
    }

    public bool GetNativesFilteredByList(out List<GsaRecord> gsaRecords, List<string> filterSelections)
    {
      var records = new List<GsaRecord>();
      var listRecords = new List<GsaList>();

      lock (cacheLock)
      {
        GetNatives<GsaList>(out records);
        if (records == null)
        {
          gsaRecords = null;
          return false;
        }

        listRecords = records.Cast<GsaList>().ToList();
        var filteredListRecords = listRecords.Where(l => filterSelections.Contains(l.Name)).ToList();

        foreach (var listRecord in filteredListRecords)
        {
          var precendentRecords = new List<GsaRecord>();

          switch (listRecord.Type)
          {
            case ListType.Member:
              GetPrecedentNatives<GsaMemb>(listRecord.Definition, out precendentRecords);
              break;
            case ListType.Element:
              GetPrecedentNatives<GsaEl>(listRecord.Definition, out precendentRecords);
              break;
            case ListType.Node:
              GetPrecedentNatives<GsaNode>(listRecord.Definition, out precendentRecords);
              break;
            case ListType.Case:
              GetPrecedentNatives<GsaLoadCase>(listRecord.Definition, out precendentRecords);
              break;
            default:
              break;
          }
          records.AddRange(precendentRecords);
        }
      }

      gsaRecords = records.Distinct().Where(g => g != null).ToList();
      return true;
    }

    public bool GetPrecedentNatives<T>(List<int> recordIndices, out List<GsaRecord> precendentRecords)
    {
      var t = typeof(T);

      var records = new List<GsaRecord>();

      foreach (var index in recordIndices)
      {
        if (t == typeof(GsaMemb))
        {
          GetNative<GsaMemb>(index, out var memberRecord);
          if (memberRecord != null)
          {
            records.Add(memberRecord);

            var gsaMemb = (GsaMemb)memberRecord;

            // Get members precedents:
            if (gsaMemb.Is1dMember())
            {
              // Nodes
              if (gsaMemb.NodeIndices.Count >= 2)
              {
                foreach (var node in gsaMemb.NodeIndices)
                {
                  GetNative<GsaNode>(node, out var nodeRecord);
                  if (nodeRecord != null) records.Add(nodeRecord);
                }
              }

              // Orientation node
              if (gsaMemb.OrientationNodeIndex.IsIndex())
              {
                GetNative<GsaNode>(gsaMemb.OrientationNodeIndex.Value, out var nodeRecord);
                if (nodeRecord != null) records.Add(nodeRecord);
              }

              // Section
              if (gsaMemb.PropertyIndex.IsIndex())
              {
                GetNative<GsaSection>(gsaMemb.PropertyIndex.Value, out var propertyRecord);
                if (propertyRecord != null) records.Add(propertyRecord);
              }
            }

            else if (gsaMemb.Is2dMember())
            {
              // Nodes
              if (gsaMemb.NodeIndices.Count >= 3)
              {
                foreach (var node in gsaMemb.NodeIndices)
                {
                  GetNative<GsaNode>(node, out var nodeRecord);
                  if (nodeRecord != null) records.Add(nodeRecord);
                }
              }

              // Void nodes
              if (gsaMemb.Voids.HasValues())
              {
                foreach (var vd in gsaMemb.Voids)
                {
                  foreach (var node in vd)
                  {
                    GetNative<GsaNode>(node, out var nodeRecord);
                    if (nodeRecord != null) records.Add(nodeRecord);
                  }
                }
              }

              // Polyline nodes
              if (gsaMemb.Polylines.HasValues())
              {
                foreach (var polyline in gsaMemb.Polylines)
                {
                  foreach (var node in polyline)
                  {
                    GetNative<GsaNode>(node, out var nodeRecord);
                    if (nodeRecord != null) records.Add(nodeRecord);
                  }
                }
              }

              // Point nodes
              if (gsaMemb.PointNodeIndices.HasValues())
              {
                foreach (var node in gsaMemb.PointNodeIndices)
                {
                  GetNative<GsaNode>(node, out var nodeRecord);
                  if (nodeRecord != null) records.Add(nodeRecord);
                }
              }

              // Additional area
              if (gsaMemb.AdditionalAreas.HasValues())
              {
                foreach (var area in gsaMemb.AdditionalAreas)
                {
                  foreach (var node in area)
                  {
                    GetNative<GsaNode>(node, out var nodeRecord);
                    if (nodeRecord != null) records.Add(nodeRecord);
                  }
                }
              }

              // Property
              if (gsaMemb.PropertyIndex.IsIndex())
              {
                GetNative<GsaProp2d>(gsaMemb.PropertyIndex.Value, out var propertyRecord);
                if (propertyRecord != null) records.Add(propertyRecord);
              }
            }
          }
        }

        else if (t == typeof(GsaEl))
        {
          GetNative<GsaEl>(index, out var elementRecord);
          if (elementRecord != null)
          {
            records.Add(elementRecord);

            var gsaEl = (GsaEl)elementRecord;

            // Get element precedents:
            if (gsaEl.Is1dElement())
            {
              if (gsaEl.NodeIndices.Count >= 2)
              {
                foreach (var node in gsaEl.NodeIndices)
                {
                  GetNative<GsaNode>(node, out var nodeRecord);
                  if (nodeRecord != null) records.Add(nodeRecord);
                }
              }

              // Orientation node
              if (gsaEl.OrientationNodeIndex.IsIndex())
              {
                GetNative<GsaNode>(gsaEl.OrientationNodeIndex.Value, out var nodeRecord);
                if (nodeRecord != null) records.Add(nodeRecord);
              }

              // Section
              if (gsaEl.PropertyIndex.IsIndex())
              {
                GetNative<GsaSection>(gsaEl.PropertyIndex.Value, out var propertyRecord);
                if (propertyRecord != null) records.Add(propertyRecord);
              }

              // Parent member
              if (gsaEl.ParentIndex.IsIndex())
              {
                GetNative<GsaMemb>(gsaEl.ParentIndex.Value, out var memberRecord);

                records.Add(memberRecord);

                memberRecord = (GsaMemb)memberRecord;
                if (memberRecord != null)
                {
                  // Drills down recursively to hit typeof(GsaMemb) base case
                  GetPrecedentNatives<GsaMemb>(new List<int>() { memberRecord.Index.Value }, out var embeddedMemberRecords);
                  if (embeddedMemberRecords != null)
                  {
                    var uniqueRecords = embeddedMemberRecords.Where(rec => !records.Contains(rec)).ToList();
                    records.AddRange(uniqueRecords);
                  }
                }
              }
            }

            else if (gsaEl.Is2dElement())
            {
              // Nodes
              if (gsaEl.NodeIndices.Count >= 3)
              {
                foreach (var node in gsaEl.NodeIndices)
                {
                  GetNative<GsaNode>(node, out var nodeRecord);
                  if (nodeRecord != null) records.Add(nodeRecord);
                }
              }

              // Property
              if (gsaEl.PropertyIndex.IsIndex())
              {
                GetNative<GsaProp2d>(gsaEl.PropertyIndex.Value, out var propertyRecord);
                if (propertyRecord != null) records.Add(propertyRecord);
              }

              // Parent member
              if (gsaEl.ParentIndex.IsIndex())
              {
                GetNative<GsaMemb>(gsaEl.ParentIndex.Value, out var memberRecord);
                if (memberRecord != null)
                {
                  records.Add(memberRecord);

                  memberRecord = (GsaMemb)memberRecord;

                  // Drills down recursively to hit typeof(GsaMemb) base case
                  GetPrecedentNatives<GsaMemb>(new List<int>() { memberRecord.Index.Value }, out var embeddedMemberRecords);

                  if (embeddedMemberRecords != null)
                  {
                    var uniqueRecords = embeddedMemberRecords.Where(rec => !records.Contains(rec)).ToList();
                    records.AddRange(uniqueRecords);
                  }
                }
              }
            }
          }
        }

        else if (t == typeof(GsaNode))
        {
          GetNative<GsaNode>(index, out var nodeRecord);
          if (nodeRecord != null)
          {
            records.Add(nodeRecord);

            var gsaNode = (GsaNode)nodeRecord;

            // Get node precedents:

            // Mass property
            if (gsaNode.MassPropertyIndex.IsIndex())
            {
              GetNative<GsaPropMass>(gsaNode.MassPropertyIndex.Value, out var propMassRecord);
              if (propMassRecord != null) records.Add(propMassRecord);
            }

            // Spring property
            if (gsaNode.SpringPropertyIndex.IsIndex())
            {
              GetNative<GsaPropSpr>(gsaNode.SpringPropertyIndex.Value, out var springPropRecord);
              if (springPropRecord != null) records.Add(springPropRecord);
            }
          }
        }

        else if (t == typeof(GsaLoadCase))
        {
          GetNative<GsaLoadCase>(index, out var loadCaseRecord);
          if (loadCaseRecord != null)
          {
            records.Add(loadCaseRecord);
          }
        }
      }

      precendentRecords = records.Distinct().ToList();

      return true;
    }

    public bool GetNatives(Type t, out List<GsaRecord> gsaRecords)
    {
      lock (cacheLock)
      {

        if (recordIndicesBySchemaType.ContainsKey(t))
        {
          gsaRecords = recordIndicesBySchemaType[t].Select(i => records[i].GsaRecord).ToList();
          return true;
        }
      }
      gsaRecords = null;
      return false;
    }

    public bool GetNatives<T>(out List<GsaRecord> gsaRecords)
    {
      var t = typeof(T);
      lock (cacheLock)
      {
        if (recordIndicesBySchemaType.ContainsKey(t))
        {
          gsaRecords = recordIndicesBySchemaType[t].Select(i => records[i].GsaRecord).ToList();
          return true;
        }
      }
      gsaRecords = null;
      return false;
    }

    public bool GetNative<T>(int index, out GsaRecord gsaRecord) => GetNative(typeof(T), index, out gsaRecord);

    public bool GetNative(Type t, int index, out GsaRecord gsaRecord)
    {
      lock (cacheLock)
      {
        if (GetAllRecords(t, index, out var foundRecordsByIndex))
        {
          var latestFound = foundRecordsByIndex.Values.Where(r => r.Latest);
          if (latestFound.Count() > 0)
          {
            gsaRecord = latestFound.First().GsaRecord;
            return true;
          }
        }
      }
      gsaRecord = null;
      return false;
    }

    public GsaRecord GetNative<T>(int index) => GetNative(typeof(T), index, out var gsaRecord) ? gsaRecord : null;

    public bool GetNative<T>(out List<GsaRecord> gsaRecords) => GetNative(typeof(T), out gsaRecords);

    public bool GetNative(Type t, out List<GsaRecord> gsaRecords)
    {
      lock (cacheLock)
      {
        if (GetAllRecords(t, out var foundRecords))
        {
          var latestFound = foundRecords.Where(r => r.Latest);
          if (latestFound.Count() > 0)
          {
            gsaRecords = latestFound.Select(f => f.GsaRecord).ToList();
            return true;
          }
        }
      }
      gsaRecords = null;
      return false;
    }

    public bool GetSpeckleObjects<T>(int gsaIndex, out List<object> foundObjects, GSALayer layer = GSALayer.Both)
    {
      return GetSpeckleObjects<T, object>(gsaIndex, out foundObjects, layer);
    }

    public bool GetSpeckleObjects<T, U>(int gsaIndex, out List<U> foundObjects, GSALayer layer = GSALayer.Both)
    {
      var t = typeof(T);
      if (!ValidSchemaTypeGsaIndex(t, gsaIndex) || !ValidSchemaTypeGsaIndexForSpeckleObjects(t, gsaIndex) || !objectIndicesByLayer.ContainsKey(layer))
      {
        foundObjects = null;
        return false;
      }
      var layerIndices = objectIndicesByLayer[layer];
      var typeIndices = objectIndicesBySchemaTypesGsaId[t][gsaIndex];
      var indices = new List<int>();
      foreach (var ti in typeIndices)
      {
        if (layerIndices.Contains(ti))
        {
          indices.Add(ti);
        }
      }
      if (indices.Count == 0)
      {
        foundObjects = null;
        return false;
      }
      foundObjects = indices.Select(i => objects[i]).Cast<U>().ToList();
      return true;
    }

    public bool GetSpeckleObjects(out List<object> foundObjects, GSALayer layer = GSALayer.Both)
    {
      if (!objectIndicesByLayer.ContainsKey(layer))
      {
        foundObjects = null;
        return false;
      }
      //var relevantObjects = objects.ToList();
      var relevantObjects = objectIndicesByLayer[layer].Select(i => objects[i]).ToList();

      foundObjects = relevantObjects;
      return true;
    }

    public bool GetSpeckleObjectsByType<T, U>(out List<U> foundObjects, GSALayer layer = GSALayer.Both)
    {
      if (!objectIndicesByLayer.ContainsKey(layer))
      {
        foundObjects = null;
        return false;
      }

      var t = typeof(T);
      var typeIndices = objectIndicesBySchemaTypesGsaId[t];
      var indices = new List<int>();

      foreach (var v in typeIndices)
      {
        indices.Add(v.Value.FirstOrDefault());
      }
      foundObjects = indices.Select(i => objects[i]).Cast<U>().ToList();

      return true;
    }

    public bool LayerContainsNode(int nodeIndex, GSALayer layer = GSALayer.Both)
    {
      if (layer == GSALayer.Both)
      {
        return nodeIndicesByLayer[GSALayer.Design].Contains(nodeIndex) || nodeIndicesByLayer[GSALayer.Analysis].Contains(nodeIndex);
      }

      return nodeIndicesByLayer[layer].Contains(nodeIndex);
    }

    public int? LookupIndex<T>(string applicationId)
    {
      var t = typeof(T);
      return (ValidAppId(applicationId, out string appId) ? GetRecordIndex(t, appId) : null);
    }

    public List<int> LookupIndices<T>(IEnumerable<string> applicationIds)
    {
      var t = typeof(T);
      return (ValidAppIds(applicationIds, out List<string> appIds) ? GetRecordIndices(t, appIds) : new List<int>());
    }

    public List<int> LookupIndices<T>()
    {
      var t = typeof(T);
      return (GetRecordIndices(t).ToList());
    }


    public string GetApplicationId<T>(int gsaIndex)
    {
      var t = typeof(T);
      if (!recordIndicesBySchemaTypeGsaId.ContainsKey(t) || recordIndicesBySchemaTypeGsaId[t] == null
       || !recordIndicesBySchemaTypeGsaId[t].ContainsKey(gsaIndex) || recordIndicesBySchemaTypeGsaId[t][gsaIndex] == null
       || recordIndicesBySchemaTypeGsaId[t][gsaIndex].Count == 0)
      {
        return "";
      }
      return recordIndicesBySchemaTypeGsaId[t][gsaIndex].OrderBy(i => i).Select(i => records[i].ApplicationId).FirstOrDefault();
    }

    public string GetApplicationId(Type t, int gsaIndex)
    {
      if (!recordIndicesBySchemaTypeGsaId.ContainsKey(t) || recordIndicesBySchemaTypeGsaId[t] == null
       || !recordIndicesBySchemaTypeGsaId[t].ContainsKey(gsaIndex) || recordIndicesBySchemaTypeGsaId[t][gsaIndex] == null
       || recordIndicesBySchemaTypeGsaId[t][gsaIndex].Count == 0)
      {
        return "";
      }
      return recordIndicesBySchemaTypeGsaId[t][gsaIndex].OrderBy(i => i).Select(i => records[i].ApplicationId).FirstOrDefault();
    }

    //To be fed into the proxy- assume the proxy knows about which keywords are SET and which are SET_AT
    //[ keyword, index, GWA line(s) ]
    public List<GsaRecord> GetExpiredRecords()
    {
      var matchingRecords = ValidRecords.Where(r => r.IsAlterable && r.Previous == true && r.Latest == false).ToList();
      return matchingRecords.Select(r => r.GsaRecord).ToList();
    }

    public List<GsaRecord> GetDeletableRecords()
    {
      var matchingRecords = ValidRecords.Where(r => r.IsAlterable && r.Latest == true).ToList();
      return matchingRecords.Select(r => r.GsaRecord).ToList();
    }

    private int? GetRecordIndex(Type t, string applicationId)
    {
      if (string.IsNullOrEmpty(applicationId))
      {
        return null;
      }
      else if (recordIndicesByApplicationId.ContainsKey(applicationId) && recordIndicesBySchemaType.ContainsKey(t))
      {
        var indices = recordIndicesByApplicationId[applicationId].Intersect(recordIndicesBySchemaType[t]);
        return (indices.Count() == 0) ? null : (int?)indices.Select(i => records[i].GsaRecord.Index).OrderBy(i => i).Last();
      }
      else if (provisionals.ContainsKey(t) && provisionals[t].ContainsRight(applicationId) && provisionals[t].FindLeft(applicationId, out int provisionalIndex))
      {
        return provisionalIndex;
      }
      return null;
    }


    private SortedSet<int> GetRecordIndices(Type t)
    {
      if (!recordIndicesBySchemaType.ContainsKey(t))
      {
        return new SortedSet<int>();
      }
      //should return GSA indices, and be ordered!

      var gsaIndexHash = GetRecordIndexHashSet(t);
      var retSet = new SortedSet<int>();
      foreach (var i in gsaIndexHash)
      {
        retSet.Add(i);
      }
      return retSet;
    }

    private List<int> GetRecordIndices(Type t, IEnumerable<string> applicationIds)
    {
      var appIds = applicationIds.Where(aid => !string.IsNullOrEmpty(aid));
      if (appIds.Count() == 0 || (!recordIndicesBySchemaType.ContainsKey(t) && !provisionals.ContainsKey(t)))
      {
        return new List<int>();
      }
      var indicesToReturn = new List<int>();
      var indicesHash = new HashSet<int>();
      if (recordIndicesBySchemaType.ContainsKey(t))
      {
        foreach (var colIndex in recordIndicesBySchemaType[t])
        {
          //The appIds have already been checked and they are all present as keys in the recordIndicesByApplicationId dictionary
          foreach (var appId in appIds)
          {
            var index = records[colIndex].GsaRecord.Index;
            if (index != null && recordIndicesByApplicationId.ContainsKey(appId) && recordIndicesByApplicationId[appId].Contains(colIndex)
              && !indicesHash.Contains(index.Value))
            {
              indicesHash.Add(index.Value);
            }
          }
        }

      }
      if (provisionals.ContainsKey(t))
      {
        foreach (var appId in appIds)
        {
          if (provisionals[t].ContainsRight(appId) && provisionals[t].FindLeft(appId, out int index))
          {
            indicesHash.Add(index);
          }
        }
      }
      indicesToReturn = indicesHash.OrderBy(i => i).ToList();
      return indicesToReturn;
    }

    private HashSet<int> GetRecordIndexHashSet(Type t)
    {
      if (!recordIndicesBySchemaType.ContainsKey(t) && !provisionals.ContainsKey(t))
      {
        return new HashSet<int>();
      }
      //should return GSA indices, be ordered!

      var gsaIndexHash = new HashSet<int>();
      if (recordIndicesBySchemaType.ContainsKey(t))
      {
        gsaIndexHash = new HashSet<int>(recordIndicesBySchemaType[t].Where(i => records[i].GsaRecord != null
          && records[i].GsaRecord.Index.HasValue)
          .Select(i => records[i].GsaRecord.Index.Value));
      }
      if (provisionals.ContainsKey(t))
      {
        if (gsaIndexHash.Count == 0)
        {
          return new HashSet<int>(provisionals[t].Lefts);
        }
        else
        {
          gsaIndexHash.UnionWith(new HashSet<int>(provisionals[t].Lefts));
        }
      }
      return gsaIndexHash;
    }

    private bool GetAllRecords(Type t, int gsaIndex, out Dictionary<int, GsaCacheRecord> foundRecords)
    {
      if (!ValidSchemaTypeGsaIndex(t, gsaIndex))
      {
        foundRecords = null;
        return false;
      }
      foundRecords = recordIndicesBySchemaTypeGsaId[t][gsaIndex].ToDictionary(i => i, i => this.records[i]);
      return true;
    }

    private bool GetAllRecords(Type t, string applicationId, out List<GsaCacheRecord> foundRecords)
    {
      if (string.IsNullOrEmpty(applicationId) || !recordIndicesByApplicationId.ContainsKey(applicationId)
        || !recordIndicesBySchemaType.ContainsKey(t))
      {
        foundRecords = null;
        return false;
      }
      var colIndices = recordIndicesByApplicationId[applicationId].Intersect(recordIndicesBySchemaType[t]).OrderBy(i => i);
      foundRecords = colIndices.Select(i => records[i]).ToList();
      return true;
    }

    private bool GetAllRecords(Type t, out List<GsaCacheRecord> foundRecords)
    {
      if (!recordIndicesBySchemaType.ContainsKey(t))
      {
        foundRecords = null;
        return false;
      }
      var colIndices = recordIndicesBySchemaType[t].OrderBy(i => i);
      foundRecords = colIndices.Select(i => records[i]).ToList();
      return true;
    }

    private bool FindProvisionalIndex(Type t, string applicationId, out int? provisionalIndex)
    {
      if (provisionals.ContainsKey(t) && provisionals[t].ContainsRight(applicationId)
        && provisionals[t].FindLeft(applicationId, out int index))
      {
        provisionalIndex = index;
        return true;
      }
      provisionalIndex = null;
      return false;
    }

    private int? HighestProvisional(Type t)
    {
      if (!provisionals.ContainsKey(t) || provisionals[t] == null || provisionals[t].Count() == 0)
      {
        return null;
      }

      return provisionals[t].MaxLeft();
    }

    private bool ProvisionalContains(Type t, int index)
    {
      if (!provisionals.ContainsKey(t) || provisionals[t] == null || provisionals[t].Count() == 0)
      {
        return false;
      }
      return provisionals[t].ContainsLeft(index);
    }

    private bool ValidSchemaTypeGsaIndex(Type t, int gsaIndex)
    {
      var valid = (recordIndicesBySchemaTypeGsaId.ContainsKey(t) && recordIndicesBySchemaTypeGsaId[t] != null
        && recordIndicesBySchemaTypeGsaId[t].ContainsKey(gsaIndex) && recordIndicesBySchemaTypeGsaId[t][gsaIndex] != null
        && recordIndicesBySchemaTypeGsaId[t][gsaIndex].Count > 0);

      return valid;
    }

    private bool ValidSpeckleObjectTypeApplicationId(Type t, string applicationId)
    {
      return (objectIndicesByType.ContainsKey(t) && objectIndicesByTypeAppId.ContainsKey(t) && objectIndicesByTypeAppId[t].ContainsKey(applicationId));
    }

    private bool ValidSchemaTypeGsaIndexForSpeckleObjects(Type t, int gsaIndex)
    {
      return (objectIndicesBySchemaTypesGsaId.ContainsKey(t) && objectIndicesBySchemaTypesGsaId[t] != null
          && objectIndicesBySchemaTypesGsaId[t].ContainsKey(gsaIndex) && objectIndicesBySchemaTypesGsaId[t][gsaIndex] != null);
    }
    #endregion

    #region scaling
    public double GetScalingFactor(UnitDimension unitDimension, string overrideUnits = null)
    {
      throw new NotImplementedException();
    }
    #endregion

    #region reservation

    private int FindNextFreeIndexForProvisional(Type t)
    {
      var indices = GetRecordIndexHashSet(t);
      var highestProvisional = HighestProvisional(t);
      var highestIndex = Math.Max((indices.Count() == 0) ? 0 : indices.Max(), highestProvisional ?? 0);
      for (int i = 1; i <= highestIndex; i++)
      {
        if (!indices.Contains(i) && !ProvisionalContains(t, i))
        {
          return i;
        }
      }
      return highestIndex + 1;
    }

    public int ResolveIndex<T>(string applicationId = "")
    {
      var t = typeof(T);
      return ResolveIndex(t, applicationId);
    }

    public int ResolveIndex(Type t, string applicationId)
    {
      lock (cacheLock)
      {
        if (ValidAppId(applicationId, out string appId) && GetAllRecords(t, appId, out var matchingRecords))
        {
          if (matchingRecords.Count() == 0)
          {
            if (FindProvisionalIndex(t, appId, out int? provisionalIndex))
            {
              return provisionalIndex.Value;
            }
            //No matches in either previous or latest
            var newIndex = FindNextFreeIndexForProvisional(t);
            UpsertProvisional(t, newIndex, appId);
            return newIndex;
          }
          else
          {
            //There should be only at most one previous and one latest for this type and applicationID
            var existingPrevious = matchingRecords.Where(r => r.Previous && !r.Latest);
            var existingLatest = matchingRecords.Where(r => r.Latest);

            return (existingLatest.Count() > 0)
              ? existingLatest.First().GsaRecord.Index.Value
              : existingPrevious.First().GsaRecord.Index.Value;
          }
        }
        else if (provisionals.ContainsKey(t) && provisionals[t] != null && appId != null
          && provisionals[t].ContainsRight(appId) && provisionals[t].FindLeft(appId, out int provisionalIndex))
        {
          return provisionalIndex;
        }
        else
        {
          //Application ID is empty, null or not found in the cache
          var indices = GetRecordIndexHashSet(t);
          var highestProvisional = HighestProvisional(t);
          var highestIndex = Math.Max((indices.Count() == 0) ? 0 : indices.Max(), highestProvisional ?? 0);
          for (int i = 1; i <= highestIndex; i++)
          {
            if (!indices.Contains(i) && !ProvisionalContains(t, i))
            {
              UpsertProvisional(t, i, appId);
              return i;
            }
          }

          UpsertProvisional(t, highestIndex + 1, appId);
          return highestIndex + 1;
        }
      }
    }

    #region validation
    private bool ValidAppId(string appIdIn, out string appIdOut)
    {
      appIdOut = null;
      if (appIdIn == null)
      {
        return false;
      }
      var appIdTrimmed = appIdIn.Replace(" ", "");
      if (!string.IsNullOrEmpty(appIdTrimmed))
      {
        appIdOut = appIdTrimmed;
        return true;
      }
      return false;
    }
    #endregion

    private bool ValidAppIds(IEnumerable<string> appIdIns, out List<string> appIdOuts)
    {
      appIdOuts = new List<string>();
      if (appIdIns == null)
      {
        return false;
      }
      appIdOuts = new List<string>();
      foreach (var aid in appIdIns)
      {
        if (ValidAppId(aid, out string appId))
        {
          appIdOuts.Add(aid);
        }
      }
      return appIdOuts.Count() > 0;
    }
    #endregion

    public void Clear()
    {
      this.records.Clear();
      recordIndicesByApplicationId.Clear();
      recordIndicesBySchemaType.Clear();
      recordIndicesBySchemaTypeGsaId.Clear();
      recordIndicesByStreamIdIndex.Clear();
      objects.Clear();
      objectIndicesByType.Clear();
      objectIndicesBySchemaTypesGsaId.Clear();
      objectIndicesByTypeAppId.Clear();
      provisionals.Clear();
      nodeIndicesByLayer.Clear();
    }
  }
}
