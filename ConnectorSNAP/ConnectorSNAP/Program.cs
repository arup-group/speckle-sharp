using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2;
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using Speckle.SNAP.API;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  class Program
  {
    public static Window MainWindow { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

      if (args.Length == 0)
      {
        Instance.SnapModel = new SnapModel();

        var appBuilder = AppBuilder.Configure(() => new App())
          .UsePlatformDetect().With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false }).LogToTrace().UseReactiveUI();

        appBuilder.Start(AppMain, null);
        //appBuilder.StartWithClassicDesktopLifetime(null);
      }
      else
      {
        try
        {
          var headless = new Headless();
          Headless.AttachConsole(-1);

          headless.RunCLI(args);

          Console.WriteLine("Finished requested action");

          FreeConsole();
        }
        catch
        {
        }
        finally
        {
          ConsoleNewLine();
        }
      }
    }

    private static void AppMain(Avalonia.Application app, string[] args)
    {
      var lifetime = new ClassicDesktopStyleApplicationLifetime
      {
        MainWindow = new MyMainWindow
        {
          DataContext = new MainWindowViewModel(new SNAPBindings()),
        }, 
        ShutdownMode = ShutdownMode.OnMainWindowClose
      };

      app.ApplicationLifetime = lifetime;

      lifetime.Start(new[] { "" });
    }

    [DllImport("kernel32")]
    private static extern bool FreeConsole();

    private static void ConsoleNewLine()
    {
      try
      {
        // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. This gives the newline and looks normal from the console:
        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
      }
      catch (Exception e)
      {
        Debug.Fail(e.ToString());
      }
    }

  }

  public class MyMainWindow: MainWindow
  {
    //The parent class is designed as a plug-in and cancels any closing request, just hides the window.  Since this connector is a deskop app,
    //this needs to be overridden to enable normal closing of the application when the window is closed.
    protected override void OnClosing(CancelEventArgs e)
    {
      this.Hide();
    }

    private void SaveCommand()
    {

    }
  }
}
