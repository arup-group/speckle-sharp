using DesktopUI2.Views.Filters;
using System;
using System.Collections.Generic;

namespace DesktopUI2.Models.Filters
{
  //public class ListSelectionFilterStandalone : ListSelectionFilter
  //{
  //  new public string Type => typeof(ListSelectionFilterStandalone).ToString();
  //  new public Type ViewType { get; } = typeof(ListFilterViewStandalone);

  //}

  public class ListSelectionFilterStandalone : ISelectionFilter
  {
    public string Type => typeof(ListSelectionFilterStandalone).ToString();
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public List<string> Values { get; set; }
    public List<string> Selection { get; set; } = new List<string>();
    public Type ViewType { get; } = typeof(ListFilterViewStandalone);

    public string Summary
    {
      get
      {
        if (Selection.Count != 0)
        {
          return string.Join(", ", Selection);
        }
        else
        {
          return "nothing";
        }
      }
    }


  }

}
