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
  public class NewOrOpenFileViewModel : ReactiveObject
  {
    public string FilePath { get; set; }

    public string FileStatus { get; set; }

    public NewOrOpenFileViewModel()
    {

    }

    public async void NewFileCommand()
    {
      //var dialog = new NewFileDialog();
      //dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      //await dialog.ShowDialog(MainWindow.Instance);

      //if (dialog.Create)
      //{
      //  try
      //  {
      //    var client = new Client(dialog.Account);
      //    var streamId = await client.StreamCreate(new StreamCreateInput { description = dialog.Description, name = dialog.StreamName, isPublic = dialog.IsPublic });
      //    var stream = await client.StreamGet(streamId);
      //    var streamState = new StreamState(dialog.Account, stream);

      //    OpenStream(streamState);

      //    Analytics.TrackEvent(dialog.Account, Analytics.Events.DUIAction, new Dictionary<string, object>() { { "name", "Stream Create" } });

      //    GetStreams().ConfigureAwait(false); //update streams
      //  }
      //  catch (Exception e)
      //  {
      //    Dialogs.ShowDialog("Something went wrong...", e.Message, Material.Dialog.Icons.DialogIconKind.Error);
      //  }
      //}
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
            //ConnectorGSA.Commands.OpenFile(path, true);
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


  }
}
