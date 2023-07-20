using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Objects.Geometry;
using Objects.Structural.Properties;
using Objects.BuiltElements;

namespace Objects.Structural.GSA.Geometry;

public class GSAGridLine : GridLine
{
  public int? nativeId { get; set; }
  public GSAGridLine() { }

  [SchemaInfo("GSAGridLine", "Creates a Speckle structural grid line for GSA", "GSA", "Geometry")]
  public GSAGridLine(string label, ICurve line, int? nativeId = null)
  {
    this.nativeId = nativeId;
    this.label = label;
    this.baseLine = line;
  }
}
