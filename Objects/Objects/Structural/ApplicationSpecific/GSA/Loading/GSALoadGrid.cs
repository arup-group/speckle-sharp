using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading;

public abstract class GSALoadGrid : Load
{
  public int? nativeId { get; set; }
  public GSAGridSurface gridSurface { get; set; }
  public Axis loadAxis { get; set; }
  public LoadDirection2D direction { get; set; }
  public GSALoadGrid() { }

  public GSALoadGrid(LoadCase loadCase, GSAGridSurface gridSurface, Axis loadAxis, LoadDirection2D direction, string name = null, int? nativeId = null)
  {
    this.nativeId = nativeId;
    this.name = name;
    this.loadCase = loadCase;
    this.gridSurface = gridSurface;
    this.loadAxis = loadAxis;
    this.direction = direction;
    this.units = "kN/m2";
  }
}
