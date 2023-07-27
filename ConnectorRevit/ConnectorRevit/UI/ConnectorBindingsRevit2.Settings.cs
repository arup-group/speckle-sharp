﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DesktopUI2.Models.Settings;
using Speckle.Core.Api;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using DesktopUI2.Models;
using DesktopUI2.ViewModels;

namespace Speckle.ConnectorRevit.UI
{
  public partial class ConnectorBindingsRevit
  {
    // CAUTION: these strings need to have the same values as in the converter
    const string InternalOrigin = "Internal Origin (default)";
    const string ProjectBase = "Project Base";
    const string Survey = "Survey";

    const string MappingStream = "Default Section Mapping Stream";

    const string defaultValue = "Default";
    const string dxf = "DXF";
    const string familyDxf = "Family DXF";

    const string StructuralFraming = "Structural Framing";
    const string StructuralWalls = "Structural Walls";
    const string ArchitecturalWalls = "Achitectural Walls";

    const string noMapping = "Never (default)";
    const string everyReceive = "Always";
    const string forNewTypes = "For New Types";

    public override List<ISetting> GetSettings()
    {
      List<string> referencePoints = new List<string>() { InternalOrigin };
      List<string> prettyMeshOptions = new List<string>() { defaultValue, dxf, familyDxf };
      List<string> mappingOptions = new List<string>() { noMapping, everyReceive, forNewTypes };

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
        new ListBoxSetting {Slug = "reference-point", Name = "Reference Point", Icon ="LocationSearching", Values = referencePoints, Selection = InternalOrigin, Description = "Sends or receives stream objects in relation to this document point"},
        new CheckBoxSetting {Slug = "linkedmodels-send", Name = "Send Linked Models", Icon ="Link", IsChecked= false, Description = "Include Linked Models in the selection filters when sending"},
        new CheckBoxSetting {Slug = "linkedmodels-receive", Name = "Receive Linked Models", Icon ="Link", IsChecked= false, Description = "Include Linked Models when receiving NOTE: elements from linked models will be received in the current document"},
        new ListBoxSetting {Slug = "section-mapping", Name = "Section Mapping", Icon ="Repeat", Values = mappingStream, Description = "Sends or receives structural stick objects (ex. columns, beams) using the section name-family/family type mappings contained in this stream"},
        new CheckBoxSetting {Slug = "recieve-objects-mesh", Name = "Receive Objects as Direct Mesh", Icon = "Link", IsChecked = false, Description = "Recieve the stream as a Meshes only"},
        new MultiSelectBoxSetting { Slug = "disallow-join", Name = "Disallow Join For Elements", Icon = "CallSplit", Description = "Determine which objects should not be allowed to join by default when receiving",
          Values = new List<string>() { ArchitecturalWalls, StructuralWalls, StructuralFraming } },
        new ListBoxSetting {Slug = "pretty-mesh", Name = "Mesh Import Method", Icon ="ChartTimelineVarient", Values = prettyMeshOptions, Selection = defaultValue, Description = "Determines the display style of imported meshes"},
        new MappingSeting {Slug = "receive-mappings", Name = "Custom Type Mapping", Icon ="LocationSearching", Values = mappingOptions, Selection = noMapping, Description = "Determine how incoming object types are mapped to object types in the host application"},
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