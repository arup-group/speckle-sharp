using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadThermal1d : Load
  {
    public int? nativeId { get; set; }

    //[DetachProperty]
    [Chunkable(500)]
    public List<Element1D> elements { get; set; }
    public List<string> elementRefs { get; set; }
    public Thermal1dLoadType type { get; set; }
    public List<double> values { get; set; }

    public GSALoadThermal1d() { }

    [SchemaInfo("GSALoadThermal1d ", "Creates a Speckle 1d thermal load for GSA", "GSA", "Loading")]
    public GSALoadThermal1d(LoadCase loadCase, Thermal1dLoadType type,
      [SchemaParamInfo("A list that represents load magnitude or load magnitude and positions (number of values varies based on load type - Uniform: 1 value (constant temperature) or Gradient in Y or Z: 4 (position 1, temperature at position 1, position 2, temperature at position 2))")] List<double> values,
      List<Element1D> elements = null, string name = null, int? nativeId = null)
    {
      if (type == Thermal1dLoadType.Uniform && values.Count != 1)
        throw new ArgumentException("Number of values provided does not match the selected load type of uniform");
      if ((type == Thermal1dLoadType.GradientInY || type == Thermal1dLoadType.GradientInZ) && values.Count != 4)
        throw new ArgumentException("Number of values provided does not match the selected load type of gradient");
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.elements = elements;
      this.type = type;
      this.values = values;

      this.elementRefs = elements.Select(e => e.applicationId).ToList();
    }
  }

  public enum Thermal1dLoadType
  {
    NotSet = 0,
    Uniform,
    GradientInY,
    GradientInZ
  }
}
