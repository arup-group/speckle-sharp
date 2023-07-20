using System.Collections.Generic;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace Objects.Structural.GSA.Geometry;

public class GSAGridSurface : Base
{
  public int? nativeId { get; set; }
  public string name { get; set; }
  public GSAGridPlane gridPlane { get; set; }

  [DetachProperty]
  [Chunkable(1000)]
  public List<Base> elements { get; set; }

  public double? tolerance { get; set; }
  public GridSurfaceElementType type { get; set; }
  public GridSurfaceSpanType span { get; set; }
  public double spanDirection { get; set; }
  public LoadExpansion loadExpansion { get; set; }

  public GSAGridSurface() { }

  [SchemaInfo("GSAGridSurface", "Creates a Speckle structural grid surface for GSA", "GSA", "Geometry")]
  public GSAGridSurface(GSAGridPlane gridPlane, [SchemaParamInfo("If null, element list defaults to all")] List<Base> elements = null, double? tolerance = null, GridSurfaceElementType type = GridSurfaceElementType.OneD, GridSurfaceSpanType span = GridSurfaceSpanType.OneWay, double spanDirection = 0, LoadExpansion loadExpansion = LoadExpansion.PlaneCorner, string name = null, int? nativeId = null)
  {
    this.nativeId = nativeId;
    this.name = name;
    this.gridPlane = gridPlane;
    this.elements = elements;
    this.tolerance = tolerance;
    this.type = type;
    this.spanDirection = spanDirection;
    this.span = span;
    this.loadExpansion = loadExpansion;
  }
}

public enum GridSurfaceElementType
{
  OneD,
  TwoD
}

public enum GridSurfaceSpanType
{
  OneWay = 1,
  TwoWay = 2,
  TwoWaySimple = 3,
  NotSet = 0,
}

public enum LoadExpansion
{
  Legacy = 1,
  PlaneAspect = 2,
  PlaneSmooth = 3,
  PlaneCorner = 4,
  NotSet = 0,
}
