using Avalonia.Controls;
using Avalonia.Controls.Selection;
using DesktopUI2.Views.Pages.ShareControls;
using ReactiveUI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace DesktopUI2.ViewModels.Share
{
  public class CollaboratorsViewModelStandalone : CollaboratorsViewModel, IRoutableViewModel
  {
    //public IScreen HostScreen { get; }
    //public string UrlPathSegment { get; } = "collaborators";

    private ConnectorBindings Bindings;

    #region bindings

    public ReactiveCommand<Unit, Unit> GoBack => MainWindowViewModelStandalone.RouterInstance.NavigateBack;

    #endregion

    private StreamViewModel _stream;

    public CollaboratorsViewModelStandalone(IScreen screen, StreamViewModel stream) : base(screen, stream)
    {
      //HostScreen = screen;
      _stream = stream;
      Role = stream.Stream.role;
      Bindings = Locator.Current.GetService<ConnectorBindingsStandalone>();

      userSearchDebouncer = Utils.Debounce(SearchUsers);

      SelectionModel = new SelectionModel<AccountViewModel>();
      SelectionModel.SingleSelect = false;
      SelectionModel.SelectionChanged += SelectionModel_SelectionChanged;

      foreach (var collab in stream.Stream.collaborators)
      {
        AddedUsers.Add(new AccountViewModel(collab));
      }
    }

    public override async void SaveCommand()
    {

      foreach (var user in AddedUsers)
      {
        //invite users by email
        if (Utils.IsValidEmail(user.Name))
        {
          try
          {
            await _stream.StreamState.Client.StreamInviteCreate(new StreamInviteCreateInput { email = user.Name, streamId = _stream.StreamState.StreamId, message = "I would like to share a model with you via Speckle!" });
          }
          catch (Exception e)
          {

          }
        }
        //add new collaborators
        else if (!_stream.Stream.collaborators.Any(x => x.id == user.Id))
        {
          try
          {
            await _stream.StreamState.Client.StreamGrantPermission(new StreamGrantPermissionInput { userId = user.Id, streamId = _stream.StreamState.StreamId, role = "stream:contributor" });
          }
          catch (Exception e)
          {

          }
        }
      }

      //remove collaborators
      foreach (var user in _stream.Stream.collaborators)
      {
        if (!AddedUsers.Any(x => x.Id == user.id))
        {
          try
          {
            await _stream.StreamState.Client.StreamRevokePermission(new StreamRevokePermissionInput { userId = user.id, streamId = _stream.StreamState.StreamId });
          }
          catch (Exception e)
          {

          }
        }
      }

      try
      {
        _stream.Stream = await _stream.StreamState.Client.StreamGet(_stream.StreamState.StreamId);
        _stream.StreamState.CachedStream = _stream.Stream;

      }
      catch (Exception e)
      {
      }

      MainWindowViewModelStandalone.RouterInstance.NavigateBack.Execute();
    }

  }
}
