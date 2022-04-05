using CsvHelper.Configuration.Attributes;
using Speckle.GSA.API.CsvSchema;

namespace Speckle.ConnectorGSA.Results
{
  public class CsvGlobalAnnotated : CsvGlobal
  {
    public override int ElemId { get; set; } = 0;

    [Index(0)]
    //[Name("case_id")]
    public override string CaseId { get; set; }

    [Ignore]
    [Name("case_permutation")]
    public override string CasePermutation { get; set; }

    [Index(3)]
    //[Name("load_x")]
    public override float? Fx { get; set; }

    [Index(4)]
    //[Name("load_y")]
    public override float? Fy { get; set; }

    [Index(5)]
    //[Name("load_z")]
    public override float? Fz { get; set; }

    [Index(6)]
    //[Name("load_xx")]
    public override float? Mxx { get; set; }

    [Index(7)]
    //[Name("load_yy")]
    public override float? Myy { get; set; }

    [Index(8)]
    //[Name("load_zz")]
    public override float? Mzz { get; set; }

    [Index(9)]
    //[Name("reaction_x")]
    public override float? Fx_Reac { get; set; }

    [Index(10)]
    //[Name("reaction_y")]
    public override float? Fy_Reac { get; set; }

    [Index(11)]
    //[Name("reaction_z")]
    public override float? Fz_Reac { get; set; }

    [Index(12)]
    //[Name("reaction_xx")]
    public override float? Mxx_Reac { get; set; }

    [Index(13)]
    //[Name("reaction_yy")]
    public override float? Myy_Reac { get; set; }

    [Index(14)]
    //[Name("reaction_zz")]
    public override float? Mzz_Reac { get; set; }

    [Index(15)]
    //[Name("mode")]
    public override int? Mode { get; set; }

    [Index(16)]
    //[Name("frequency")]
    public override float? Frequency { get; set; }

    [Index(17)]
    //[Name("load_factor")]
    public override float? LoadFactor { get; set; }

    [Index(18)]
    //[Name("modal_stiff")]
    public override float? ModalStiffness { get; set; }

    [Index(19)]
    //[Name("modal_geo_stiff")]
    public override float? ModalGeometricStiffness { get; set; }

    [Index(20)]
    //[Name("modal_mass")]
    public override float? ModalMass { get; set; }

    [Index(21)]
    //[Name("effective_mass_x")]
    public override float? EffectiveMassX { get; set; }

    [Index(22)]
    //[Name("effective_mass_y")]
    public override float? EffectiveMassY { get; set; }

    [Index(23)]
    //[Name("effective_mass_z")]
    public override float? EffectiveMassZ { get; set; }

    [Index(24)]
    //[Name("effective_mass_xx")]
    public override float? EffectiveMassXX { get; set; }

    [Index(25)]
    //[Name("effective_mass_yy")]
    public override float? EffectiveMassYY { get; set; }

    [Index(26)]
    //[Name("effective_mass_zz")]
    public override float? EffectiveMassZZ { get; set; }
  }
}
