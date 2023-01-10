//using Avalonia.Metadata;
//using DesktopUI2.Models;
//using DesktopUI2.Models.Scheduler;
//using DesktopUI2.Views.Windows;
//using ReactiveUI;
//using Speckle.Core.Logging;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;

//namespace DesktopUI2.ViewModels
//{
//  public class ResultsViewModelStandalone : ReactiveObject
//  {        
//    private StreamViewModel _streamViewModel;

//    public ResultSettings ResultSettings { get; set; }

//    public ResultsViewModelStandalone() { }

//    public ResultsViewModelStandalone(StreamViewModel streamViewModel)
//    {
//      _streamViewModel = streamViewModel;
//      ResultSettings = _streamViewModel.ResultSettings;
//    }

//    private void SaveCommand()
//    {
//      try
//      {
//        _streamViewModel.ResultSettings = ResultSettings;
//        ResultsStandalone.Instance.Close(null);
//      }
//      catch (Exception ex)
//      {

//      }
//    }
//  }
//}
