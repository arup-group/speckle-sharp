using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;
using ReactiveUI;
using Speckle.Core.Api;


namespace DesktopUI2.Views.Pages
{
  public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
  {
    public SettingsView()
    {
      InitializeComponent();
      Instance = this;
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);

    }

    public static SettingsView Instance { get; private set; }
  }
}
