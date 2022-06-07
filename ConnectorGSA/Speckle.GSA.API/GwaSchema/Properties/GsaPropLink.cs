using System.Collections.Generic;

namespace Speckle.GSA.API.GwaSchema
{
  public class GsaPropLink : GsaRecord
  {
    public string Name { get => name; set { name = value; } }
    public Colour Colour = Colour.NO_RGB;
    public LinkType Type;
    public Dictionary<AxisDirection6, List<AxisDirection6>> LinkedCondition;

    public GsaPropLink() : base()
    {
      //Defaults
      Version = 3;
    }
  }
}
