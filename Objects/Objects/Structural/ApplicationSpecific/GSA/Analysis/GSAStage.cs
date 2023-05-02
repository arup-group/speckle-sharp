using System.Collections.Generic;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Newtonsoft.Json;

namespace Objects.Structural.GSA.Analysis
{
  public class GSAStage : Base
  {
    public int? nativeId { get; set; }
    public string name { get; set; }
    public string colour { get; set; }

    [DetachProperty]
    [Chunkable(1000)]
    public List<Base> elements { get; set; }
    public double creepFactor { get; set; } //Phi
    public int stageTime { get; set; } //number of days

    [DetachProperty]
    [Chunkable(1000)]
    public List<Base> lockedElements { get; set; } //elements not part of the current analysis stage
    public GSAStage() { }

    [SchemaInfo("GSAStage", "Creates a Speckle structural analysis stage for GSA", "GSA", "Analysis")]
    public GSAStage(List<Base> elements = null, double creepFactor = 0, int stageTime = 0, List<Base> lockedElements = null, string colour = null, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.colour = colour;
      this.elements = elements;
      this.creepFactor = creepFactor;
      this.stageTime = stageTime;
      this.lockedElements = lockedElements;
    }
  }
}
