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

namespace ConnectorGSA.UI
{
  public partial class ConnectorBindingsGSA : ConnectorBindings
  {
    const string MappingStream = "Default Section Mapping Stream";

    public override List<ISetting> GetSettings()
    {
      List<string> mappingStream = new List<string>() { MappingStream };

      return new List<ISetting>
      {
        new ListBoxSetting {Slug = "section-mapping", Name = "Section Mapping", Icon ="Repeat", Values = mappingStream, Description = "Sends or receives structural stick objects (ex. columns, beams) using the section name-family/family type mappings contained in this stream"}
      }; ;
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
