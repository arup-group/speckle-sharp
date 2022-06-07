using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Geometry;
using Objects.Structural.Materials;

namespace Objects.Structural.Properties
{
  public class PropertyLink : Property
  {
    public PropertyLink() { }

    [SchemaInfo("PropertyLink", "Creates a Speckle structural link property", "Structural", "Properties")]
    public PropertyLink(string name)
    {
      this.name = name;
    }
  }
}
