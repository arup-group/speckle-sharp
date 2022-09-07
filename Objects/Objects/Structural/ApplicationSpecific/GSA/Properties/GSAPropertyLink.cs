using Speckle.Core.Kits;
using Speckle.Core.Models;
using Objects.Structural.Properties;
using Objects.Structural.Materials;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;

namespace Objects.Structural.GSA.Properties
{
  public class GSAPropertyLink : PropertyLink
  {
    public int? nativeId { get; set; }
    public string colour { get; set; }
    public LinkPropertyType type { get; set; }
    public GSAConstraintCondition constraintCondition { get; set; }
    public GSAPropertyLink() { }

    [SchemaInfo("GSAPropertyLink", "Creates a Speckle structural link element property for GSA", "GSA", "Properties")]
    public GSAPropertyLink(string name, NonCustomLinkPropertyType type = NonCustomLinkPropertyType.ALL, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.type = (LinkPropertyType)type;
    }

    [SchemaInfo("GSAPropertyLink (custom)", "Creates a Speckle structural link element property (with custom constraint conditions) for GSA", "GSA", "Properties")]
    public GSAPropertyLink(string name, GSAConstraintCondition coupledDirections, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.type = LinkPropertyType.Custom;
      this.constraintCondition = coupledDirections;
    }
  }

  public enum NonCustomLinkPropertyType
  {
    NotSet = 0,
    ALL,
    XY_PLANE,
    YZ_PLANE,
    ZX_PLANE,
    PIN,
    XY_PLANE_PIN,
    YZ_PLANE_PIN,
    ZX_PLANE_PIN,
    BAR,
    TENS,
    COMP
  }

  public enum LinkPropertyType 
  {
    NotSet = 0,
    ALL,
    XY_PLANE,
    YZ_PLANE,
    ZX_PLANE,
    PIN,
    XY_PLANE_PIN,
    YZ_PLANE_PIN,
    ZX_PLANE_PIN,
    BAR,
    TENS,
    COMP,
    Custom
  }

}
