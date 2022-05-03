using System;
using System.Collections.Generic;
using System.Text;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Analysis;
using System.Linq;

namespace Objects.Structural.GSA.Geometry
{
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
    public GSAConstraintCondition([SchemaParamInfo("A list of directions the X direction is coupled with (should be x, yy and/or zz)")] List<string> X = null,
      [SchemaParamInfo("A list of directions the Y direction is coupled with (should be y, xx and/or zz)")] List<string> Y = null,
      [SchemaParamInfo("A list of directions the Z direction is coupled with (should be z, xx and/or yy)")] List<string> Z = null,
      [SchemaParamInfo("A list of directions the XX direction is coupled with (should be xx)")] List<string> XX = null,
      [SchemaParamInfo("A list of directions the YY direction is coupled with (should be yy)")] List<string> YY = null,
      [SchemaParamInfo("A list of directions the ZZ direction is coupled with (should be zz)")] List<string> ZZ = null)
    {
      this.X = X;
      this.Y = Y;
      this.Z = Z;
      this.XX = XX;
      this.YY = YY;
      this.ZZ = ZZ;
    }
  }


  //public class GSAConstraintCondition : Base
  //{
  //  public List<AxisDirection6> X { get; set; }
  //  public List<AxisDirection6> Y { get; set; }
  //  public List<AxisDirection6> Z { get; set; }
  //  public List<AxisDirection6> XX { get; set; }
  //  public List<AxisDirection6> YY { get; set; }
  //  public List<AxisDirection6> ZZ { get; set; }
  //  public GSAConstraintCondition() { }

  //  [SchemaInfo("GSAConstraintCondition", "Creates a custom link description for a rigid constraint (ie. to be used with rigid contraints with custom linkage type)", "GSA", "Geometry")]
  //  public GSAConstraintCondition([SchemaParamInfo("A list of directions the X direction is coupled with (should be x, yy and/or zz)")] List<AxisDirection6> X = null,
  //    [SchemaParamInfo("A list of directions the Y direction is coupled with (should be y, xx and/or zz)")] List<AxisDirection6> Y = null,
  //    [SchemaParamInfo("A list of directions the Z direction is coupled with (should be z, xx and/or yy)")] List<AxisDirection6> Z = null,
  //    [SchemaParamInfo("A list of directions the XX direction is coupled with (should be xx)")] List<AxisDirection6> XX = null,
  //    [SchemaParamInfo("A list of directions the YY direction is coupled with (should be yy)")] List<AxisDirection6> YY = null,
  //    [SchemaParamInfo("A list of directions the ZZ direction is coupled with (should be zz)")] List<AxisDirection6> ZZ = null)
  //  {
  //    this.X = X;
  //    this.Y = Y;
  //    this.Z = Z;
  //    this.XX = XX;
  //    this.YY = YY;
  //    this.ZZ = ZZ;
  //  }
  //}

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
}
