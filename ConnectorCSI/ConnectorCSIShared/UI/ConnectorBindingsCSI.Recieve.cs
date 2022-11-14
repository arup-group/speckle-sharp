using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.ViewModels;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Speckle.Core.Logging;

namespace Speckle.ConnectorCSI.UI
{
  public partial class ConnectorBindingsCSI : ConnectorBindings
  {
    public override bool CanPreviewReceive => false;
    public override Task<StreamState> PreviewReceive(StreamState state, ProgressViewModel progress)
    {
      return null;
    }

    public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
    {
      Exceptions.Clear();

      var kit = KitManager.GetDefaultKit();
      var appName = GetHostAppVersion(Model);
      var converter = kit.LoadConverter(appName);
      if (converter == null)
      {
        progress.Report.LogOperationError(new SpeckleException("Could not find any Kit!"));
        return null;
      }

      converter.SetContextDocument(Model);
      converter.ReceiveMode = state.ReceiveMode;
      Exceptions.Clear();

      var stream = await state.Client.StreamGet(state.StreamId);

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
        return null;

      var transport = new ServerTransport(state.Client.Account, state.StreamId);

      Exceptions.Clear();

      Commit commit = null;
      if (state.CommitId == "latest")
      {
        var res = await state.Client.BranchGet(progress.CancellationTokenSource.Token, state.StreamId, state.BranchName, 1);
        commit = res.commits.items.FirstOrDefault();
      }
      else
      {
        commit = await state.Client.CommitGet(progress.CancellationTokenSource.Token, state.StreamId, state.CommitId);
      }
      string referencedObject = commit.referencedObject;

      state.LastSourceApp = commit.sourceApplication;

      var commitObject = await Operations.Receive(
                referencedObject,
                progress.CancellationTokenSource.Token,
                transport,
                onProgressAction: dict => progress.Update(dict),
                onErrorAction: (Action<string, Exception>)((s, e) =>
                {
                  progress.Report.LogOperationError(new Core.Logging.SpeckleException(e.Message, true, Sentry.SentryLevel.Error));
                  Core.Logging.Analytics.TrackEvent(state.Client.Account, Core.Logging.Analytics.Events.Receive, new Dictionary<string, object>() { { "commit_receive_failed", e.Message } });
                  progress.CancellationTokenSource.Cancel();
                }),
                onTotalChildrenCountKnown: count => { progress.Max = count + 1; },
                disposeTransports: true
                );

      if (progress.Report.OperationErrorsCount != 0)
        return state;

      try
      {
        await state.Client.CommitReceived(new CommitReceivedInput
        {
          streamId = stream?.id,
          commitId = commit?.id,
          message = commit?.message,
          sourceApplication = GetHostAppVersion(Model)
        });
      }
      catch
      {
        // Do nothing!
      }

      //progress.Report = new ProgressReport();
      //var conversionProgressDict = new ConcurrentDictionary<string, int>();
      //conversionProgressDict["Conversion"] = 0;




      var commitObjs = FlattenCommitObject(commitObject, converter);
      //progress.Max = commitObjs.Count();

      foreach (var commitObj in commitObjs)
      {
        try
        {
          BakeObject(commitObj, converter);
          //Execute.PostToUIThread(() =>
          //{
            //conversionProgressDict["Conversion"]++;
            //progress.Update(conversionProgressDict);
          //});
        }
        catch (Exception e)
        {
          progress.Report.LogOperationError(e);
        }
      }

      progress.Report.Merge(converter.Report);
      //var c = converter as Objects.Converter.CSI.ConverterCSI;
      //c.Model.View.RefreshView();

      if (progress.Report.OperationErrorsCount != 0)
        return null;

      try
      {
        //await state.RefreshStream();
        WriteStateToFile();
      }
      catch (Exception e)
      {
        WriteStateToFile();
        progress.Report.LogOperationError(e);
      }

      return state;
    }

    /// <summary>
    /// conversion to native
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="state"></param>
    /// <param name="converter"></param>
    private void BakeObject(Base obj, ISpeckleConverter converter)
    {
      try
      {
        converter.ConvertToNative(obj);
      }
      catch (Exception e)
      {
        //var exception = new Exception($"Failed to convert object {obj.id} of type {obj.speckle_type}\n with error\n{e}");
        converter.Report.LogConversionError(e);
        return;
      }
    }

    /// <summary>
    /// Recurses through the commit object and flattens it. 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="converter"></param>
    /// <returns></returns>
    private List<Base> FlattenCommitObject(object obj, ISpeckleConverter converter)
    {
      List<Base> objects = new List<Base>();

      if (obj is Base @base)
      {
        if (converter.CanConvertToNative(@base))
        {
          objects.Add(@base);
          return objects;
        }
        else
        {
          foreach (var prop in @base.GetDynamicMembers())
            objects.AddRange(FlattenCommitObject(@base[prop], converter));
          return objects;
        }
      }

      if (obj is List<object> list)
      {
        foreach (var listObj in list)
          objects.AddRange(FlattenCommitObject(listObj, converter));
        return objects;
      }

      if (obj is IDictionary dict)
      {
        foreach (DictionaryEntry kvp in dict)
          objects.AddRange(FlattenCommitObject(kvp.Value, converter));
        return objects;
      }

      return objects;
    }

  }
}