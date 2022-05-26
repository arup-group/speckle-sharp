using System.Collections.Generic;

namespace DesktopUI2.Models
{
  public class ResultSettings
  {
    public List<ResultSettingItem> ResultSettingItems { get; set; }
    public string CasesDescription { get; set; }

    public int Additional1DPositions { get; set; } = 3;


    public ResultSettings()
    {

      //ResultSettingItems = Instance.GsaModel.Proxy.ResultTypeStrings.Keys.Select(k => new ResultSettingItem(GsaProxy.ResultTypeStrings[k], k, true)).ToList();

      ResultSettingItems = new List<ResultSettingItem>()
      {
        new ResultSettingItem("Nodal Displacements", "NodalDisplacements", true),
        new ResultSettingItem("Nodal Velocity", "NodalVelocity", false),
        new ResultSettingItem("Nodal Acceleration", "NodalAcceleration", false),
        new ResultSettingItem("Nodal Reaction", "NodalReaction", true),
        new ResultSettingItem("Constraint Forces", "ConstraintForces", true),
        new ResultSettingItem("1D Element Displacement", "Element1dDisplacement", false),
        new ResultSettingItem("1D Element Force", "Element1dForce", true),
        new ResultSettingItem("2D Element Displacement", "Element2dDisplacement", true),
        new ResultSettingItem("2D Element Projected Moment", "Element2dProjectedMoment",true),
        new ResultSettingItem("2D Element Projected Force", "Element2dProjectedForce", false),
        new ResultSettingItem("2D Element Projected Stress - Bottom", "Element2dProjectedStressBottom", false),
        new ResultSettingItem("2D Element Projected Stress - Middle", "Element2dProjectedStressMiddle", false),
        new ResultSettingItem("2D Element Projected Stress - Top", "Element2dProjectedStressTop", false),
        new ResultSettingItem("Assembly Forces and Moments", "AssemblyForcesAndMoments", true),
        new ResultSettingItem("Total Loads & Reactions", "TotalLoadsAndReactions", false),
        new ResultSettingItem("Dynamic Summary", "DynamicSummary", false),
      };
    }
  }
}