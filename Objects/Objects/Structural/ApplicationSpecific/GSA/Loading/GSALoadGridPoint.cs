using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Objects.Structural.GSA.Geometry;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadGridPoint : GSALoadGrid
  {
    public Point position { get; set; }
    public double value { get; set; }
    public GSALoadGridPoint() { }

    [SchemaInfo("GSALoadGridPoint", "Creates a Speckle structural grid point load for GSA", "GSA", "Loading")]
    public GSALoadGridPoint(LoadCase loadCase, GSAGridSurface gridSurface, Point position, double value, Axis loadAxis = null, LoadDirection2D direction = LoadDirection2D.Z, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.gridSurface = gridSurface;
      this.loadAxis = loadAxis == null ? new Axis("Global", AxisType.Cartesian, new Plane(new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0), new Vector(0, 1, 0))) : loadAxis;
      this.direction = direction;
      this.position = position;
      this.value = value;
    }
  }
}
