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
using Speckle.ConnectorGSA.Proxy;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using Speckle.Newtonsoft.Json;
using static DesktopUI2.ViewModels.MappingViewModel;


namespace ConnectorGSA.UI
{
  public partial class ConnectorBindingsGSA : ConnectorBindings, IConnectorBindingsStandalone
  {
    public override List<string> GetSelectedObjects()
    {
      return new List<string>();
    }


    public override List<ISelectionFilter> GetSelectionFilters()
    {
      var types = new List<string> { "Node", "Element1D", "Element2D", "Member1D", "Member2D" };

      ((GsaProxy)Instance.GsaModel.Proxy).GetGwaListData(GSALayer.Both, out var records);

      var listNames = new List<string>();

      records.ForEach(record => listNames.Add(record.Name));

      return new List<ISelectionFilter>()
      {
        //new AllSelectionFilterStandalone { Slug ="all", Name = "Everything", Icon = "CubeScan", Description = "Selects all model objects." },
        new AllSelectionFilter { Slug ="all", Name = "Everything", Icon = "CubeScan", Description = "Selects all model objects." },
        //new ListSelectionFilter { Slug ="type", Name = "Type", Icon = "Category", Values = types, Description ="Selects all objects belonging to the specified categories."},
        new ListSelectionFilter { Slug ="list", Name = "List", Icon = "LayersTriple", Values = listNames, Description ="Selects all objects belonging to the specified list."},
        //new ListSelectionFilterStandalone { Slug ="list", Name = "List", Icon = "LayersTriple", Values = listNames, Description ="Selects all objects belonging to the specified list."},
        //new ManualSelectionFilter()
      };
    }

    private List<string> GetSelectionFilterObjects(ISelectionFilter filter)
    {
      var selection = new List<string>();
      try
      {
        switch (filter.Slug)
        {
          case "manual":
            selection.Add("everything");
            return selection;

          case "all":
            selection.Add("everything");
            return selection;

          case "type":
            selection.Add("everything");
            return selection;

          case "layer":
            selection.Add("everything");
            return selection;

          case "list":
            return filter.Selection;
        }
      }
      catch (Exception e)
      {

      }

      return selection;

    }

    public override List<string> GetObjectsInView()
    {
      return new List<string>();
    }

    public override void SelectClientObjects(List<string> objs, bool deselect = false)
    {
      throw new NotImplementedException();
    }

    public override async Task<Dictionary<string, List<MappingValue>>> ImportFamilyCommand(Dictionary<string, List<MappingValue>> Mapping)
    {
      return new Dictionary<string, List<MappingValue>>();
    }
  }
}
