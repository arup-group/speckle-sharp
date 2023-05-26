using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace DesktopUI2.Views.Pages
{
  public partial class StreamEditViewStandalone : ReactiveUserControl<StreamViewModel>
  {
    public ItemsRepeater ResultLog => this.FindControl<ItemsRepeater>("ReportLog");
    public StreamEditViewStandalone()
    {
      InitializeComponent();
      Instance = this;

      this.WhenActivated(disposables =>
      {
        var updateReportCmd = new UpdateReportCommand(this.ViewModel, ResultLog);

        this.WhenAnyValue(x => x.ViewModel.Progress.IsProgressing).InvokeCommand(updateReportCmd).DisposeWith(disposables);
      });
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);

    }
    public static StreamEditViewStandalone Instance { get; private set; }

  }

  internal class UpdateReportCommand : ICommand
  {
    private readonly StreamViewModel _streamViewModel;
    private readonly ItemsRepeater _itemsControl;

    public UpdateReportCommand(StreamViewModel streamViewModel, ItemsRepeater itemsControl)
    {
      _streamViewModel = streamViewModel;
      _itemsControl = itemsControl;
    }

    public event System.EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return _streamViewModel != null && !string.IsNullOrEmpty(_streamViewModel.Log);
    }

    public void Execute(object parameter)
    {
      _itemsControl.Items = _streamViewModel.Log.Split('\n');
    }
  }
}
