using Avalonia.Metadata;
using DesktopUI2.Models;
using DesktopUI2.Models.Scheduler;
using DesktopUI2.Views;
using ReactiveUI;
using Speckle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DesktopUI2.Models
{
  public class ResultSettings : ReactiveObject
  {
    private bool _sendResults = false;
    public bool SendResults
    {
      get => _sendResults;
      set
      {
        this.RaiseAndSetIfChanged(ref _sendResults, value);
      }
    }

    private bool _useServerTransport = true;
    public bool UseServerTransport
    {
      get => _useServerTransport;
      set
      {
        this.RaiseAndSetIfChanged(ref _useServerTransport, value);
      }
    }

    private bool _useLocalTransport = false;
    public bool UseLocalTransport
    {
      get => _useLocalTransport;
      set
      {
        this.RaiseAndSetIfChanged(ref _useLocalTransport, value);
      }
    }

    private bool _saveResultsToCsv = false;
    public bool SaveResultsToCsv
    {
      get => _saveResultsToCsv;
      set
      {
        this.RaiseAndSetIfChanged(ref _saveResultsToCsv, value);
      }
    }

    private bool _useLocalAxis = true;
    public bool UseLocalAxis
    {
      get => _useLocalAxis;
      set
      {
        this.RaiseAndSetIfChanged(ref _useLocalAxis, value);
      }
    }

    public List<ResultSettingItem> ResultSettingItems { get; set; }
    public string CasesDescription { get; set; }
    public int Additional1DPositions { get; set; } = 3;

    public ResultSettings()
    {
      ResultSettingItems = new List<ResultSettingItem>()
      {
        new ResultSettingItem("Nodal Displacements", "NodalDisplacements", true),
        new ResultSettingItem("Nodal Velocity", "NodalVelocity", false),
        new ResultSettingItem("Nodal Acceleration", "NodalAcceleration", false),
        new ResultSettingItem("Nodal Reaction", "NodalReaction", true),
        new ResultSettingItem("Constraint Forces", "ConstraintForces", false),
        new ResultSettingItem("1D Element Displacement", "Element1dDisplacement", false),
        new ResultSettingItem("1D Element Force", "Element1dForce", true),
        new ResultSettingItem("2D Element Displacement", "Element2dDisplacement", false),
        new ResultSettingItem("2D Element Projected Moment", "Element2dProjectedMoment",true),
        new ResultSettingItem("2D Element Projected Force", "Element2dProjectedForce", true),
        new ResultSettingItem("2D Element Projected Stress - Bottom", "Element2dProjectedStressBottom", false),
        new ResultSettingItem("2D Element Projected Stress - Middle", "Element2dProjectedStressMiddle", false),
        new ResultSettingItem("2D Element Projected Stress - Top", "Element2dProjectedStressTop", false),
        new ResultSettingItem("Assembly Forces and Moments", "AssemblyForcesAndMoments", false),
        new ResultSettingItem("Total Loads & Reactions", "TotalLoadsAndReactions", false),
        new ResultSettingItem("Dynamic Summary", "DynamicSummary", false),
      };
    }
  }
}