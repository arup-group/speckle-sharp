namespace Speckle.SNAP.API.s8iSchema
{
  public class Beam : LinearBase, ISnapRecord
  {
    //Beams doesn't seem to have names in this format
    public NodeType NodeIType { get; set; } = NodeType.SecondaryNode;
    public NodeType NodeJType { get; set; } = NodeType.SecondaryNode;
    public string FinishingCoating { get; set; }
    public bool RigidJointIEdge { get; set; } = false;
    public bool RigidJointJEdge { get; set; } = false;
    public bool Cantilever { get; set; } = false;
    public string SectionProperties { get; set; }
  }
}
