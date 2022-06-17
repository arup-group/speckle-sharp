using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using DesktopUI2.Models;
using DesktopUI2.Views;
using DesktopUI2.Views.Windows.Dialogs;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using ReactiveUI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DesktopUI2.ViewModels
{
  public class HomeViewModelStandalone : HomeViewModel, IRoutableViewModel
  {

    #region bindings
    public bool HasGSAFile { get; set; }

    private string _FilePath = "No file loaded";

    public string FilePath
    {
      get => _FilePath;
      set
      {
        this.RaiseAndSetIfChanged(ref _FilePath, value);
      }
    }

    public string FileStatus { get; set; }

    #endregion

    public HomeViewModelStandalone(IScreen screen) : base(screen)
    {
      Instance = this;

      SavedStreams.CollectionChanged += SavedStreams_CollectionChanged;

      Bindings = Locator.Current.GetService<ConnectorBindings>();
      this.RaisePropertyChanged("SavedStreams");
      //Init();

      var config = ConfigManager.Load();
      ChangeTheme(config.DarkTheme);
    }



    public async void NewFileCommand()
    {
      try
      {
        var bindings = (IConnectorBindingsStandalone)Bindings;
        bindings.NewFile();
        HasGSAFile = true;
        FilePath = "New file";
        FileStatus = "new";
      }
      catch (Exception e)
      {
        Dialogs.ShowDialog(MainWindow.Instance, "Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
      }
    }


    public async void OpenFileCommand()
    {
      var dialog = new OpenFileDialog();
      dialog.Filters.Add(new FileDialogFilter() { Name = "GSA Files", Extensions = { "gwb", "gwa" } });
      var result = await dialog.ShowAsync(MainWindow.Instance);

      if (result != null)
      {
        var path = result.FirstOrDefault();
        if (!string.IsNullOrEmpty(path))
        {
          try
          {
            var bindings = (IConnectorBindingsStandalone)Bindings;
            bindings.OpenFile(path);
            HasGSAFile = true;
            FilePath = path;
            FileStatus = "existing"; // GsaLoadedFileType.ExistingFile;
          }
          catch (Exception e)
          {
            Dialogs.ShowDialog(MainWindow.Instance, "Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
          }
        }
      }
    }

    private Tuple<bool, string> ValidateUrl(string url)
    {
      Uri uri;
      try
      {
        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
          var sw = new StreamWrapper(url);
        }
        else return new Tuple<bool, string>(false, "URL is not valid.");
      }
      catch { return new Tuple<bool, string>(false, "URL is not a Stream."); }

      return new Tuple<bool, string>(true, "");
    }

    private Tuple<bool, string> ValidateName(string name)
    {
      if (string.IsNullOrEmpty(name))
        return new Tuple<bool, string>(false, "Streams need a name too!");

      if (name.Trim().Length < 3)
        return new Tuple<bool, string>(false, "Name is too short.");

      return new Tuple<bool, string>(true, "");
    }

  }
}