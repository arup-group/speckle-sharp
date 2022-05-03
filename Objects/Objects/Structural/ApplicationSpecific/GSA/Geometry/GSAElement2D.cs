using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using System;
using System.Linq;

namespace Objects.Structural.GSA.Geometry
{
  public class GSAElement2D : Element2D
  {
    public int? nativeId { get; set; }
    public int group { get; set; }
    public string colour { get; set; }
    public bool isDummy { get; set; }
    public ElementType2D type { get; set; } //analysis formulation - ex. Quad4, Tri3
    public GSAElement2D() { }

    [SchemaInfo("GSAElement2D", "Creates a Speckle structural 2D element for GSA", "GSA", "Geometry")]
    public GSAElement2D(List<Node> nodes, Property2D property, ElementType2D type = ElementType2D.Quad4, string name = null, double offset = 0, double orientationAngle = 0, int group = 0, string colour = "NO_RGB", bool isDummy = false, int? nativeId = null)
    {
      switch (type)
      {
        case ElementType2D.Quad4:
          if (nodes.Count != 4)
            throw new ArgumentException("Number of nodes provided does not match the selected element type of Quad 4");
          break;
        case ElementType2D.Quad8:
          if (nodes.Count != 8)
            throw new ArgumentException("Number of nodes provided does not match the selected element type of Quad 8");
          break;
        case ElementType2D.Triangle3:
          if (nodes.Count != 3)
            throw new ArgumentException("Number of nodes provided does not match the selected element type of Triangle 3");
          break;
        case ElementType2D.Triangle6:
          if (nodes.Count != 6)
            throw new ArgumentException("Number of nodes provided does not match the selected element type of Triangle 6");
          break;
      }

      this.nativeId = nativeId;
      this.topology = nodes;
      this.property = property;
      this.type = type;
      this.name = name;
      this.offset = offset;
      this.orientationAngle = orientationAngle;
      this.group = group;
      this.colour = colour;
      this.isDummy = isDummy;
      this.memberType = MemberType.NotSet;

      this.topologyRefs = nodes.Select(n => n.applicationId).ToList();
    }
  }
}
