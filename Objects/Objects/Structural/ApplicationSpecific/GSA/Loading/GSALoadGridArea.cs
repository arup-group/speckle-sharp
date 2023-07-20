using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading;

public class GSALoadGridArea : GSALoadGrid
{
  public GSAPolyline polyline { get; set; }
  public bool isProjected { get; set; }
  public double value { get; set; }
  public GSALoadGridArea() { }

  [SchemaInfo("GSALoadGridArea (explicit polyline)", "Creates a Speckle structural grid area load (by polyline) for GSA", "GSA", "Loading")]
  public GSALoadGridArea(LoadCase loadCase, GSAGridSurface gridSurface, Polyline polyline, double value, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, bool isProjected = false, string name = null, int? nativeId = null)
  {
    this.name = name;
    this.loadCase = loadCase;
    this.nativeId = nativeId;
    this.gridSurface = gridSurface;
    this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
    this.direction = direction;
    this.polyline = new GSAPolyline(polyline);
    this.isProjected = isProjected;
    this.value = value;
  }

  [SchemaInfo("GSALoadGridArea (GSAPolyline)", "Creates a Speckle structural grid area load (by GSAPolyline) for GSA", "GSA", "Loading")]
  public GSALoadGridArea(LoadCase loadCase, GSAGridSurface gridSurface, GSAPolyline polyline, double value, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, bool isProjected = false, string name = null, int? nativeId = null)
  {
    this.name = name;
    this.loadCase = loadCase;
    this.nativeId = nativeId;
    this.gridSurface = gridSurface;
    this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
    this.direction = direction;
    this.polyline = polyline;
    this.isProjected = isProjected;
    this.value = value;
      }

  [SchemaInfo("GSALoadGridArea (whole plane)", "Creates a Speckle structural grid area load for GSA", "GSA", "Loading")]
  public GSALoadGridArea(LoadCase loadCase, GSAGridSurface gridSurface, double value, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, bool isProjected = false, string name = null, int? nativeId = null)
  {
    this.name = name;
    this.loadCase = loadCase;
    this.nativeId = nativeId;
    this.gridSurface = gridSurface;
    this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
    this.direction = direction;
    this.isProjected = isProjected;
    this.value = value;
  }
}
