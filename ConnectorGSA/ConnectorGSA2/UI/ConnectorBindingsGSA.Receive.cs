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
  public partial class ConnectorBindingsGSA : ConnectorBindingsStandalone
  {
    public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(VersionedHostApplications.GSA);
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
          var fromStr = "mm"; // coordinator.ReceiverTab.CoincidentNodeUnits.GetStringValue();
          var toStr = lengthUnitData.Name;
          factor = (lengthUnitData == null) ? 1 : Speckle.Core.Kits.Units.GetConversionFactor(fromStr, toStr);
        }
      }
      Instance.GsaModel.CoincidentNodeAllowance = 0.05 * factor; // coordinator.ReceiverTab.CoincidentNodeAllowance * factor;

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

      var transport = new ServerTransport(account, state.StreamId);
      var stream = await state.Client.StreamGet(state.StreamId);

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
      {
        return null;
      }

      Commit myCommit = null;
      //if "latest", always make sure we get the latest commit when the user clicks "receive"
      if (state.CommitId == "latest")
      {
        var res = await state.Client.BranchGet(progress.CancellationTokenSource.Token, state.StreamId, state.BranchName, 1);
        myCommit = res.commits.items.FirstOrDefault();
      }
      else
      {
        myCommit = await state.Client.CommitGet(progress.CancellationTokenSource.Token, state.StreamId, state.CommitId);
      }
      string referencedObject = myCommit.referencedObject;

      var commitObject = await Operations.Receive(
          referencedObject,
          progress.CancellationTokenSource.Token,
          transport,
          onProgressAction: dict => progress.Update(dict),
          onErrorAction: (s, e) =>
          {
            progress.Report.LogOperationError(e);
            progress.CancellationTokenSource.Cancel();
          },
          onTotalChildrenCountKnown: count => { progress.Max = count; },
          disposeTransports: true
          );

      try
      {
        await state.Client.CommitReceived(new CommitReceivedInput
        {
          streamId = stream?.id,
          commitId = myCommit?.id,
          message = myCommit?.message, 
          sourceApplication = VersionedHostApplications.GSA
        });
      }
      catch
      {
        // Do nothing!
      }

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
      }

      Console.WriteLine("Receiving complete");

      return null;
    }
  }
}
