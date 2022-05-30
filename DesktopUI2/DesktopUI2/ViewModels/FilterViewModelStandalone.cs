using Avalonia.Controls.Selection;
using DesktopUI2.Models.Filters;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopUI2.ViewModels
{
  public class FilterViewModelStandalone : FilterViewModel
  {
    new private ConnectorBindings Bindings;

    private ISelectionFilter _filter;

    new public ISelectionFilter Filter
    {
      get => _filter;
      set
      {
        this.RaiseAndSetIfChanged(ref _filter, value);
        RestoreSelectedItems();
        this.RaisePropertyChanged("Summary");
      }
    }

    new public SelectionModel<string> SelectionModel { get; }

    new public string Summary { get { return Filter.Summary; } }

    public FilterViewModelStandalone(ISelectionFilter filter) : base()
    {
      try
      {
        SelectionModel = new SelectionModel<string>();
        SelectionModel.SingleSelect = false;
        SelectionModel.SelectionChanged += SelectionChanged;

        //use dependency injection to get bindings
        Bindings = Locator.Current.GetService<ConnectorBindings>();

        Filter = filter;

        //TODO should clean up this logic a bit
        //maybe have a model, view and viewmodel for each filter
        if (filter is ListSelectionFilterStandalone l)
        {
          _valuesList = SearchResults = new List<string>(l.Values);
        }

        //TODO:
        //FilterView.DataContext = this;
      }
      catch (Exception ex)
      {

      }
    }

    #region LIST FILTER
    void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
    {
      try
      {
        if (!isSearching)
        {
          foreach (var a in e.SelectedItems)
            if (!Filter.Selection.Contains(a))
              Filter.Selection.Add(a as string);
          foreach (var r in e.DeselectedItems)
            Filter.Selection.Remove(r as string);

          this.RaisePropertyChanged("Summary");
        }
      }
      catch (Exception ex)
      {

      }
    }
    //private bool isSearching = false;
    //private string _searchQuery;
    //public string SearchQuery
    //{
    //  get => _searchQuery;
    //  set
    //  {
    //    isSearching = true;
    //    this.RaiseAndSetIfChanged(ref _searchQuery, value);

    //    SearchResults = new List<string>(_valuesList.Where(v => v.ToLower().Contains(SearchQuery.ToLower())).ToList());
    //    this.RaisePropertyChanged(nameof(SearchResults));
    //    isSearching = false;
    //    RestoreSelectedItems();

    //  }
    //}

    // searching will change data source and remove selected items in the ListBox, 
    // restore them as the query is cleared
    new public void RestoreSelectedItems()
    {
      try
      {
        var itemsToRemove = new List<string>();

        if (Filter.Type == typeof(ListSelectionFilterStandalone).ToString())
        {
          foreach (var item in Filter.Selection)
          {
            if (!_valuesList.Contains(item))
              itemsToRemove.Add(item);
            if (!SelectionModel.SelectedItems.Contains(item))
              SelectionModel.Select(SearchResults.IndexOf(item));
          }

          foreach (var itemToRemove in itemsToRemove)
            Filter.Selection.Remove(itemToRemove);
        }

        this.RaisePropertyChanged("PropertyName");
        this.RaisePropertyChanged("PropertyValue");
        this.RaisePropertyChanged("PropertyOperator");
      }
      catch (Exception ex)
      {

      }
    }

    //public List<string> SearchResults { get; set; } = new List<string>();
    private List<string> _valuesList { get; } = new List<string>();

    #endregion

    #region MANUAL FILTER
    new public void SetObjectSelection()
    {
      try
      {
        var objIds = Bindings.GetSelectedObjects();
        if (objIds == null || objIds.Count == 0)
        {
          //Globals.Notify("Could not get object selection.");
          return;
        }

        Filter.Selection = objIds;
        this.RaisePropertyChanged("Summary");
        //Globals.Notify("Object selection set.");
      }
      catch (Exception ex)
      {

      }
    }

    new public void AddObjectSelection()
    {
      try
      {
        var objIds = Bindings.GetSelectedObjects();
        if (objIds == null || objIds.Count == 0)
        {
          //Globals.Notify("Could not get object selection.");
          return;
        }

        objIds.ForEach(id =>
        {
          if (Filter.Selection.FirstOrDefault(x => x == id) == null)
          {
            Filter.Selection.Add(id);
          }
        });
        this.RaisePropertyChanged("Summary");
        //Globals.Notify("Object added.");
      }
      catch (Exception ex)
      {

      }
    }

    new public void RemoveObjectSelection()
    {
      try
      {
        var objIds = Bindings.GetSelectedObjects();
        if (objIds == null || objIds.Count == 0)
        {
          //Globals.Notify("Could not get object selection.");
          return;
        }

        var filtered = Filter.Selection.Where(o => objIds.IndexOf(o) == -1).ToList();

        if (filtered.Count == Filter.Selection.Count)
        {
          //Globals.Notify("No objects removed.");
          return;
        }

        //Globals.Notify($"{Filter.Selection.Count - filtered.Count} objects removed.");
        Filter.Selection = filtered;
        this.RaisePropertyChanged("Summary");
      }
      catch (Exception ex)
      {

      }
    }

    new public void ClearObjectSelection()
    {
      Filter.Selection = new List<string>();
      this.RaisePropertyChanged("Summary");
      //Globals.Notify($"Selection cleared.");
    }
    #endregion

    #region PROPERTY FILTER

    //not the cleanest way, but it works
    //should create proper view models for each!
    new public string PropertyName
    {
      get => (Filter as PropertySelectionFilterStandalone).PropertyName;
      set
      {
        (Filter as PropertySelectionFilterStandalone).PropertyName = value;
        this.RaisePropertyChanged("Summary");
      }
    }
    new public string PropertyValue
    {
      get => (Filter as PropertySelectionFilterStandalone).PropertyValue;
      set
      {
        (Filter as PropertySelectionFilterStandalone).PropertyValue = value;
        this.RaisePropertyChanged("Summary");
      }
    }
    new public string PropertyOperator
    {
      get => (Filter as PropertySelectionFilterStandalone).PropertyOperator;
      set
      {
        (Filter as PropertySelectionFilterStandalone).PropertyOperator = value;
        this.RaisePropertyChanged("Summary");
      }
    }


    #endregion

    new public bool IsReady()
    {
      if (Filter is ManualSelectionFilter && !Filter.Selection.Any())
        return false;

      if (Filter is ListSelectionFilterStandalone && !Filter.Selection.Any())
        return false;

      if (Filter is PropertySelectionFilterStandalone p)
      {
        if (string.IsNullOrEmpty(p.PropertyName) || string.IsNullOrEmpty(p.PropertyOperator) || string.IsNullOrEmpty(p.PropertyValue))
          return false;
      }

      return true;
    }
  }
}
