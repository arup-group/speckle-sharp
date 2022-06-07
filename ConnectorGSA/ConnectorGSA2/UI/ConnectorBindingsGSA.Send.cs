using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.Models.Filters;
using DesktopUI2.Models.Settings;
using DesktopUI2.ViewModels;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using ConnectorGSA.Models;
using ConnectorGSA.Utilities;
using Microsoft.Win32;
using Newtonsoft.Json;
using Speckle.ConnectorGSA.Proxy;
using Speckle.ConnectorGSA.Proxy.Cache;
using Speckle.Core.Credentials;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System.Collections;
using System.Reflection;
using System.Collections.Concurrent;


namespace ConnectorGSA.UI
{
  public partial class ConnectorBindingsGSA : ConnectorBindings, IConnectorBindingsStandalone
  {
    public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(VersionedHostApplications.GSA);
      if (converter == null)
        throw new Exception("Could not find any Kit!");

      var account = state.Client.Account;

      // set converter settings as tuples (setting slug, setting selection)
      var settings = new Dictionary<string, string>();
      foreach (var setting in state.Settings)
      {
        if (setting.Slug == "layer")
        {
          if (setting.Selection != null)
          {
            if (Enum.TryParse(setting.Selection, out GSALayer streamLayer))
              Instance.GsaModel.StreamLayer = streamLayer;
          }
        }
        //else if (setting.Slug == "send-content")
        //{
        //  Instance.GsaModel.StreamSendConfig = ((CheckBoxSetting)setting).IsChecked ? StreamContentConfig.ModelAndResults : StreamContentConfig.ModelOnly;
        //}
        settings.Add(setting.Slug, setting.Selection);
      }
      converter.SetConverterSettings(settings);

      var selectedObjects = GetSelectionFilterObjects(state.Filter);
      //state.SelectedObjectIds = selectedObjects.Select(x => x.UniqueId).ToList();

      var percentage = 0;
      var perecentageProgressLock = new object();

      var currentFile = ((GsaProxy)Instance.GsaModel.Proxy).FilePath;
      if (currentFile != null) progress.Report.Log($"Using GSA file: {currentFile}");
      else progress.Report.Log($"Using new GSA file");

      var startTime = DateTime.Now;

      progress.Report.Log("Preparing cache");

      var loaded = Commands.LoadDataFromFile(progress); //Ensure all nodes
      if (loaded) progress.Report.Log("Loaded data from file into cache");
      else
      {
        progress.Report.LogOperationError(new Exception("Failed to load data from file into cache"));
        return null;
      }

      percentage += 20;

      var sendResults = ResultSettings != null && ResultSettings.SendResults;
      var loadedResults = false;
      if (sendResults)
      {
        Instance.GsaModel.StreamSendConfig = ResultSettings.SendResults ? StreamContentConfig.ModelAndResults : StreamContentConfig.ModelOnly;
        Instance.GsaModel.Result1DNumPosition = ResultSettings.Additional1DPositions; //end points (2) plus additional
        Instance.GsaModel.ResultInLocalAxis = ResultSettings.UseLocalAxis;

        var resultsToSend = ResultSettings.ResultSettingItems.Where(rsi => rsi.Selected).ToList();

        if (resultsToSend != null && resultsToSend.Count() > 0 && !string.IsNullOrEmpty(ResultSettings.CasesDescription)
          && (Instance.GsaModel.ResultCases == null || Instance.GsaModel.ResultCases.Count() == 0))
        {
          try
          {
            progress.Report.Log("Preparing results");
            var analIndices = new List<int>();
            var comboIndices = new List<int>();
            if (((GsaCache)Instance.GsaModel.Cache).GetNatives<GsaAnal>(out var analRecords) && analRecords != null && analRecords.Count() > 0)
            {
              analIndices.AddRange(analRecords.Select(r => r.Index.Value));
            }
            if (((GsaCache)Instance.GsaModel.Cache).GetNatives<GsaCombination>(out var comboRecords) && comboRecords != null && comboRecords.Count() > 0)
            {
              comboIndices.AddRange(comboRecords.Select(r => r.Index.Value));
            }

            var description = String.IsNullOrEmpty(ResultSettings.CasesDescription) ? "all" : ResultSettings.CasesDescription.ToUpper();
            var expanded = ((GsaProxy)Instance.GsaModel.Proxy).ExpandLoadCasesAndCombinations(description, analIndices, comboIndices);
            if (expanded != null && expanded.Count() > 0)
            {
              progress.Report.Log("Resolved load cases");

              Instance.GsaModel.ResultCases = expanded;

              var resultTypes = new List<ResultType>();

              foreach (var result in resultsToSend)
              {
                if (Enum.TryParse(result.ResultType, out ResultType resultType))
                {
                  resultTypes.Add(resultType);
                }
              }
              Instance.GsaModel.ResultTypes = resultTypes;

              loadedResults = true;
            } else
            {
              progress.Report.LogOperationError(new Exception("Unable to resolve load cases - does the file have results?"));
            }
          }
          catch
          {

          }
        }
      }

      TimeSpan duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of reading GSA model into cache: " + duration.ToString(@"hh\:mm\:ss"));
        Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "timeToHydrateCache", duration.ToString(@"hh\:mm\:ss") } });
      }
      startTime = DateTime.Now;
      
      if (sendResults && loadedResults)
      {
        try
        {
          loadedResults = Instance.GsaModel.Proxy.PrepareResults(Instance.GsaModel.ResultTypes);
          foreach (var rg in Instance.GsaModel.ResultGroups)
          {
            ((GsaProxy)Instance.GsaModel.Proxy).LoadResults(rg, out int numErrorRows, Instance.GsaModel.ResultCases);
          }

          percentage += 20;

          duration = DateTime.Now - startTime;
          if (duration.Seconds > 0)
          {
            progress.Report.Log("Duration of preparing results: " + duration.ToString(@"hh\:mm\:ss"));
          }

          //Instance.GsaModel.ResultCases = null;
        }
        catch
        {
          progress.Report.LogOperationError(new Exception("Failed to load results from file"));
        }
        startTime = DateTime.Now;
      }

      var numToConvert = ((GsaCache)Instance.GsaModel.Cache).NumNatives;

      var conversionProgressDict = new ConcurrentDictionary<string, int>();
      conversionProgressDict["Conversion"] = 0;
      progress.Max = numToConvert;

      int numConverted = 0;
      int totalConversionPercentage = 80 - percentage;
      Instance.GsaModel.ConversionProgress = new Progress<bool>((bool success) =>
      {
        lock (perecentageProgressLock)
        {
          numConverted++;
        }

        conversionProgressDict["Conversion"]++;
        progress.Update(conversionProgressDict);
      });

      List<Base> objs = null;
      try
      {
        objs = Commands.ConvertToSpeckle(converter, selectedObjects);
      }
      catch (Exception ex)
      {

      }

      Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "numConverted", numConverted } });

      progress.Report.Merge(converter.Report);

      if (progress.Report.ConversionErrors != null && converter.Report.ConversionErrors.Count > 0)
      {
        foreach (var ce in converter.Report.ConversionErrors)
        {
          progress.Report.LogConversionError(ce);
        }
      }

      if (objs == null)
      {
        progress.Report.LogConversionError(new Exception("Failed to convert cache data to Speckle"));
        return null;
      }

      progress.Report.Log("Converted cache data to Speckle");

      duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of conversion to Speckle: " + duration.ToString(@"hh\:mm\:ss"));
        Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "timeToConvertToSpeckle", duration.ToString(@"hh\:mm\:ss") } });
      }
      startTime = DateTime.Now;

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
        return null;

      //The converter itself can't give anything back other than Base objects, so this is the first time it can be adorned with any
      //info useful to the sending in streams
      progress.Report.Log($"Sending to server: {state.ServerUrl}");

      var commitObj = new Base();
      var resultsObj = new Base();
      foreach (var obj in objs)
      {
        var typeName = obj.GetType().Name;
        string name = "";
        if (typeName.ToLower().Contains("model"))
        {
          try
          {
            name = string.Join(" ", (string)obj["layerDescription"], "Model");
          }
          catch
          {
            name = typeName;
          }
        }
        else if (typeName.ToLower().Contains("result"))
        {
          name = "Results";
          if (ResultSettings.UseLocalTransport)
          {
            resultsObj['@' + name] = obj;
          }
          if (!ResultSettings.UseServerTransport)
          {
            continue;
          }
        }

        commitObj['@' + name] = obj;
      }

      var serverTransport = new ServerTransport(account, state.StreamId);
      var sent = await Commands.SendCommit(commitObj, state, progress, ((GsaModel)Instance.GsaModel).LastCommitId, serverTransport);

      if (!String.IsNullOrEmpty(sent))
      {
        progress.Report.Log("Successfully sent data to stream");
        Commands.UpsertSavedReceptionStreamInfo(true, null, state);
        Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "totalChildrenCountSentObject", commitObj.GetTotalChildrenCount() } });

        duration = DateTime.Now - startTime;
        if (duration.Seconds > 0)
        {
          progress.Report.Log("Duration of sending to Speckle server: " + duration.ToString(@"hh\:mm\:ss"));
          Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "timeToSend", duration.ToString(@"hh\:mm\:ss") } });
        }
        startTime = DateTime.Now;
      }
      else
      {
        progress.Report.LogOperationError(new Exception("Unable to send data to stream"));
        return null;
      }

      if (sendResults && loadedResults)
      {
        if (ResultSettings.UseLocalTransport)
        {
          var basePath = System.IO.Path.Combine(GetDocumentLocation(), state.StreamId);
          var sqliteTransport = new SQLiteTransport(basePath, "SpeckleGSA", "Results");
          var sentLocal = await Commands.SendObject(resultsObj, progress, sqliteTransport);

          if (!String.IsNullOrEmpty(sentLocal))
          {
            progress.Report.Log($"Successfully sent results data to local SQLite file at: {System.IO.Path.Combine(basePath, "SpeckleGSA", "Results.db")}");

            duration = DateTime.Now - startTime;
            if (duration.Seconds > 0)
            {
              progress.Report.Log("Duration of sending to local SQLite file: " + duration.ToString(@"hh\:mm\:ss"));
              Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "timeToSendLocal", duration.ToString(@"hh\:mm\:ss") } });
            }
            startTime = DateTime.Now;
          }
          else
          {
            progress.Report.LogOperationError(new Exception("Unable to send results data to local SQLite file"));
          }
        }
        if (ResultSettings.SaveResultsToCsv)
        {
          var basePath = System.IO.Path.Combine(GetDocumentLocation(), state.StreamId, "SpeckleGSA", "GSAExport");
          var baseDir = System.IO.Directory.CreateDirectory(basePath);
          foreach (System.IO.FileInfo file in baseDir.GetFiles())
          {
            file.Delete();
          }

          var exportDir = ((GsaProxy)Instance.GsaModel.Proxy).resultDir;

          foreach (var rg in Instance.GsaModel.ResultGroups)
          {
            var res = Enum.GetName(typeof(ResultGroup), rg).ToLower();
            var fileName = $"result_{res}.csv";
            var filePath = System.IO.Path.Combine(exportDir, $"result_{res}", fileName);
            var exists = System.IO.File.Exists(filePath);
            if (exists)
            {
              var exportFile = System.IO.Path.Combine(basePath, fileName);
              try
              {
                System.IO.File.Copy(filePath, exportFile, true);
              }
              catch
              {
                progress.Report.LogOperationError(new Exception("Unable to copy raw GSA results export (csv file)"));
              }
            }
          }

          progress.Report.Log($"Raw GSA results export (csv files) saved at: {basePath}");
        }

      }

      return sent;
    }
  }
}
