using System;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Geometry;
public class GSAElement1D : Element1D
{
  public int? nativeId { get; set; } //equiv to num record of gwa keyword
  public int group { get; set; }
  public string colour { get; set; }
  public string action { get; set; }
  public bool isDummy { get; set; }
  public string parentApplicationId { get; set; }
  public GSAElement1D() { }

  [SchemaInfo("GSAElement1D (from local axis)", "Creates a Speckle structural 1D element for GSA (from local axis)", "GSA", "Geometry")]
  public GSAElement1D(ICurve baseLine, Property property, ElementType1D type = ElementType1D.Beam, string name = null,
      [SchemaParamInfo("If null, restraint condition defaults to unreleased (fully fixed translations and rotations)")] Restraint end1Releases = null,
      [SchemaParamInfo("If null, restraint condition defaults to unreleased (fully fixed translations and rotations)")] Restraint end2Releases = null,
      [SchemaParamInfo("If null, defaults to no offsets")] Vector end1Offset = null,
      [SchemaParamInfo("If null, defaults to no offsets")] Vector end2Offset = null, Plane localAxis = null, int group = 1, string colour = "NO_RGB", bool isDummy = false, int? nativeId = null)
  {
    if (type == ElementType1D.Link)
    {
      if (!(property.GetType() == typeof(PropertyLink)) && !(property.GetType().IsSubclassOf(typeof(PropertyLink))))
        throw new Exception("Link property must be provided if element is of type link");
    }
    else
    {
      if (!(property.GetType() == typeof(Property1D)) && !(property.GetType().IsSubclassOf(typeof(Property1D))))
        throw new Exception("1d property must be provided (mismatch between provided property and element type");
    }

    this.name = name;
    this.nativeId = nativeId;
    this.baseLine = baseLine;
    this.property = property;
    this.type = type;
    this.end1Releases = end1Releases == null ? new Restraint("FFFFFF") : end1Releases;
    this.end2Releases = end2Releases == null ? new Restraint("FFFFFF") : end2Releases;
    this.end1Offset = end1Offset == null ? new Vector(0, 0, 0) : end1Offset;
    this.end2Offset = end2Offset == null ? new Vector(0, 0, 0) : end2Offset;
    this.localAxis = localAxis;
    this.group = group;
    this.colour = colour;
    this.isDummy = isDummy;
  }

  [SchemaInfo("GSAElement1D (from orientation node and angle)", "Creates a Speckle structural 1D element for GSA (from orientation node and angle)", "GSA", "Geometry")]
  public GSAElement1D(ICurve baseLine, Property property, ElementType1D type = ElementType1D.Beam, string name = null,
      [SchemaParamInfo("If null, restraint condition defaults to unreleased (fully fixed translations and rotations)")] Restraint end1Releases = null,
      [SchemaParamInfo("If null, restraint condition defaults to unreleased (fully fixed translations and rotations)")] Restraint end2Releases = null,
      [SchemaParamInfo("If null, defaults to no offsets")] Vector end1Offset = null,
      [SchemaParamInfo("If null, defaults to no offsets")] Vector end2Offset = null,
      Node orientationNode = null, double orientationAngle = 0, int group = 1, string colour = "NO_RGB", bool isDummy = false, int ? nativeId = null)
  {
    if (type == ElementType1D.Link)
    {
      if (!(property.GetType() == typeof(PropertyLink)) && !(property.GetType().IsSubclassOf(typeof(PropertyLink))))
        throw new Exception("Link property must be provided if element is of type link");
    }
    else
    {
      if (!(property.GetType() == typeof(Property1D)) && !(property.GetType().IsSubclassOf(typeof(Property1D))))
        throw new Exception("1d property must be provided (mismatch between provided property and element type");
    }

    this.name = name;
    this.nativeId = nativeId;
    this.baseLine = baseLine;
    this.property = property;
    this.type = type;
    this.end1Releases = end1Releases == null ? new Restraint("FFFFFF") : end1Releases;
    this.end2Releases = end2Releases == null ? new Restraint("FFFFFF") : end2Releases;
    this.end1Offset = end1Offset == null ? new Vector(0, 0, 0) : end1Offset;
    this.end2Offset = end2Offset == null ? new Vector(0, 0, 0) : end2Offset;
    this.orientationNode = orientationNode;
    this.orientationAngle = orientationAngle;
    this.group = group;
    this.colour = colour;
    this.isDummy = isDummy;
  }
}
