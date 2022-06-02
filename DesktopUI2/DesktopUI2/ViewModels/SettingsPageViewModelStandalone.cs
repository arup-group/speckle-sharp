//using ReactiveUI;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reactive;

//namespace DesktopUI2.ViewModels
//{
//  public class SettingsPageViewModelStandalone : SettingsPageViewModel, IRoutableViewModel
//  {
//    new public IScreen HostScreen { get; }
//    new public ReactiveCommand<Unit, Unit> GoBack => MainWindowViewModelStandalone.RouterInstance.NavigateBack;

//    private StreamViewModelStandalone _streamViewModel;

//    private List<SettingViewModel> _settings;
//    new public List<SettingViewModel> Settings
//    {
//      get => _settings;
//      private set => this.RaiseAndSetIfChanged(ref _settings, value);
//    }

//    public SettingsPageViewModelStandalone(IScreen screen, List<SettingViewModel> settings, StreamViewModelStandalone streamViewModel) : base()
//    {
//      HostScreen = screen;
//      Settings = settings;
//      _streamViewModel = streamViewModel;
//    }

//    new public void SaveCommand()
//    {
//      _streamViewModel.Settings = Settings.Select(x => x.Setting).ToList();

//      MainWindowViewModelStandalone.RouterInstance.NavigateBack.Execute();
//    }

//  }
//}
