namespace Speckle.SNAP.API.s8iSchema
{
  public class Node : ISnapRecordNamed
  {
    public string Name { get; set; }
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Z { get; set; } = 0;
    public double AdditionalMass { get; set; } = 0;
    public double Mass { get; set; } = 0;
    public string Restraint { get; set; }
    public string AxisX { get; set; } = "";
    public string AxisY { get; set; } = "";
    public string AxisZ { get; set; } = "";
    public bool HistoryOutput { get; set; } = false;
    public bool Output { get; set; } = true;
  }
}
