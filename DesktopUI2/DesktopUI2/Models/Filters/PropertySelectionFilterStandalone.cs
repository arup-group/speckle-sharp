using DesktopUI2.Views.Filters;
using System;
using System.Collections.Generic;

namespace DesktopUI2.Models.Filters
{
  //public class PropertySelectionFilterStandalone : PropertySelectionFilter
  //{
  //  new public string Type => typeof(PropertySelectionFilterStandalone).ToString();

  //  new public Type ViewType { get; } = typeof(PropertyFilterViewStandalone);

  //}


  public class PropertySelectionFilterStandalone : ISelectionFilter
  {
    public string Type => typeof(PropertySelectionFilterStandalone).ToString();
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public List<string> Selection { get; set; } = new List<string>();
    public List<string> Values { get; set; }
    public List<string> Operators { get; set; }
    public string PropertyName { get; set; }
    public string PropertyValue { get; set; }
    public string PropertyOperator { get; set; }

    public Type ViewType { get; } = typeof(PropertyFilterViewStandalone);

    public string Summary
    {
      get
      {
        return $"{PropertyName} {PropertyOperator} {PropertyValue}";
      }
    }
  }
}
