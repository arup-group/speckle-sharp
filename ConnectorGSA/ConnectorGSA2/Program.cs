using Avalonia;
using Avalonia.ReactiveUI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopUI2;
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using System.Timers;
using System.Diagnostics;
using Avalonia.Controls;
using ConnectorGSA.UI;
using System.Reflection;

namespace ConnectorGSA.Launcher
{
  class Program
  {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    //public static void Main(string[] args) => BuildAvaloniaApp()
    //    .StartWithClassicDesktopLifetime(args);


    public static void Main(string[] args)
    {
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
      AppDomain domain = null;
      try
      {
        //cHelper helper = new Helper();
        //var etabsObject = helper.GetObject("CSI.ETABS.API.ETABSObject");
        //model = etabsObject.SapModel;
      }
      catch (Exception E)
      {
        return;
      }

      try
      {
        //SelectionTimer = new Timer(2000) { AutoReset = true, Enabled = true };
        //SelectionTimer.Elapsed += SelectionTimer_Elapsed;
        //SelectionTimer.Start();
        OpenOrFocusSpeckle();
      }

      catch (Exception e)
      {
        throw e;
        //return;
      }

      return;
    }


    public static Window MainWindow { get; private set; }

    public static ConnectorBindingsGSA Bindings { get; set; }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<DesktopUI2.App>()
      .UsePlatformDetect()
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
      .LogToTrace()
      .UseReactiveUI();

    public static void CreateOrFocusSpeckle()
    {
      if (MainWindow == null)
      {
        BuildAvaloniaApp().Start(AppMain, null);
      }

      MainWindow.Show();
      MainWindow.Activate();
    }

    private static void AppMain(Application app, string[] args)
    {
      var viewModel = new MainWindowViewModel(Bindings);
      MainWindow = new MainWindow { DataContext = viewModel };
      MainWindow.Closed += SpeckleWindowClosed;
      MainWindow.Closing += SpeckleWindowClosed;
      app.Run(MainWindow);
      //Task.Run(() => app.Run(MainWindow));
    }

    public static void OpenOrFocusSpeckle()
    {
      Bindings = new ConnectorBindingsGSA();
      CreateOrFocusSpeckle();
    }

    private static void SpeckleWindowClosed(object sender, EventArgs e)
    {
      Environment.Exit(0);
    }

    static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
      Assembly a = null;
      var name = args.Name.Split(',')[0];
      string path = Path.GetDirectoryName(typeof(App).Assembly.Location);

      string assemblyFile = Path.Combine(path, name + ".dll");

      if (File.Exists(assemblyFile))
        a = Assembly.LoadFrom(assemblyFile);

      return a;
    }

  }
}
