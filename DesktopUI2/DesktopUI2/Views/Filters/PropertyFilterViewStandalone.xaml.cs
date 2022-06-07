using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;

namespace DesktopUI2.Views.Filters
{
  public partial class PropertyFilterViewStandalone : ReactiveUserControl<FilterViewModelStandalone>
  {
    public PropertyFilterViewStandalone()
    {
      InitializeComponent();
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
