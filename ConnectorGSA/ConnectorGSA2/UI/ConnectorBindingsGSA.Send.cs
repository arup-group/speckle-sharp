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
  public partial class ConnectorBindingsGSA : ConnectorBindings
  {
    public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(VersionedHostApplications.GSA);
      if (converter == null)
        throw new Exception("Could not find any Kit!");

      var account = state.Client.Account; //((GsaModel)Instance.GsaModel).Account;

      // set converter settings as tuples (setting slug, setting selection)
      var settings = new Dictionary<string, string>();
      foreach (var setting in state.Settings)
      {
        settings.Add(setting.Slug, setting.Selection);
      }
      converter.SetConverterSettings(settings);

      var selectedObjects = GetSelectionFilterObjects(state.Filter);
      //state.SelectedObjectIds = selectedObjects.Select(x => x.UniqueId).ToList();

      var percentage = 0;
      var perecentageProgressLock = new object();

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

      //var resultsToSend = coordinator.SenderTab.ResultSettings.ResultSettingItems.Where(rsi => rsi.Selected).ToList();
      //if (resultsToSend != null && resultsToSend.Count() > 0 && !string.IsNullOrEmpty(coordinator.SenderTab.LoadCaseList)
      //  && (Instance.GsaModel.ResultCases == null || Instance.GsaModel.ResultCases.Count() == 0))
      //{
      //  try
      //  {
      //    statusProgress.Report("Preparing results");
      //    var analIndices = new List<int>();
      //    var comboIndices = new List<int>();
      //    if (((GsaCache)Instance.GsaModel.Cache).GetNatives<GsaAnal>(out var analRecords) && analRecords != null && analRecords.Count() > 0)
      //    {
      //      analIndices.AddRange(analRecords.Select(r => r.Index.Value));
      //    }
      //    if (((GsaCache)Instance.GsaModel.Cache).GetNatives<GsaCombination>(out var comboRecords) && comboRecords != null && comboRecords.Count() > 0)
      //    {
      //      comboIndices.AddRange(comboRecords.Select(r => r.Index.Value));
      //    }
      //    var expanded = ((GsaProxy)Instance.GsaModel.Proxy).ExpandLoadCasesAndCombinations(coordinator.SenderTab.LoadCaseList, analIndices, comboIndices);
      //    if (expanded != null && expanded.Count() > 0)
      //    {
      //      loggingProgress.Report(new MessageEventArgs(MessageIntent.Display, MessageLevel.Information, "Resolved load cases"));

      //      Instance.GsaModel.ResultCases = expanded;
      //      Instance.GsaModel.ResultTypes = resultsToSend.Select(rts => rts.ResultType).ToList();
      //    }
      //  }
      //  catch
      //  {

      //  }
      //}

      TimeSpan duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of reading GSA model into cache: " + duration.ToString(@"hh\:mm\:ss"));
      }
      startTime = DateTime.Now;

      if (Instance.GsaModel.SendResults)
      {
        try
        {
          Instance.GsaModel.Proxy.PrepareResults(Instance.GsaModel.ResultTypes);
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
        }
        catch
        {

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
        objs = Commands.ConvertToSpeckle(converter);
      }
      catch (Exception ex)
      {

      }

      progress.Report.Merge(converter.Report);

      if (progress.Report.ConversionErrors != null && converter.Report.ConversionErrors.Count > 0)
      {
        foreach (var ce in converter.Report.ConversionErrors)
        {
          progress.Report.LogConversionError(ce);
        }
      }

      progress.Report.Log("Converted cache data to Speckle");

      duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of conversion to Speckle: " + duration.ToString(@"hh\:mm\:ss"));
      }
      startTime = DateTime.Now;

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
        return null;

      //The converter itself can't give anything back other than Base objects, so this is the first time it can be adorned with any
      //info useful to the sending in streams
      progress.Report.Log("Sending to Server");

      var commitObj = new Base();
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
        }

        commitObj['@' + name] = obj;
      }

      var serverTransport = new ServerTransport(account, state.StreamId);
      var sent = await Commands.SendCommit(commitObj, state, progress, ((GsaModel)Instance.GsaModel).LastCommitId, serverTransport);

      if (sent != null)
      {
        progress.Report.Log("Successfully sent data to stream");
        Commands.UpsertSavedReceptionStreamInfo(true, null, state);
      }
      else
      {
        progress.Report.LogOperationError(new Exception("Unable to send data to stream"));
      }

      if (progress.Report.ConversionErrorsCount > 0 && progress.Report.OperationErrorsCount > 0)
      {
        foreach (var se in progress.Report.ConversionErrors)
        {
          progress.Report.LogConversionError(se);
        }
        foreach (var se in progress.Report.OperationErrors)
        {
          progress.Report.LogOperationError(se);
        }
      }

      duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of sending to Speckle: " + duration.ToString(@"hh\:mm\:ss"));
      }
      startTime = DateTime.Now;

      Console.WriteLine("Sending complete");

      return null;
    }
  }
}
