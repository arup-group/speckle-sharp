using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Objects.Structural.GSA.Geometry;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadGridLine : GSALoadGrid
  {
    public GSAPolyline polyline { get; set; }
    public bool isProjected { get; set; }
    public List<double> values { get; set; }
    public GSALoadGridLine() { }

    [SchemaInfo("GSALoadGridLine (explicit polyline)", "Creates a Speckle structural grid line load (based on an explicit polyline) for GSA", "GSA", "Loading")]
    public GSALoadGridLine(LoadCase loadCase, GSAGridSurface gridSurface, Polyline polyline, List<double> values, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, bool isProjected = false, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.gridSurface = gridSurface;
      this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
      this.direction = direction;
      this.polyline = new GSAPolyline(polyline);
      this.isProjected = isProjected;
      this.values = values;
    }

    [SchemaInfo("GSALoadGridLine (GSAPolyline)", "Creates a Speckle structural grid line load (based on a GSAPolyline) for GSA", "GSA", "Loading")]
    public GSALoadGridLine(LoadCase loadCase, GSAGridSurface gridSurface, GSAPolyline polyline, List<double> values, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, bool isProjected = false, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.gridSurface = gridSurface;
      this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
      this.direction = direction;
      this.polyline = polyline;
      this.isProjected = isProjected;
      this.values = values;
    }
    }
}
