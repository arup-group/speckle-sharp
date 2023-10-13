using System;
using System.Collections.Generic;
using System.Text;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Analysis;
using System.Linq;

namespace Objects.Structural.GSA.Geometry;

public class GSARigidConstraint : Base
{
  public string name { get; set; }
  public int? nativeId { get; set; }
  public Node primaryNode { get; set; }
  public string primaryNodeRef { get; set; }

  [Chunkable(250)]
  public List<Node> constrainedNodes { get; set; }
  public List<string> constrainedNodeRefs { get; set; }
  public Base parentMember { get; set; }
  public List<GSAStage> stages { get; set; }
  public LinkageType type { get; set; }

  public GSAConstraintCondition constraintCondition { get; set; }
  public GSARigidConstraint() { }

  [SchemaInfo("GSARigidConstraint (custom link)", "Creates a Speckle structural rigid restraint (a set of nodes constrained to move as a rigid body) for GSA", "GSA", "Geometry")]
  public GSARigidConstraint(Node primaryNode, List<Node> constrainedNodes, GSAConstraintCondition coupledDirections, List<GSAStage> stageList = null, string name = null, int? nativeId = null)
  {
    this.name = name;
    this.nativeId = nativeId;
    this.primaryNode = primaryNode;
    this.constrainedNodes = constrainedNodes;
    this.parentMember = parentMember;
    this.stages = stageList;
    this.type = LinkageType.Custom;
    this.constraintCondition = coupledDirections;

    this.constrainedNodeRefs = constrainedNodes.Select(n => n.applicationId).ToList();
  }

  [SchemaInfo("GSARigidConstraint", "Creates a Speckle structural rigid restraint (a set of nodes constrained to move as a rigid body) for GSA", "GSA", "Geometry")]
  public GSARigidConstraint(Node primaryNode, List<Node> constrainedNodes, LinkageType type, List<GSAStage> stageList = null, string name = null, int? nativeId = null)
  {
    this.name = name;
    this.nativeId = nativeId;
    this.primaryNode = primaryNode;
    this.constrainedNodes = constrainedNodes;
    this.stages = stageList;
    this.type = type;

    this.constrainedNodeRefs = constrainedNodes.Select(n => n.applicationId).ToList();
  }
}

public class GSAConstraintCondition : Base
{
  public List<string> X { get; set; }
  public List<string> Y { get; set; }
  public List<string> Z { get; set; }
  public List<string> XX { get; set; }
  public List<string> YY { get; set; }
  public List<string> ZZ { get; set; }
  public GSAConstraintCondition() { }

  [SchemaInfo("GSAConstraintCondition", "Creates a custom link description for a rigid constraint (ie. to be used with rigid contraints with custom linkage type)", "GSA", "Geometry")]
  public GSAConstraintCondition([SchemaParamInfo("A comma-separated string denoting the coupled degrees of freedom for X (can be x, yy and/or zz degrees of freedom, ex. 'x,yy' for x and yy degrees of freedom)")] string X = null,
    [SchemaParamInfo("A comma-separated string denoting the coupled degrees of freedom for Y (should be y, xx and/or zz degrees of freedom, ex. 'xx' for the xx degree of freedom)")] string Y = null,
    [SchemaParamInfo("A comma-separated string denoting the coupled degrees of freedom for Z (should be z, xx and/or yy degrees of freedom, ex. 'z, xx, yy' for the z, xx and yy degrees of freedom)")] string Z = null,
    [SchemaParamInfo("Whether the XX degrees of freedom are coupled")] bool XX = false,
    [SchemaParamInfo("Whether the YY degrees of freedom are couple")] bool YY = false,
    [SchemaParamInfo("Whether the YY degrees of freedom are couple")] bool ZZ = false)
  {
    if (X != null) this.X = X.Split(',').Select(s => s.Trim()).ToList();
    if (Y != null) this.Y = Y.Split(',').Select(s => s.Trim()).ToList();
    if (Z != null) this.Z = Z.Split(',').Select(s => s.Trim()).ToList();
    if (XX) this.XX = new List<string> { "XX" };
    if (YY) this.YY = new List<string> { "YY" };
    if (ZZ) this.ZZ = new List<string> { "ZZ" };

  }
}


public enum AxisDirection6
{
  NotSet = 0,
  X,
  Y,
  Z,
  XX,
  YY,
  ZZ
}

public enum LinkageType
{
  NotSet = 0,
  ALL,
  XY_PLANE,
  YZ_PLANE,
  ZX_PLANE,
  XY_PLATE,
  YZ_PLATE,
  ZX_PLATE,
  PIN,
  XY_PLANE_PIN,
  YZ_PLANE_PIN,
  ZX_PLANE_PIN,
  XY_PLATE_PIN,
  YZ_PLATE_PIN,
  ZX_PLATE_PIN,
  Custom
}
