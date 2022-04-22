using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Controls.Selection;
using DesktopUI2.Models;
using DesktopUI2.Models.Settings;
using DesktopUI2.Views;
using DesktopUI2.Views.Pages;
using DesktopUI2.Views.Windows;
using DynamicData;
using Material.Icons;
using Material.Icons.Avalonia;
using ReactiveUI;
using Speckle.Core.Api;
using Speckle.Core.Logging;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Stream = Speckle.Core.Api.Stream;
using DesktopUI2.Views.Windows.Dialogs;

namespace DesktopUI2.ViewModels
{
  public class StreamViewModelStandalone : StreamViewModel, IRoutableViewModel
  {
    private IScreen HostScreen { get; set; }

    private ConnectorBindingsStandalone Bindings;

    private List<MenuItemViewModel> _menuItems = new List<MenuItemViewModel>();
    new public ReactiveCommand<Unit, Unit> GoBack => MainWindowViewModelStandalone.RouterInstance.NavigateBack;
    public List<string> GSALayers { get; set; } = new List<string> { "Design", "Analysis", "Both" };

    public SelectionModel<string> SelectedLayer { get; set; }

    public double CoincidentNodeAllowance { get; set; } = 10;

    public List<string> Units { get; set; } = new List<string> { "Millimetres", "Metres", "Inches" };


    private string _selectedUnits = "Millimetres";
    public string SelectedUnits
    {
      get => _selectedUnits;
      set
      {
        this.RaiseAndSetIfChanged(ref _selectedUnits, value);
        Bindings.Units = value;
      }
    }
    void SelectedLayerSelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
    {
      Bindings.Layer = (string)e.SelectedItems.FirstOrDefault();
    }

    void SelectedUnitsSelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
    {
      Bindings.Units = (string)e.SelectedItems.FirstOrDefault();
    }
    public StreamViewModelStandalone(StreamState streamState, IScreen hostScreen, ICommand removeSavedStreamCommand) : base()
    {
      StreamState = streamState;
      //use cached stream, then load a fresh one async 
      //this way we can immediately show stream name and other info and update it later if it changed
      Stream = streamState.CachedStream;
      Client = streamState.Client;
      IsReceiver = streamState.IsReceiver;

      //default to receive mode if no permission to send
      if (Stream.role == null || Stream.role == "stream:reviewer")
      {
        IsReceiver = true;
      }

      HostScreen = hostScreen;
      RemoveSavedStreamCommand = removeSavedStreamCommand;

      //use dependency injection to get bindings
      Bindings = Locator.Current.GetService<ConnectorBindingsStandalone>();

      if (Client == null)
      {
        NoAccess = true;
        Notification = "You do not have access to this Stream.";
        NotificationUrl = $"{streamState.ServerUrl}/streams/{streamState.StreamId}";
        return;
      }

      Init();
      GenerateMenuItems();

      var updateTextTimer = new System.Timers.Timer();
      updateTextTimer.Elapsed += UpdateTextTimer_Elapsed;
      updateTextTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
      updateTextTimer.Enabled = true;

      SelectedLayer = new SelectionModel<string>();
      SelectedLayer.SingleSelect = true;
      SelectedLayer.Select(0);

      SelectedLayer.SelectionChanged += SelectedLayerSelectionChanged;
    }

    private void Init()
    {
      GetStream().ConfigureAwait(false);

      GetBranchesAndRestoreState();
      GetActivity();
    }

    private void UpdateTextTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      this.RaisePropertyChanged("LastUsed");
      this.RaisePropertyChanged("LastUpdated");
    }

    private void GenerateMenuItems()
    {
      var menu = new MenuItemViewModel { Header = new MaterialIcon { Kind = MaterialIconKind.EllipsisVertical, Foreground = Avalonia.Media.Brushes.Gray } };
      menu.Items = new List<MenuItemViewModel> {
        //new MenuItemViewModel (EditSavedStreamCommand, "Edit",  MaterialIconKind.Cog),
        new MenuItemViewModel (ViewOnlineSavedStreamCommand, "View online",  MaterialIconKind.ExternalLink),
        new MenuItemViewModel (CopyStreamURLCommand, "Copy URL to clipboard",  MaterialIconKind.ContentCopy),
        new MenuItemViewModel (OpenReportCommand, "Open Report",  MaterialIconKind.TextBox)
      };
      var customMenues = Bindings.GetCustomStreamMenuItems();
      if (customMenues != null)
        menu.Items.AddRange(customMenues.Select(x => new MenuItemViewModel(x, this.StreamState)).ToList());
      //remove is added last
      //menu.Items.Add(new MenuItemViewModel(RemoveSavedStreamCommand, StreamState.Id, "Remove", MaterialIconKind.Bin));
      MenuItems.Add(menu);

      this.RaisePropertyChanged("MenuItems");
    }

    internal override async void GetBranchesAndRestoreState()
    {
      //get available settings from our bindings
      Settings = Bindings.GetSettings();

      //get available filters from our bindings
      AvailableFilters = new List<FilterViewModel>(Bindings.GetSelectionFilters().Select(x => new FilterViewModel(x)));
      SelectedFilter = AvailableFilters[0];

      var branches = await Client.StreamGetBranches(Stream.id, 100, 0);
      Branches = branches;

      var branch = Branches.FirstOrDefault(x => x.name == StreamState.BranchName);
      if (branch != null)
        SelectedBranch = branch;
      else
        SelectedBranch = Branches[0];

      if (StreamState.Filter != null)
      {
        SelectedFilter = AvailableFilters.FirstOrDefault(x => x.Filter.Slug == StreamState.Filter.Slug);
        if (SelectedFilter != null)
          SelectedFilter.Filter = StreamState.Filter;
      }
      if (StreamState.Settings != null)
      {
        foreach (var setting in Settings)
        {
          var savedSetting = StreamState.Settings.FirstOrDefault(o => o.Slug == setting.Slug);
          if (savedSetting != null)
            setting.Selection = savedSetting.Selection;
        }
      }
    }

    private async void GetActivity()
    {

      var filteredActivity = (await Client.StreamGetActivity(Stream.id))
        .Where(x => x.actionType == "commit_create" || x.actionType == "commit_receive" || x.actionType == "stream_create")
        .Reverse().ToList();
      var activity = new List<ActivityViewModel>();
      foreach (var a in filteredActivity)
      {
        var avm = new ActivityViewModel();
        await avm.Init(a, Client);
        activity.Add(avm);

      }
      Activity = activity;
      ScrollToBottom();

    }

    private async void ScrollToBottom()
    {
      if (StreamEditViewStandalone.Instance != null)
      {
        await Task.Delay(250);
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
          var scroller = StreamEditViewStandalone.Instance.FindControl<ScrollViewer>("activityScroller");
          if (scroller != null)
          {
            scroller.ScrollToEnd();
          }
        });
      }
    }

    /// <summary>
    /// Update the model Stream state whenever we send, receive or save a stream
    /// </summary>
    private void UpdateStreamState()
    {
      StreamState.BranchName = SelectedBranch.name;
      StreamState.IsReceiver = IsReceiver;
      if (IsReceiver)
        StreamState.CommitId = SelectedCommit.id;
      if (!IsReceiver)
        StreamState.Filter = SelectedFilter.Filter;
      StreamState.Settings = Settings.Select(o => o).ToList();
    }

    private async void GetCommits()
    {
      if (SelectedBranch.commits == null || SelectedBranch.commits.totalCount > 0)
      {
        var branch = await Client.BranchGet(Stream.id, SelectedBranch.name, 100);
        branch.commits.items.Insert(0, new Commit { id = "latest", message = "Always receive the latest commit sent to this branch." });
        Commits = branch.commits.items;
        var commit = Commits.FirstOrDefault(x => x.id == StreamState.CommitId);
        if (commit != null)
          SelectedCommit = commit;
        else
          SelectedCommit = Commits[0];
      }
      else
      {
        SelectedCommit = null;
        Commits = new List<Commit>();
        SelectedCommit = null;
      }
    }

    private async void Client_OnCommitCreated(object sender, Speckle.Core.Api.SubscriptionModels.CommitInfo info)
    {
      var branches = await Client.StreamGetBranches(StreamState.StreamId);

      if (!IsReceiver) return;

      var binfo = branches.FirstOrDefault(b => b.name == info.branchName);
      var cinfo = binfo.commits.items.FirstOrDefault(c => c.id == info.id);

      Notification = $"{cinfo.authorName} sent to {info.branchName}: {info.message}";
      NotificationUrl = $"{StreamState.ServerUrl}/streams/{StreamState.StreamId}/commits/{cinfo.id}";
      ScrollToBottom();
    }


    private void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
    {
      try
      {
        byte[] bytes = e.Result;

        System.IO.Stream stream = new MemoryStream(bytes);

        var image = new Avalonia.Media.Imaging.Bitmap(stream);
        PreviewImage = image;
        this.RaisePropertyChanged("PreviewImage");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex);
        PreviewImageUrl = null; // Could not download...
      }
    }

    #region commands

    public override async void SendCommand()
    {
      UpdateStreamState();
      //save the stream as well
      HomeViewModelStandalone.Instance.AddSavedStream(this);

      Reset();
      Progress.ProgressTitle = "Sending to Speckle 🚀";
      Progress.IsProgressing = true;

      var dialog = new QuickOpsDialog();
      dialog.DataContext = Progress;
      dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dialog.ShowDialog(MainWindowStandalone.Instance);

      var commitId = await Task.Run(() => Bindings.SendStream(StreamState, Progress));
      Progress.IsProgressing = false;

      if (!Progress.CancellationTokenSource.IsCancellationRequested)
      {
        LastUsed = DateTime.Now.ToString();
        Analytics.TrackEvent(Client.Account, Analytics.Events.Send);

        Notification = $"Sent successfully, view online";
        NotificationUrl = $"{StreamState.ServerUrl}/streams/{StreamState.StreamId}/commits/{commitId}";
      }
      else
        dialog.Close();

      if (Progress.Report.ConversionErrorsCount > 0 || Progress.Report.OperationErrorsCount > 0)
        ShowReport = true;

      GetActivity();
    }

    public override async void ReceiveCommand()
    {
      UpdateStreamState();
      //save the stream as well
      HomeViewModelStandalone.Instance.AddSavedStream(this);

      Reset();

      Progress.ProgressTitle = "Receiving from Speckle 🚀";
      Progress.IsProgressing = true;

      var dialog = new QuickOpsDialog();
      dialog.DataContext = Progress;
      dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dialog.ShowDialog(MainWindowStandalone.Instance);

      await Task.Run(() => Bindings.ReceiveStream(StreamState, Progress));
      Progress.IsProgressing = false;

      if (!Progress.CancellationTokenSource.IsCancellationRequested)
      {
        LastUsed = DateTime.Now.ToString();
        Analytics.TrackEvent(StreamState.Client.Account, Analytics.Events.Receive);
      }
      else
        dialog.Close(); // if user cancelled close automatically

      if (Progress.Report.ConversionErrorsCount > 0 || Progress.Report.OperationErrorsCount > 0)
        ShowReport = true;


      GetActivity();

    }

    private void Reset()
    {
      Notification = "";
      NotificationUrl = "";
      ShowReport = false;
      Progress = new ProgressViewModel();
    }




    private async void OpenSettingsCommand()
    {
      try
      {
        var settingsWindow = new Settings();
        settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        // Not doing this causes Avalonia to throw an error about the owner being already set on the Setting View UserControl
        Settings.ForEach(x => x.ResetView());

        var settingsPageViewModel = new SettingsPageViewModel(Settings.Select(x => new SettingViewModel(x)).ToList());
        settingsWindow.DataContext = settingsPageViewModel;
        settingsWindow.Title = $"Settings for {Stream.name}";
        Analytics.TrackEvent(null, Analytics.Events.DUIAction, new Dictionary<string, object>() { { "name", "Settings Open" } });
        var saveResult = await settingsWindow.ShowDialog<bool?>(MainWindow.Instance); // TODO: debug throws "control already has a visual parent exception" when calling a second time

        if (saveResult != null && (bool)saveResult)
        {
          Settings = settingsPageViewModel.Settings.Select(x => x.Setting).ToList();
        }
      }
      catch (Exception e)
      {
      }
    }


    [DependsOn(nameof(SelectedBranch))]
    [DependsOn(nameof(SelectedFilter))]
    [DependsOn(nameof(SelectedCommit))]
    [DependsOn(nameof(IsReceiver))]
    private bool CanSaveCommand(object parameter)
    {
      return true;
    }


    [DependsOn(nameof(DesktopUI2.ViewModels.HomeViewModelStandalone.HasGSAFile))]
    [DependsOn(nameof(SelectedBranch))]
    [DependsOn(nameof(SelectedFilter))]
    [DependsOn(nameof(IsReceiver))]
    private bool CanSendCommand(object parameter)
    {
      return IsReady();
    }

    [DependsOn(nameof(DesktopUI2.ViewModels.HomeViewModelStandalone.HasGSAFile))]
    [DependsOn(nameof(SelectedBranch))]
    [DependsOn(nameof(SelectedCommit))]
    [DependsOn(nameof(IsReceiver))]
    private bool CanReceiveCommand(object parameter)
    {
      return IsReady();
    }

    private bool IsReady()
    {
      if (NoAccess)
        return false;
      if (SelectedBranch == null)
        return false;
      if (!HomeViewModelStandalone.Instance.HasGSAFile)
        return false;

      if (!IsReceiver)
      {
        if (SelectedFilter == null)
          return false;
        if (!SelectedFilter.IsReady())
          return false;
      }
      else
      {
        if (SelectedCommit == null)
          return false;
      }

      return true;
    }
    #endregion

  }
}
