using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DesktopUI2.ViewModels;
using Speckle.Core.Credentials;
using System.Collections.Generic;
using System;
using System.Reactive;
using ReactiveUI;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;

namespace DesktopUI2.Views.Windows.Dialogs
{
  public partial class NewStreamDialog : DialogUserControl
  {
    public static NewStreamDialog Instance { get; private set; }
    public NewStreamDialog()
    {
      InitializeComponent();
      Instance = this;
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public void Close_Click(object sender, RoutedEventArgs e)
    {
      Instance.Close(false);
    }
  }
}
