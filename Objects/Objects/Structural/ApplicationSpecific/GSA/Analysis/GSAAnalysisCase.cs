using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Loading;
using System;

namespace Objects.Structural.GSA.Analysis;

public class GSAAnalysisCase : Base
{
  public int? nativeId { get; set; }
  public string name { get; set; }

  //[DetachProperty]
  //public GSAAnalysisTask task { get; set; } //task reference

  [DetachProperty]
  public List<LoadCase> loadCases { get; set; }
  public List<double> loadFactors { get; set; }

  public GSAAnalysisCase() { }

  [SchemaInfo("GSAAnalysisCase", "Creates a GSA analysis case", "GSA", "Analysis")]
  public GSAAnalysisCase([SchemaParamInfo("A list of load cases")] List<LoadCase> loadCases,
    [SchemaParamInfo("A list of load factors (to be mapped to provided load cases)")] List<double> loadFactors, string name = null, int? nativeId = null)
  {
    if (loadCases.Count != loadFactors.Count)
      throw new ArgumentException("Number of load cases provided does not match number of load factors provided");
    this.nativeId = nativeId;
    this.name = name;
    this.loadCases = loadCases;
    this.loadFactors = loadFactors;
  }
}
