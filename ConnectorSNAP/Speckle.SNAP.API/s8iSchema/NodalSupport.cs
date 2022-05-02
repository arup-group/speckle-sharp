namespace Speckle.SNAP.API.s8iSchema
{
  public class NodalSupport : VectorSixBase, ISnapRecordNamed
  {
    public NodalSupport(string name, params bool[] restraints) : base(name, restraints) { }
    public NodalSupport(params bool[] restraints) : base(restraints) { }
  }
}
