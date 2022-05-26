using System;
using System.IO;
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
using Speckle.GSA.API;
using ConnectorGSA.Models;
using Speckle.ConnectorGSA.Proxy;

namespace ConnectorGSA.UI
{
  public partial class ConnectorBindingsGSA : ConnectorBindingsStandalone
  {
    public GsaModel Model;

    public List<Exception> ConversionErrors { get; set; } = new List<Exception>();

    /// <summary>
    /// Keeps track of errors in the operations of send/receive.
    /// </summary>
    public List<Exception> OperationErrors { get; set; } = new List<Exception>();
    public ConnectorBindingsGSA() : base()
    {
      Model = new GsaModel();
    }

    private string _layer = "Design";
    public override string Layer
    {
      get => _layer;
      set
      {
        _layer = value;
        var layer = Enum.TryParse(value, out GSALayer streamLayer);
        if (layer)
          Instance.GsaModel.StreamLayer = streamLayer;
      }
    }

    private string _units = "millimetres";
    public override string Units
    {
      get => _units;
      set
      {
        _units = value;
        var unit = Enum.TryParse(value, out Models.GsaUnit units);
        if (unit)
          Instance.GsaModel.Units = Commands.UnitEnumToString(units);
      }
    }

    private double _coincidentNodeAllowance = 10;
    public override double CoincidentNodeAllowance
    {
      get => _coincidentNodeAllowance;
      set
      {
        _coincidentNodeAllowance = value;
        Instance.GsaModel.CoincidentNodeAllowance = value;
      }
    }

    public override string GetHostAppNameVersion() => VersionedHostApplications.GSA;
    public override string GetHostAppName() => HostApplications.GSA.Slug;
    public override string GetDocumentId() => GetDocHash();
    private string GetDocHash() => Speckle.Core.Models.Utilities.hashString(((GsaProxy)Model?.Proxy).FilePath, Speckle.Core.Models.Utilities.HashingFuctions.MD5);
    public override string GetDocumentLocation() => Path.GetDirectoryName(((GsaProxy)Model?.Proxy).FilePath); // CurrentDoc.Document.PathName;
    public override string GetFileName() => Path.GetFileName(((GsaProxy)Model?.Proxy).FilePath); // CurrentDoc.Document.Title;
    public override string GetActiveViewName()
    {
      return "Entire Document"; // Note: gsa does not have views that filter objects.
    }


    public override List<StreamState> GetStreamsInFile()
    {
      return new List<StreamState>() { };
    }


    public override async void WriteStreamsToFile(List<StreamState> streams)
    {
      foreach (var state in streams)
      {
        Commands.UpsertSavedReceptionStreamInfo(true, null, state);
      }
    }


    public override List<MenuItem> GetCustomStreamMenuItems()
    {
      return new List<MenuItem>();
    }

    public override void NewFile()
    {
      Commands.NewFile();
    }

    public override void OpenFile(string filePath)
    {
      Commands.OpenFile(filePath, true);
    }

  }
}
