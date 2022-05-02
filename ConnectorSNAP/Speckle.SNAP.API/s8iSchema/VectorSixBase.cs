namespace Speckle.SNAP.API.s8iSchema
{
  public abstract class VectorSixBase
  {
    public string Name { get; set; }
    public bool[] Restraints { get; set; } = new bool[6] { false, false, false, false, false, false };

    public VectorSixBase(string name, params bool[] restraints)
    {
      Name = name;
      for (int i = 0; i < restraints.Length; i++)
      {
        Restraints[i] = restraints[i];
      }
    }

    public VectorSixBase(params bool[] restraints)
    {
      for (int i = 0; i < restraints.Length; i++)
      {
        Restraints[i] = restraints[i];
      }
    }
  }
}
