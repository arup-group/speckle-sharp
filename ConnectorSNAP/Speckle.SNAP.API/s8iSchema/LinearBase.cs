namespace Speckle.SNAP.API.s8iSchema
{
  public abstract class LinearBase
  {
    public string NodeI { get; set; }
    public string NodeJ { get; set; }
    public string CrossSection { get; set; }
    public StructureType StructureType { get; set; } = StructureType.ReinforcedConcrete;
    public string Material { get; set; } = "SS400";
  }
}
