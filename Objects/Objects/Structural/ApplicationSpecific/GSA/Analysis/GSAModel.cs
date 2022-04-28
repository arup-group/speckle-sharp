using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Loading;
using Objects.Structural.Analysis;
using Objects.Structural.Results;
using System;

namespace Objects.Structural.GSA.Analysis
{
  public class GSAModel : Base
  {
    public int? nativeId { get; set; }
    public string name { get; set; }

    [DetachProperty]
    public Model designLayerModel { get; set; }

    [DetachProperty]
    public Model analysisLayerModel { get; set; }

    [DetachProperty]
    public ResultSetAll results { get; set; }

    public GSAModel() { }

    [SchemaInfo("GSAModel", "Creates a GSA model", "GSA", "Analysis")]
    public GSAModel(Model designLayerModel, Model analysisLayerModel, ResultSetAll results, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.designLayerModel = designLayerModel;
      this.analysisLayerModel = analysisLayerModel;
      this.results = results;
    }
  }
}
