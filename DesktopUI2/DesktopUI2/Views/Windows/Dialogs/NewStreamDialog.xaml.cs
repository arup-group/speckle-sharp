using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DesktopUI2.ViewModels;
using Speckle.Core.Credentials;
using System.Collections.Generic;

namespace DesktopUI2.Views.Windows.Dialogs
{
  public partial class NewStreamDialog : DialogUserControl
  {
    public static NewStreamDialog Instance { get; private set; }
    public Account Account { get; set; }
    public string StreamName { get; set; }
    public string Description { get; set; }
    public bool IsPublic { get; set; }

    public NewStreamDialog() 
    {
      InitializeComponent();
      Instance = this;
    }

    public NewStreamDialog(List<AccountViewModel> accounts)
    {
      InitializeComponent();
      Instance = this;
      var combo = this.FindControl<ComboBox>("accounts");
      combo.Items = accounts;
      try
      {
        combo.SelectedIndex = accounts.FindIndex(x => x.Account.isDefault);
      }
      catch
      {
        combo.SelectedIndex = 0;
      }

    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public void Create_Click(object sender, RoutedEventArgs e)
    {
      var isPublic = this.FindControl<ToggleSwitch>("isPublic").IsChecked;
      //too lazy to create a view model for this or properly style the Dialogs
      Account = (this.FindControl<ComboBox>("accounts").SelectedItem as AccountViewModel).Account;
      StreamName = this.FindControl<TextBox>("name").Text;
      Description = this.FindControl<TextBox>("description").Text;
      IsPublic = isPublic.HasValue ? isPublic.Value : false;
      Instance.Close(true);
    }

    public void Close_Click(object sender, RoutedEventArgs e)
    {
      Instance.Close(false);
    }
  }
}
