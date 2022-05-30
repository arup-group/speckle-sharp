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
    public abstract string Layer { get; set; } // should be made a settings item
    public abstract string Units { get; set; } // should be made a settings item
    public abstract double CoincidentNodeAllowance { get; set; } // should be made a settings item
    public abstract bool SendResults { get; set; }

    public abstract ResultSettings ResultSettings { get; set; }
    //public List<StreamState> SavedStreamStates = new List<StreamState>();

    #region abstract methods
    public abstract void NewFile();
    public abstract void OpenFile(string filePath);

    #endregion
  }
}
