using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using ConnectorRevit;
using DesktopUI2.Models.Settings;
using Speckle.ConnectorRevit.Storage;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using DesktopUI2.Models;
using DesktopUI2.ViewModels;
using RevitElement = Autodesk.Revit.DB.Element;

namespace Speckle.ConnectorRevit.UI
{
  public partial class ConnectorBindingsRevit2
  {
    // CAUTION: these strings need to have the same values as in the converter
    const string InternalOrigin = "Internal Origin (default)";
    const string ProjectBase = "Project Base";
    const string Survey = "Survey";

    const string MappingStream = "Default Section Mapping Stream";

    public override List<ISetting> GetSettings()
    {
      List<string> referencePoints = new List<string>() { InternalOrigin };

      // find project base point and survey point. these don't always have name props, so store them under custom strings
      var basePoint = new FilteredElementCollector(CurrentDoc.Document).OfClass(typeof(BasePoint)).Cast<BasePoint>().Where(o => o.IsShared == false).FirstOrDefault();
      if (basePoint != null)
        referencePoints.Add(ProjectBase);
      var surveyPoint = new FilteredElementCollector(CurrentDoc.Document).OfClass(typeof(BasePoint)).Cast<BasePoint>().Where(o => o.IsShared == true).FirstOrDefault();
      if (surveyPoint != null)
        referencePoints.Add(Survey);

      List<string> mappingStream = new List<string>() { MappingStream };

      return new List<ISetting>
      {
        new ListBoxSetting {Slug = "reference-point", Name = "Reference Point", Icon ="LocationSearching", Values = referencePoints, Description = "Sends or receives stream objects in relation to this document point"},
        new CheckBoxSetting {Slug = "linkedmodels-send", Name = "Send Linked Models", Icon ="Link", IsChecked= false, Description = "Include Linked Models in the selection filters when sending"},
        new CheckBoxSetting {Slug = "linkedmodels-receive", Name = "Receive Linked Models", Icon ="Link", IsChecked= false, Description = "Include Linked Models when receiving NOTE: elements from linked models will be received in the current document"},
        new ListBoxSetting {Slug = "section-mapping", Name = "Section Mapping", Icon ="Repeat", Values = mappingStream, Description = "Sends or receives structural stick objects (ex. columns, beams) using the section name-family/family type mappings contained in this stream"}
      };
    }

    public async Task<string> GetSectionMappingData(StreamState state, ProgressViewModel progress)
    {
      const string mappingsStreamId = "e53a0242be";

      const string mappingsBranch = "mappings";
      const string sectionBranchPrefix = "sections";
      var key = $"{state.Client.Account.id}-{mappingsStreamId}";

      var mappingsTransport = new ServerTransport(state.Client.Account, mappingsStreamId);
      var mappingsTransportLocal = new SQLiteTransport(null, "Speckle", "Mappings");

      var mappingsStream = await state.Client.StreamGet(mappingsStreamId);
      var branches = await state.Client.StreamGetBranches(progress.CancellationTokenSource.Token, mappingsStreamId);

      foreach (var branch in branches)
      {
        if (branch.name == mappingsBranch || branch.name.StartsWith(sectionBranchPrefix))
        {
          var mappingsCommit = branch.commits.items.FirstOrDefault();
          var referencedMappingsObject = mappingsCommit.referencedObject;

          var mappingsCommitObject = await Operations.Receive(
            referencedMappingsObject,
            progress.CancellationTokenSource.Token,
            mappingsTransport,
            onProgressAction: dict => { },
            onErrorAction: (s, e) =>
            {
              progress.Report.LogOperationError(e);
              progress.CancellationTokenSource.Cancel();
            },
            disposeTransports: true
            );

          var hash = $"{key}-{branch.name}";
          var existingObjString = mappingsTransportLocal.GetObject(hash);
          if (existingObjString != null)
            mappingsTransportLocal.UpdateObject(hash, JsonConvert.SerializeObject(mappingsCommitObject));
          else
            mappingsTransportLocal.SaveObject(hash, JsonConvert.SerializeObject(mappingsCommitObject));
        }
      }
      return key;
    }
  }
}
