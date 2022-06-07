using DesktopUI2.Models;
using DesktopUI2.Models.Filters;
using DesktopUI2.Models.Settings;
using DesktopUI2.ViewModels;
using Sentry.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesktopUI2
{
  public interface IConnectorBindingsStandalone 
  {
    public abstract ResultSettings ResultSettings { get; set; }

    #region abstract methods
    public abstract void NewFile();
    public abstract void OpenFile(string filePath);

    #endregion
  }
}
