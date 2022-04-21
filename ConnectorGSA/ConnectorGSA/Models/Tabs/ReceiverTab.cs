using Speckle.GSA.API;
using System.Collections.Generic;

namespace ConnectorGSA.Models
{
  public class ReceiverTab : TabBase
  {
    public double CoincidentNodeAllowance { get; set; } = 10;
    public GsaUnit CoincidentNodeUnits { get; set; } = GsaUnit.Millimetres;
    public string MappingStreamId { get; set; } = "";
    public List<StreamStateOld> ReceiverStreamStates { get => this.StreamStates; }

    public ReceiverTab() : base(GSALayer.Design)
    {

    }
  }
}
