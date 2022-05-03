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
  public partial class ConnectorBindingsGSA : ConnectorBindingsStandalone
  {
    public override List<string> GetSelectedObjects()
    {
      return new List<string>();
    }

    public override List<ISelectionFilter> GetSelectionFilters()
    {
      var types = new List<string> { "Node", "Element1D", "Element2D", "Member1D", "Member2D" };

      return new List<ISelectionFilter>()
      {
        new AllSelectionFilter { Slug ="all", Name = "Everything", Icon = "CubeScan", Description = "Selects all model objects." },
        new ListSelectionFilter { Slug ="type", Name = "Type", Icon = "Category", Values = types, Description ="Selects all objects belonging to the specified categories."},
        new ManualSelectionFilter()
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

    public override void SelectClientObjects(string args)
    {
      throw new NotImplementedException();
    }
  }
}
