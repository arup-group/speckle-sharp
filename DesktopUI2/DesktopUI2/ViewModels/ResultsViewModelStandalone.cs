using Avalonia.Controls;
using DesktopUI2.Models;
using ReactiveUI;

namespace DesktopUI2.ViewModels
{
  public class ResultsViewModelStandalone : ViewModelBase
  {
    public ResultSettings ResultSettings { get; set; } = new ResultSettings();

    public void SaveCommand(Window window)
    {
      if (window != null)
        window.Close(true);
    }

  }
}
