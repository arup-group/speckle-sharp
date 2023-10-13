using System.Collections.Generic;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;
using Objects.Structural.Loading;
using GSANode = Objects.Structural.GSA.Geometry.GSANode;

namespace Objects.Structural.GSA.Loading;

public class GSALoadNode : LoadNode
{
  public int? nativeId { get; set; }
  public GSALoadNode() { }

  [SchemaInfo("GSALoadNode", "Creates a Speckle node load for GSA", "GSA", "Loading")]
  public GSALoadNode(LoadCase loadCase, List<GSANode> nodes, LoadDirection direction, double value, string name = null, int? nativeId = null)
  {
    this.nativeId = nativeId;
    this.name = name;
    this.loadCase = loadCase;
    List<Node> baseNodes = nodes.ConvertAll(x => (Node)x);
    this.nodes = baseNodes;
    this.direction = direction;
    this.value = value;
  }

  [SchemaInfo("GSALoadNode (user-defined axis)", "Creates a Speckle node load (user-defined axis) for GSA", "GSA", "Loading")]
  public GSALoadNode(LoadCase loadCase, List<Node> nodes, Axis loadAxis, LoadDirection direction, double value, string name = null, int? nativeId = null)
  {
    this.nativeId = nativeId;
    this.name = name;
    this.loadCase = loadCase;
    this.nodes = nodes;
    this.loadAxis = loadAxis;
    this.direction = direction;
    this.value = value;
  }
}
