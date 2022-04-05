using Speckle.Core.Kits;
using System.Collections.Generic;
using Objects.Structural.Loading;
using System;
using Speckle.Core.Models;

namespace Objects.Structural.GSA.Analysis
{
  public class GSACombinationCase : LoadCombination
  {
    public int? nativeId { get; set; }
    public GSACombinationCase() { }

    [SchemaInfo("GSACombinationCase", "Creates a GSA combination case", "GSA", "Loading")]
    public GSACombinationCase(string name,
        [SchemaParamInfo("A list of load cases")] List<Base> loadCases,
        [SchemaParamInfo("A list of load factors (to be mapped to provided load cases)")] List<double> loadFactors, CombinationType combinationType = CombinationType.LinearAdd, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;

      if (loadCases.Count != loadFactors.Count)
        throw new ArgumentException("Number of load cases provided does not match number of load factors provided");

      this.loadFactors = loadFactors;
      this.loadCases = loadCases;
      this.combinationType = combinationType;
      this.nativeId = nativeId;
    }
  }
}
