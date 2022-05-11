using Objects.Geometry;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using System.Collections.Generic;
using System.Linq;

namespace Objects.Structural.GSA.Geometry
{
  public class GSAPolyline : Base
  {
    public string name { get; set; }
    public int? nativeId { get; set; }    
    public Polyline description { get; set; }
    public string colour { get; set; }
    public GSAGridPlane gridPlane { get; set; }
    public GSAPolyline() { }

    [SchemaInfo("GSAPolyline", "Creates a Speckle structural polyline for GSA", "GSA", "Geometry")]
    public GSAPolyline(Polyline description, string colour = "NO_RGB", GSAGridPlane gridPlane = null, string name = null, int? nativeId = null)
    {
      this.name = name;
      this.nativeId = nativeId;
      this.description = description;
      this.colour = colour;
      this.gridPlane = gridPlane;
    }

    [SchemaInfo("GSAPolyline (from coordinates)", "Creates a Speckle structural polyline from coordinates for GSA", "GSA", "Geometry")]
    public GSAPolyline(List<double> coordinatesArray, string colour = "NO_RGB", GSAGridPlane gridPlane = null, string name = null, int? nativeId = null)
    {
      this.name = name;
      this.nativeId = nativeId;
      this.description = new Polyline(coordinatesArray);
      this.colour = colour;
      this.gridPlane = gridPlane;

    }
  }
}
