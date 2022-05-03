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
    //Instance of this HomeViewModel, so that the SavedStreams are kept in memory and not disposed on navigation
    new public static HomeViewModelStandalone Instance { get; private set; }
    public override ConnectorBindings Bindings { get; set; }

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

      Bindings = Locator.Current.GetService<ConnectorBindingsStandalone>();
      this.RaisePropertyChanged("SavedStreams");
      Init();

      var config = ConfigManager.Load();
      ChangeTheme(config.DarkTheme);
    }


    //write changes to file every time they happen
    //this is because if there is an active document change we need to swap saved streams and restore them later
    //even if the doc has not been saved
    private void SavedStreams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      WriteStreamsToFile();
    }

    private async Task GetStreams()
    {
      if (!HasAccounts)
        return;

      InProgress = true;

      Streams = new List<StreamAccountWrapper>();

      foreach (var account in Accounts)
      {
        try
        {
          var client = new Client(account.Account);
          Streams.AddRange((await client.StreamsGet()).Select(x => new StreamAccountWrapper(x, account.Account)));
        }
        catch (Exception e)
        {
          Dialogs.ShowDialog($"Could not get streams for {account.Account.userInfo.email} on {account.Account.serverInfo.url}.", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
        }
      }
      Streams = Streams.OrderByDescending(x => DateTime.Parse(x.Stream.updatedAt)).ToList();

      InProgress = false;

    }

    private async Task SearchStreams()
    {
      if (SearchQuery == "")
      {
        GetStreams().ConfigureAwait(false);
        return;
      }
      if (SearchQuery.Length <= 2)
        return;
      InProgress = true;

      Streams = new List<StreamAccountWrapper>();

      foreach (var account in Accounts)
      {
        try
        {
          var client = new Client(account.Account);
          Streams.AddRange((await client.StreamSearch(SearchQuery)).Select(x => new StreamAccountWrapper(x, account.Account)));
        }
        catch (Exception e)
        {

        }
      }

      Streams = Streams.OrderByDescending(x => DateTime.Parse(x.Stream.updatedAt)).ToList();

      InProgress = false;

    }

    private void RemoveSavedStream(string id)
    {
      var s = SavedStreams.FirstOrDefault(x => x.StreamState.Id == id);
      if (s != null)
      {
        SavedStreams.Remove(s);
        if (s.StreamState.Client != null)
          Analytics.TrackEvent(s.StreamState.Client.Account, Analytics.Events.DUIAction, new Dictionary<string, object>() { { "name", "Stream Remove" } });
      }

      this.RaisePropertyChanged("HasSavedStreams");
    }

    public override async void NewStreamCommand()
    {
      var dialog = new NewStreamDialog(Accounts);
      dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      await dialog.ShowDialog(MainWindowStandalone.Instance);

      if (dialog.Create)
      {
        try
        {
          var client = new Client(dialog.Account);
          var streamId = await client.StreamCreate(new StreamCreateInput { description = dialog.Description, name = dialog.StreamName, isPublic = dialog.IsPublic });
          var stream = await client.StreamGet(streamId);
          var streamState = new StreamState(dialog.Account, stream);

          OpenStream(streamState);

          Analytics.TrackEvent(dialog.Account, Analytics.Events.DUIAction, new Dictionary<string, object>() { { "name", "Stream Create" } });

          GetStreams().ConfigureAwait(false); //update streams
        }
        catch (Exception e)
        {
          Dialogs.ShowDialog("Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
        }
      }
    }

    public async void NewFileCommand()
    {
      try
      {
        var bindings = (ConnectorBindingsStandalone)Bindings;
        bindings.NewFile();
        HasGSAFile = true;
        FilePath = "New file";
        FileStatus = "new";
      }
      catch (Exception e)
      {
        Dialogs.ShowDialog("Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
      }
    }


    public async void OpenFileCommand()
    {
      var dialog = new OpenFileDialog();
      dialog.Filters.Add(new FileDialogFilter() { Name = "GSA Files", Extensions = { "gwb", "gwa" } });
      var result = await dialog.ShowAsync(MainWindowStandalone.Instance);

      if (result != null)
      {
        var path = result.FirstOrDefault();
        if (!string.IsNullOrEmpty(path))
        {
          try
          {
            var bindings = (ConnectorBindingsStandalone)Bindings;
            bindings.OpenFile(path);
            HasGSAFile = true;
            FilePath = path;
            FileStatus = "existing"; // GsaLoadedFileType.ExistingFile;
          }
          catch (Exception e)
          {
            Dialogs.ShowDialog("Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
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
   
    public override void OpenStream(StreamState streamState)
    {
      MainWindowViewModelStandalone.RouterInstance.Navigate.Execute(new StreamViewModelStandalone(streamState, HostScreen, RemoveSavedStreamCommand));
    }

    private void ChangeTheme(bool isDark)
    {
      var paletteHelper = new PaletteHelper();
      var theme = paletteHelper.GetTheme();

      if (isDark)
        theme.SetBaseTheme(BaseThemeMode.Light.GetBaseTheme());
      else
        theme.SetBaseTheme(BaseThemeMode.Dark.GetBaseTheme());
      paletteHelper.SetTheme(theme);
    }

  }
}