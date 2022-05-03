using DesktopUI2.Views.Pages;
using ReactiveUI;
using Speckle.Core.Logging;
using Splat;
using System.Reactive;

namespace DesktopUI2.ViewModels
{
  public class MainWindowViewModelStandalone : ViewModelBase, IScreen
  {
    public string TitleFull => "Speckle for " + Bindings.GetHostAppNameVersion();
    public RoutingState Router { get; private set; }

    public ConnectorBindingsStandalone Bindings { get; private set; } = new DummyBindingsStandalone();

    public static RoutingState RouterInstance { get; private set; }

    public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;


    public MainWindowViewModelStandalone(ConnectorBindingsStandalone _bindings) : base()
    {
      Bindings = _bindings;
      Setup.Init(Bindings.GetHostAppNameVersion(), Bindings.GetHostAppName());
      Init();
    }
    public MainWindowViewModelStandalone()
    {
      Init();
    }

    private void Init()
    {
      Router = new RoutingState();

      Locator.CurrentMutable.Register(() => new StreamEditViewStandalone(), typeof(IViewFor<StreamViewModelStandalone>));
      Locator.CurrentMutable.Register(() => new HomeViewStandalone(), typeof(IViewFor<HomeViewModelStandalone>));
      Locator.CurrentMutable.Register(() => Bindings, typeof(ConnectorBindingsStandalone));

      RouterInstance = Router; // makes the router available app-wide
      Router.Navigate.Execute(new HomeViewModelStandalone(this));

      Bindings.UpdateSavedStreams = HomeViewModelStandalone.Instance.UpdateSavedStreams;
      Bindings.UpdateSelectedStream = HomeViewModelStandalone.Instance.UpdateSelectedStream;

      Router.PropertyChanged += Router_PropertyChanged;
      //var theme = PaletteHelper.GetTheme();
      //theme.SetPrimaryColor(SwatchHelper.Lookup[MaterialColor.Blue600]);
      //PaletteHelper.SetTheme(theme);
    }

    private void Router_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      throw new System.NotImplementedException();
    }

    public static void GoHome()
    {
      if (RouterInstance != null && HomeViewModelStandalone.Instance != null)
        RouterInstance.Navigate.Execute(HomeViewModelStandalone.Instance);
    }

  }
}
