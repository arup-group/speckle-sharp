using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;

namespace DesktopUI2.Views.Pages
{
  public partial class StreamEditViewStandalone : ReactiveUserControl<StreamViewModelStandalone>
  {
    public StreamEditViewStandalone()
    {
      InitializeComponent();
      Instance = this;
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);

    }

    public static StreamEditViewStandalone Instance { get; private set; }

  }
}
