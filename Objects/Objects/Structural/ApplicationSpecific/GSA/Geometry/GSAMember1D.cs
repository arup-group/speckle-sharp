using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using System.Linq;

namespace Objects.Structural.GSA.Geometry //GSA.Geometry?
{
  public class GSAMember1D : Element1D
  {
    public int? nativeId { get; set; }
    public int group { get; set; }
    public string colour { get; set; }
    public bool isDummy { get; set; }
    public bool intersectsWithOthers { get; set; }
    public double targetMeshSize { get; set; }

    public GSAMember1D() { }

    [SchemaInfo("GSAMember1D (from local axis)", "Creates a Speckle structural 1D member for GSA (from local axis)", "GSA", "Geometry")]
    public GSAMember1D(ICurve baseLine, Property1D property, MemberType memberType, ElementType1D analysisType, string name = null, Restraint end1Releases = null, Restraint end2Releases = null, Vector end1Offset = null, Vector end2Offset = null, Plane localAxis = null, int? nativeId = null, bool isDummy = false)
    {
      this.name = name;
      this.nativeId = nativeId;
      this.baseLine = baseLine;
      this.property = property;
      this.memberType = memberType;
      this.type = analysisType;
      this.end1Releases = end1Releases == null ? new Restraint("FFFFFF") : end1Releases;
      this.end2Releases = end2Releases == null ? new Restraint("FFFFFF") : end2Releases;
      this.end1Offset = end1Offset == null ? new Vector(0, 0, 0) : end1Offset;
      this.end2Offset = end2Offset == null ? new Vector(0, 0, 0) : end2Offset;
      this.localAxis = localAxis;
      this.isDummy = isDummy;
    }

    [SchemaInfo("GSAMember1D (from orientation node and angle)", "Creates a Speckle structural 1D member for GSA (from orientation node and angle)", "GSA", "Geometry")]
    public GSAMember1D(ICurve baseLine, Property1D property, MemberType memberType, ElementType1D analysisType, string name = null, Restraint end1Releases = null, Restraint end2Releases = null, Vector end1Offset = null, Vector end2Offset = null, GSANode orientationNode = null, double orientationAngle = 0, int? nativeId = null, bool isDummy = false)
    {
      this.name = name;
      this.nativeId = nativeId;
      this.baseLine = baseLine;
      this.property = property;
      this.memberType = memberType;
      this.type = analysisType;
      this.end1Releases = end1Releases == null ? new Restraint("FFFFFF") : end1Releases;
      this.end2Releases = end2Releases == null ? new Restraint("FFFFFF") : end2Releases;
      this.end1Offset = end1Offset == null ? new Vector(0, 0, 0) : end1Offset;
      this.end2Offset = end2Offset == null ? new Vector(0, 0, 0) : end2Offset;
      this.orientationNode = orientationNode;
      this.orientationAngle = orientationAngle;
      this.isDummy = isDummy;
    }

    [SchemaInfo("GSAMember1D (from local axis and topology)", "Creates a Speckle structural 1D member for GSA (from local axis and topology)", "GSA", "Geometry")]
    public GSAMember1D(List<Node> topology, Property1D property, MemberType memberType, ElementType1D analysisType, string name = null, Restraint end1Releases = null, Restraint end2Releases = null, Vector end1Offset = null, Vector end2Offset = null, Plane localAxis = null, int? nativeId = null, bool isDummy = false)
    {
      this.name = name;
      this.nativeId = nativeId;
      this.topology = topology;
      this.property = property;
      this.memberType = memberType;
      this.type = analysisType;
      this.end1Releases = end1Releases == null ? new Restraint("FFFFFF") : end1Releases;
      this.end2Releases = end2Releases == null ? new Restraint("FFFFFF") : end2Releases;
      this.end1Offset = end1Offset == null ? new Vector(0, 0, 0) : end1Offset;
      this.end2Offset = end2Offset == null ? new Vector(0, 0, 0) : end2Offset;
      this.localAxis = localAxis;
      this.isDummy = isDummy;

      this.topologyRefs = topology.Select(n => n.applicationId).ToList();
    }
  }
}
