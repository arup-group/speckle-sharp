﻿using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using System.Linq;

namespace Objects.Structural.GSA.Geometry
{
  public class GSAMember2D : Element2D
  {
    public int? nativeId { get; set; }
    public int group { get; set; }
    public string colour { get; set; }
    public bool isDummy { get; set; }
    public bool intersectsWithOthers { get; set; }
    public double targetMeshSize { get; set; }
    public List<List<Node>> voids { get; set; }
    public MemberType2D memberType { get; set; }
    
    public GSAMember2D() { }

    [SchemaInfo("GSAMember2D", "Creates a Speckle structural 2D member for GSA", "GSA", "Geometry")]
    public GSAMember2D([SchemaParamInfo("An ordered list of nodes which represents the perimeter of a member (ie. order of points should based on valid polyline)")] List<Node> perimeter,
        Property2D property, MemberType2D memberType,
        [SchemaParamInfo("A list of ordered lists of nodes representing the voids within a member (ie. order of points should be based on valid polyline)")] List<List<Node>> voids = null,
        double offset = 0, double orientationAngle = 0, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.topology = perimeter; //needs to be ordered properly (ie. matching the point order of a valid polyline)            
      this.property = property;
      this.memberType = memberType;
      this.voids = voids; //needs to be ordered properly (ie. matching the point order of a valid polyline)
      this.offset = offset;
      this.orientationAngle = orientationAngle;

      var coordinates = perimeter.SelectMany(x => x.basePoint.ToList()).ToList();
      coordinates.AddRange(perimeter[0].basePoint.ToList());
      this.outline = new Polyline(coordinates, this.units != null ? this.units : perimeter.FirstOrDefault().units);
    }
  }
}
