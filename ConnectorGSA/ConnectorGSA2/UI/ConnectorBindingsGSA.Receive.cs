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
    public override List<ReceiveMode> GetReceiveModes()
    {
      return new List<ReceiveMode> { ReceiveMode.Create };
    }

    public override bool CanPreviewReceive => false;
    public override async Task<StreamState> PreviewReceive(StreamState state, ProgressViewModel progress)
    {
      return null;
    }

    public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(HostApplications.GSA.Name);
      if (converter == null)
        throw new Exception("Could not find any Kit!");

      var percentage = 0;

      //Instance.GsaModel.StreamLayer = coordinator.ReceiverTab.TargetLayer;
      //Instance.GsaModel.Units = UnitEnumToString(coordinator.ReceiverTab.CoincidentNodeUnits);
      //Instance.GsaModel.LoggingMinimumLevel = (int)coordinator.LoggingMinimumLevel;

      var perecentageProgressLock = new object();
      var numToConvertProgressLock = new object();

      var client = state.Client;
      var account = client.Account;

      var mappingsStreamId = String.Empty;

      // set converter settings as tuples (setting slug, setting selection)
      var settings = new Dictionary<string, string>();
      foreach (var setting in state.Settings)
      {
        if (setting.Slug == "section-mapping")
        {
          if (setting.Selection != null)
          {
            var mappingKey = await GetSectionMappingData(state, progress);
            progress.Report.Log($"Using section mapping data from: {mappingKey}");
            setting.Selection = mappingKey;
          }
        }
        else if (setting.Slug == "node-coincidence-allowance")
        {
          if (double.TryParse(setting.Selection, out var allowance))
          {
            Instance.GsaModel.CoincidentNodeAllowance = allowance;
          }
        }
        else if (setting.Slug == "unit")
        {
          if (Enum.TryParse(setting.Selection, out Models.GsaUnit units))
          {
            Instance.GsaModel.Units = Commands.UnitEnumToString(units);
          }
        }
        settings.Add(setting.Slug, setting.Selection);
      }

      converter.SetConverterSettings(settings);

      var startTime = DateTime.Now;

      progress.Report.Log("Reading GSA data into cache");
      //Load data to cause merging
      Commands.LoadDataFromFile(progress);

      double factor = 1;
      if (Instance.GsaModel.Cache.GetNatives(typeof(GsaUnitData), out var gsaUnitDataRecords))
      {
        var lengthUnitData = (GsaUnitData)gsaUnitDataRecords.FirstOrDefault(r => ((GsaUnitData)r).Option == UnitDimension.Length);
        if (lengthUnitData != null)
        {
          var fromStr = Instance.GsaModel.Units; // coordinator.ReceiverTab.CoincidentNodeUnits.GetStringValue(); 
          var toStr = lengthUnitData.Name;
          factor = (lengthUnitData == null) ? 1 : Speckle.Core.Kits.Units.GetConversionFactor(fromStr, toStr);
        }
      }
      Instance.GsaModel.CoincidentNodeAllowance = Instance.GsaModel.CoincidentNodeAllowance * factor; // coordinator.ReceiverTab.CoincidentNodeAllowance * factor;

      percentage = 10;
      //percentageProgress.Report(percentage);

      TimeSpan duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Loaded data from file into cache");
        progress.Report.Log("Duration of reading GSA model into cache: " + duration.ToString(@"hh\:mm\:ss"));
      }
      startTime = DateTime.Now;

      progress.Report.Log("Accessing stream");

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
      {
        return null;
      }

      Commit myCommit = await ConnectorHelpers.GetCommitFromState(state, progress.CancellationToken);
      state.LastCommit = myCommit;
      Base commitObject = await ConnectorHelpers.ReceiveCommit(myCommit, state, progress);
      await ConnectorHelpers.TryCommitReceived(state, myCommit, HostApplications.GSA.Name, progress.CancellationToken);

      if (progress.Report.OperationErrorsCount != 0)
      {
        return state;
      }

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
      {
        return null;
      }

      duration = DateTime.Now - startTime;

      progress.Report.Log("Duration of reception from Speckle and scaling: " + duration.ToString(@"hh\:mm\:ss"));
      Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "commit_receive_time", duration.ToString(@"hh\:mm\:ss") } });

      startTime = DateTime.Now;

      progress.Report.Log("Converting");

      var flattenedGroups = Commands.FlattenCommitObject(commitObject, (Base o) => converter.CanConvertToNative(o));

      int numConverted = 0;
      int numToConvert = flattenedGroups.Count;
      int totalConversionPercentage = 90 - percentage;

      var conversionProgressDict = new ConcurrentDictionary<string, int>();
      conversionProgressDict["Conversion"] = 0;
      progress.Max = numToConvert;

      Instance.GsaModel.ConversionProgress = new Progress<bool>((bool success) =>
      {
        lock (perecentageProgressLock)
        {
          numConverted++;
        }
        conversionProgressDict["Conversion"]++;
        progress.Update(conversionProgressDict);
        //percentageProgress.Report(percentage + Math.Round(((double)numConverted / (double)numToConvert) * totalConversionPercentage, 0));
      });

      Commands.ConvertToNative(flattenedGroups, converter, progress);

      progress.Report.Merge(converter.Report);

      if (progress.Report.ConversionErrors != null && converter.Report.ConversionErrors.Count > 0)
      {
        foreach (var ce in converter.Report.ConversionErrors)
        {
          progress.Report.LogConversionError(ce);
        }
      }

      progress.Report.Log("Converted Speckle to GSA objects");

      duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of conversion from Speckle: " + duration.ToString(@"hh\:mm\:ss"));
        Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "commit_convert_time", duration.ToString(@"hh\:mm\:ss") } });
      }
      startTime = DateTime.Now;

      //The cache is filled with natives
      if (Instance.GsaModel.Cache.GetNatives(out var gsaRecords))
      {
        ((GsaProxy)Instance.GsaModel.Proxy).WriteModel(gsaRecords, progress);
      }

      ((GsaProxy)Instance.GsaModel.Proxy).UpdateCasesAndTasks();
      ((GsaProxy)Instance.GsaModel.Proxy).UpdateViews();

      duration = DateTime.Now - startTime;
      if (duration.Seconds > 0)
      {
        progress.Report.Log("Duration of writing converted objects to GSA: " + duration.ToString(@"hh\:mm\:ss"));
        Analytics.TrackEvent(account, Analytics.Events.GSA, new Dictionary<string, object>() { { "commit_write_time", duration.ToString(@"hh\:mm\:ss") } });
      }

      Console.WriteLine("Receiving complete");

      return state;
    }
  }
}
