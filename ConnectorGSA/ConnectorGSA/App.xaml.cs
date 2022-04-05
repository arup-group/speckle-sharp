using Speckle.GSA.API;
using System.Globalization;
using System.Windows;

namespace ConnectorGSA
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

      Instance.GsaModel = new GsaModel();

      if (e.Args.Length == 0)
      {
        MainWindow wnd = new MainWindow();
        wnd.Show();
      }
      else
      {
        var headless = new Headless();
        Headless.AttachConsole(-1);

        headless.RunCLI(e.Args);
        System.Windows.Forms.SendKeys.SendWait("{ENTER}"); //TODO: not great, figure out a better way to signal completion of process in console (we should have two binaries, for headless and GUI app, anyways?)
        Current.Shutdown();
      }
    }
  }
}
