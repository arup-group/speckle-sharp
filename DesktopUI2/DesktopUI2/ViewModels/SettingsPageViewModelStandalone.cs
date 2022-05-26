using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace DesktopUI2.ViewModels
{
  public class SettingsPageViewModelStandalone : SettingsPageViewModel, IRoutableViewModel
  {
    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, Unit> GoBack => MainWindowViewModelStandalone.RouterInstance.NavigateBack;

    private StreamViewModel _streamViewModel;

    private List<SettingViewModel> _settings;
    public List<SettingViewModel> Settings
    {
      get => _settings;
      private set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    public SettingsPageViewModelStandalone(IScreen screen, List<SettingViewModel> settings, StreamViewModel streamViewModel) : base(screen, settings, streamViewModel)
    {
      HostScreen = screen;
      Settings = settings;
      _streamViewModel = streamViewModel;
    }

    public void SaveCommand()
    {
      _streamViewModel.Settings = Settings.Select(x => x.Setting).ToList();

      MainWindowViewModelStandalone.RouterInstance.NavigateBack.Execute();
    }

  }
}
