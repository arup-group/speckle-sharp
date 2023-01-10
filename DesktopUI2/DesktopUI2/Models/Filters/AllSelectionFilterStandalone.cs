//using DesktopUI2.Views.Filters;
//using System;
//using System.Collections.Generic;

//namespace DesktopUI2.Models.Filters
//{
//  //public class AllSelectionFilterStandalone : AllSelectionFilter
//  //{
//  //  new public string Type => typeof(AllSelectionFilterStandalone).ToString();
//  //  new public Type ViewType { get; } = typeof(AllFilterViewStandalone);

//  //}

//  public class AllSelectionFilterStandalone : ISelectionFilter
//  {
//    public string Type => typeof(AllSelectionFilterStandalone).ToString();
//    public string Name { get; set; }
//    public string Slug { get; set; }
//    public string Icon { get; set; }
//    public string Description { get; set; }
//    public List<string> Selection { get; set; } = new List<string>();
//    public Type ViewType { get; } = typeof(AllFilterViewStandalone);

//    public string Summary
//    {
//      get
//      {
//        return "everything";
//      }
//    }
//  }
//}
