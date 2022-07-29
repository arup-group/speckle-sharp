using Avalonia.Controls;
using Avalonia.Controls.Selection;
using DesktopUI2.Views.Pages.ShareControls;
using ReactiveUI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using DesktopUI2.Models;
using System.Web;
using System.Net;
using System.IO;
using DesktopUI2.Views.Windows.Dialogs;
using GraphQL;
using GraphQL.Client.Http;
using System.Threading.Tasks;
using Avalonia.Metadata;

namespace DesktopUI2.ViewModels
{
  public class NewStreamViewModel : ReactiveObject, IRoutableViewModel
  {
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "collaborators";

    private ConnectorBindings Bindings;

    #region bindings

    public ReactiveCommand<Unit, Unit> GoBack => MainViewModel.RouterInstance.NavigateBack;

    private AccountViewModel _account;
    public AccountViewModel Account
    {
      get { return _account; }
      set
      {
        // Some logic here
        _account = value;
        IsJobNumberRequiredToCreateStreams();
      }
    }

    public ObservableCollection<AccountViewModel> Accounts { get; set; }
    public string StreamName { get; set; }
    public string Description { get; set; }
    public bool IsPublic { get; set; }

    public string _jobNumber;
    public string JobNumber
    {
      get => _jobNumber;
      set
      {
        this.RaiseAndSetIfChanged(ref _jobNumber, value);
        this.RaisePropertyChanged("JobNumberProvided");
        Search();
      }
    }

    public bool JobNumberProvided => (JobNumberRequired && JobNumber != null) || (!JobNumberRequired);

    public Action jobSearchDebouncer = null;

    public string _searchQuery = "";

    public string SearchQuery
    {
      get => _searchQuery;
      set
      {
        this.RaiseAndSetIfChanged(ref _searchQuery, value);
        Search();
      }
    }

    private List<JobModel> _jobs;
    public List<JobModel> Jobs
    {
      get => _jobs;
      set
      {
        this.RaiseAndSetIfChanged(ref _jobs, value);
      }
    }

    private JobModel _selectedJobs;

    public JobModel SelectedJob
    {
      get => _selectedJobs;
      set
      {
        this.RaiseAndSetIfChanged(ref _selectedJobs, value);
        if (value != null)
        {
          SearchQuery = "";
          JobNumber = value.JobCode;
        }

      }
    }

    private bool _dropDownOpen;
    public bool DropDownOpen
    {
      get => _dropDownOpen;
      set
      {
        this.RaiseAndSetIfChanged(ref _dropDownOpen, value);
      }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
      get => _errorMessage;
      set
      {
        this.RaiseAndSetIfChanged(ref _errorMessage, value);
      }
    }

    private bool _showProgress;
    public bool ShowProgress
    {
      get => _showProgress;
      set
      {
        this.RaiseAndSetIfChanged(ref _showProgress, value);
      }
    }

    private bool _jobNumberRequired;
    public bool JobNumberRequired
    {
      get => _jobNumberRequired;
      set
      {
        this.RaiseAndSetIfChanged(ref _jobNumberRequired, value);
        this.RaisePropertyChanged("JobNumberProvided");
      }
    }

    private string _defaultJobNumberWatermark = "Search job number or name";
    private string _jobNumberWatermark;
    public string JobNumberWatermark
    {
      get => _jobNumberWatermark;
      set
      {
        this.RaiseAndSetIfChanged(ref _jobNumberWatermark, value);
      }
    }

    #endregion

    public NewStreamViewModel() { }

    public NewStreamViewModel(IScreen screen, List<AccountViewModel> accounts)
    {
      HostScreen = screen;
      Bindings = Locator.Current.GetService<ConnectorBindings>();

      Accounts = new ObservableCollection<AccountViewModel>(accounts);
      Account = Accounts[0];

      jobSearchDebouncer = Utils.Debounce(SearchJobs);

      IsJobNumberRequiredToCreateStreams();
    }

    public class ServerInfoResponse
    {
      public ServerInfo serverInfo { get; set; }
    }

    public class ServerInfo
    {
      public bool requireJobNumberToCreateStreams { get; set; }
      public string name { get; set; }
    }

    public async void IsJobNumberRequiredToCreateStreams()
    {
      await CheckIfJobNumberRequired();
    }

    public async Task<bool> CheckIfJobNumberRequired()
    {
      var acc = Account.Account;
      var client = new Client(acc);

      var request = new GraphQLRequest
      {
        Query = @" query { serverInfo { requireJobNumberToCreateStreams name } }"
      };

      try
      {
        var response = await client.GQLClient.SendQueryAsync<ServerInfoResponse>(request);

        if (response.Errors == null)
        {
          JobNumberRequired = response.Data.serverInfo.requireJobNumberToCreateStreams;

          if (JobNumberRequired) JobNumberWatermark = _defaultJobNumberWatermark + " (required)";
          else JobNumberWatermark = _defaultJobNumberWatermark + " (optional)";

          return true;
        }

        return false;
      }
      catch
      {
        JobNumberRequired = false;
        JobNumberWatermark = _defaultJobNumberWatermark + " (optional)";
        return false;
      }
    }

    public void Search()
    {
      Focus();
      if (SearchQuery.Length < 4)
        return;

      jobSearchDebouncer();
    }

    //focus is lost when the dropdown gets closed
    public void Focus()
    {
      DropDownOpen = false;
      var searchBox = NewStreamDialog.Instance.FindControl<TextBox>("SearchBox");
      searchBox.Focus();
    }

    public async void SearchJobs()
    {
      ShowProgress = true;
      var acc = Account.Account;

      var query = SearchQuery.Replace("-", "");
      var url = $"{acc.serverInfo.url}/api/jobNumber/{query}";
      var token = acc.token;

      var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
      httpWebRequest.Method = "GET";
      httpWebRequest.Headers["Authorization"] = $"Bearer {token}";

      try
      {
        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

        if (httpResponse.StatusCode != HttpStatusCode.OK)
        {
          return;
        }
        else
        {
          string result = null;
          using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
          {
            result = streamReader.ReadToEnd();
          }

          var searchOptions = Newtonsoft.Json.JsonConvert.DeserializeObject<JobSearchOptions>(result);

          Jobs = searchOptions.jobs;

          ShowProgress = false;
          DropDownOpen = true;
        }
      }
      catch (Exception e)
      {
        ErrorMessage = e.Message;
        ShowProgress = false;
      }
    }

    public void ClearSearchCommand()
    {
      SearchQuery = "";
      Jobs = null;

      ErrorMessage = null;
      ShowProgress = false;
    }

    public virtual async void NewStreamCommand()
    {
      try
      {
        var client = new Client(Account.Account);
        StreamCreateInput createInput;
        if (!String.IsNullOrEmpty(JobNumber))
          createInput = new StreamWithJobNumberCreateInput { description = Description, name = StreamName, isPublic = IsPublic, jobNumber = JobNumber };
        else
          createInput = new StreamCreateInput { description = Description, name = StreamName, isPublic = IsPublic };

        var streamId = await client.StreamCreate(createInput);
        var stream = await client.StreamGet(streamId);
        var streamState = new StreamState(Account.Account, stream);

        NewStreamDialog.Instance.Close(true);

        HomeViewModel.OpenStream(streamState);
        Analytics.TrackEvent(Account.Account, Analytics.Events.DUIAction, new Dictionary<string, object>() { { "name", "Stream Create" } });

      }
      catch (Exception e)
      {
        Dialogs.ShowDialog("Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
      }
    }
  }
}
