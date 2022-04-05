namespace Speckle.GSA.API.CsvSchema
{
  public class CsvGlobal : CsvRecord
  {
    public virtual string CasePermutation { get; set; }

    public virtual float? Fx { get; set; }

    public virtual float? Fy { get; set; }

    public virtual float? Fz { get; set; }

    public float? F { get => Magnitude(Fx, Fy, Fz); }

    public virtual float? Mxx { get; set; }

    public virtual float? Myy { get; set; }

    public virtual float? Mzz { get; set; }

    public float? M { get => Magnitude(Mxx, Myy, Mzz); }

    public virtual float? Fx_Reac { get; set; }

    public virtual float? Fy_Reac { get; set; }

    public virtual float? Fz_Reac { get; set; }

    public float? F_Reac { get => Magnitude(Fx_Reac, Fy_Reac, Fz_Reac); }

    public virtual float? Mxx_Reac { get; set; }

    public virtual float? Myy_Reac { get; set; }

    public virtual float? Mzz_Reac { get; set; }

    public float? M_Reac { get => Magnitude(Mxx_Reac, Myy_Reac, Mzz_Reac); }

    public virtual int? Mode { get; set; }

    public virtual float? Frequency { get; set; }

    public virtual float? LoadFactor { get; set; }

    public virtual float? ModalStiffness { get; set; }

    public virtual float? ModalGeometricStiffness { get; set; }

    public virtual float? ModalMass { get; set; }

    public virtual float? EffectiveMassX { get; set; }

    public virtual float? EffectiveMassY { get; set; }

    public virtual float? EffectiveMassZ { get; set; }

    public virtual float? EffectiveMassXX { get; set; }

    public virtual float? EffectiveMassYY { get; set; }

    public virtual float? EffectiveMassZZ { get; set; }

  }
}
