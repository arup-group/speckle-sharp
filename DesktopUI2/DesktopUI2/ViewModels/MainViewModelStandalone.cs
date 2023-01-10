//using DesktopUI2.ViewModels.Share;
//using DesktopUI2.Views.Pages;
//using ReactiveUI;
//using Speckle.Core.Logging;
//using Splat;
//using System;
//using System.Reactive;

//namespace DesktopUI2.ViewModels
//{
//  public class MainViewModelStandalone : MainViewModel, IScreen
//  {
//    new public string TitleFull => "Speckle for " + Bindings.GetHostAppNameVersion();
//    new public RoutingState Router { get; private set; }

//    new public ConnectorBindings Bindings { get; private set; } = new DummyBindingsStandalone();

//    new public static RoutingState RouterInstance { get; private set; }

//    new public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

//    public MainViewModelStandalone(ConnectorBindings _bindings) : base()
//    {
//      Bindings = _bindings;
//      Setup.Init(Bindings.GetHostAppNameVersion(), Bindings.GetHostAppName());
//      Init();
//    }

//    public MainViewModelStandalone()
//    {
//      Init();
//    }

//    private void Init()
//    {
//      Router = new RoutingState();

//      Locator.CurrentMutable.Register(() => new StreamEditViewStandalone(), typeof(IViewFor<StreamViewModel>));
//      Locator.CurrentMutable.Register(() => new HomeViewStandalone(), typeof(IViewFor<HomeViewModelStandalone>));
//      //Locator.CurrentMutable.Register(() => new CollaboratorsView(), typeof(IViewFor<CollaboratorsViewModelStandalone>));
//      //Locator.CurrentMutable.Register(() => new SettingsView(), typeof(IViewFor<SettingsPageViewModelStandalone>));
//      Locator.CurrentMutable.Register(() => Bindings, typeof(ConnectorBindings));

//      RouterInstance = Router; // makes the router available app-wide
//      Router.Navigate.Execute(new HomeViewModelStandalone(this));

//      Bindings.UpdateSavedStreams = HomeViewModelStandalone.Instance.UpdateSavedStreams;
//      Bindings.UpdateSelectedStream = HomeViewModelStandalone.Instance.UpdateSelectedStream;

//      Router.PropertyChanged += Router_PropertyChanged;
//      //var theme = PaletteHelper.GetTheme();
//      //theme.SetPrimaryColor(SwatchHelper.Lookup[MaterialColor.Blue600]);
//      //PaletteHelper.SetTheme(theme);
//    }

//    //https://github.com/AvaloniaUI/Avalonia/issues/5290
//    private void CatchReactiveException(Exception e)
//    {
//      //do nothing
//    }

//    private void Router_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
//    {
//      throw new System.NotImplementedException();
//    }

//    new public static void GoHome()
//    {
//      if (RouterInstance != null && HomeViewModelStandalone.Instance != null)
//        RouterInstance.Navigate.Execute(HomeViewModelStandalone.Instance);
//    }

//  }
//}
