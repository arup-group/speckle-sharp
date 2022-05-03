using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Analysis;
using System.Linq;

namespace Objects.Structural.GSA.Geometry
{
  public class GSAGeneralisedRestraint : Base
  {
    public int? nativeId { get; set; }
    public string name { get; set; }
    public Restraint restraint { get; set; }

    [Chunkable(250)]
    public List<Node> nodes { get; set; }
    public List<string> nodeRefs { get; set; }

    [DetachProperty]
    public List<GSAStage> stages { get; set; }
    public GSAGeneralisedRestraint() { }

    [SchemaInfo("GSAGeneralisedRestraint", "Creates a Speckle structural generalised restraint (a set of restraint conditions to be applied to a list of nodes) for GSA", "GSA", "Geometry")]
    public GSAGeneralisedRestraint(Restraint restraint, List<Node> nodes, List<GSAStage> stages, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.restraint = restraint;
      this.nodes = nodes;
      this.stages = stages;

      this.nodeRefs = nodes.Select(n => n.applicationId).ToList();
    }
  }
}
