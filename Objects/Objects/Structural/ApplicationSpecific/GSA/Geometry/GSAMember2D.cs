using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using System.Linq;
using System;

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
    public AnalysisType2D analysisType { get; set; }

    public GSAMember2D() { }

    [SchemaInfo("GSAMember2D", "Creates a Speckle structural 2D member for GSA", "GSA", "Geometry")]
    public GSAMember2D([SchemaParamInfo("An ordered list of nodes which represents the perimeter of a member (ie. order of points should based on valid polyline)")] List<Node> perimeter,
        Property2D property, MemberType2D memberType = MemberType2D.Generic2D, AnalysisType2D analysisType = AnalysisType2D.Linear,
        [SchemaParamInfo("A list of ordered lists of nodes representing the voids within a member (ie. order of points should be based on valid polyline)")] List<List<Node>> voids = null,
        double offset = 0, double orientationAngle = 0, string name = null, int? nativeId = null, bool isDummy = false)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.topology = perimeter; //needs to be ordered properly (ie. matching the point order of a valid polyline)            
      this.property = property;
      this.memberType = (MemberType)Enum.Parse(typeof(MemberType), memberType.ToString());
      this.analysisType = analysisType;
      this.voids = voids; //needs to be ordered properly (ie. matching the point order of a valid polyline)
      this.offset = offset;
      this.orientationAngle = orientationAngle;
      this.isDummy = isDummy;

      var outline = new List<ICurve> { };
      var coordinates = perimeter.SelectMany(x => x.basePoint.ToList()).ToList();
      coordinates.AddRange(perimeter[0].basePoint.ToList());
      outline.Add(new Polyline(coordinates, this.units != null ? this.units : perimeter.FirstOrDefault().units));

      if(voids != null)
      {
        foreach(var v in voids)
        {
          var voidCoordinates = v.SelectMany(x => x.basePoint.ToList()).ToList();
          voidCoordinates.AddRange(v[0].basePoint.ToList());
          outline.Add(new Polyline(voidCoordinates, this.units != null ? this.units : v.FirstOrDefault().units));
        }
      }

      this.outline = outline;

      this.topologyRefs = topology.Select(n => n.applicationId).ToList();
    }

    [SchemaInfo("GSAMember2D (from polyline)", "Creates a Speckle structural 2D member for GSA", "GSA", "Geometry")]
    public GSAMember2D([SchemaParamInfo("A polyline which represents the perimeter of a member")] Polyline perimeter,
        Property2D property, MemberType2D memberType = MemberType2D.Generic2D, AnalysisType2D analysisType = AnalysisType2D.Linear,
        [SchemaParamInfo("A list of polylines which represent the voids within a member")] List<Polyline> voids = null,
        double offset = 0, double orientationAngle = 0, string name = null, int? nativeId = null, bool isDummy = false)
    {
      this.nativeId = nativeId;
      this.name = name;     
      this.topology = GetNodesFromPolyline(perimeter);           
      this.property = property;
      this.memberType = (MemberType)Enum.Parse(typeof(MemberType), memberType.ToString());
      this.analysisType = analysisType;

      var outlineLoops = new List<ICurve>() { perimeter };
      if (voids != null)
      {
        var voidLoops = new List<List<Node>>() { };
        foreach (var v in voids)
        {
          voidLoops.Add(GetNodesFromPolyline(v));
          outlineLoops.Add(v);
        }
        this.voids = voidLoops;
      }

      this.offset = offset;
      this.orientationAngle = orientationAngle;
      this.isDummy = isDummy;
      this.outline = outlineLoops;

      this.topologyRefs = topology.Select(n => n.applicationId).ToList();
    }
  }
}
