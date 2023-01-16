using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;
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
using System.Reflection;

namespace DesktopUI2.Launcher
{
  class Program
  {
    public static void Main(string[] args)
    {
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
      AppDomain domain = null;

      try
      {
        OpenOrFocusSpeckle();
      }

      catch (Exception e)
      {
        throw e;
      }

      return;
    }
      

    public static Window MainWindow { get; private set; }

    public static ConnectorBindings Bindings { get; set; }

    public static AppBuilder BuildAvaloniaApp()
    {
      string path = Path.GetDirectoryName(typeof(App).Assembly.Location);

      string nativeLib = Path.Combine(path, "Native", "libAvalonia.Native.OSX.dylib");

      return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .With(new X11PlatformOptions { UseGpu = false })
      .With(new MacOSPlatformOptions { ShowInDock = false })
      .With(new AvaloniaNativePlatformOptions
      {
        AvaloniaNativeLibraryPath = nativeLib
      })
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
      .LogToTrace()
      .UseReactiveUI();
    }

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
      var viewModel = new MainViewModel(Bindings);
      MainWindow = new MainWindow { DataContext = viewModel };
      MainWindow.Closed += SpeckleWindowClosed;
      MainWindow.Closing += SpeckleWindowClosed;
      app.Run(MainWindow);
    }

    public static void OpenOrFocusSpeckle()
    {
      Bindings = new DummyBindingsStandalone();
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
