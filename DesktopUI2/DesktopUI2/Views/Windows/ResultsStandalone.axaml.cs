using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;


namespace DesktopUI2.Views.Windows
{
  public partial class ResultsStandalone : ReactiveWindow<ResultsViewModelStandalone>
  {
    public static ResultsStandalone Instance { get; private set; }

    public ResultsStandalone()
    {
      AvaloniaXamlLoader.Load(this);
      Instance = this;
#if DEBUG
      this.AttachDevTools();
#endif
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
