using Speckle.GSA.API;
using System.Collections.Generic;
using DesktopUI2.Models;

namespace ConnectorGSA.Models
{
  public class ReceiverTab : TabBase
  {
    public double CoincidentNodeAllowance { get; set; } = 10;
    public GsaUnit CoincidentNodeUnits { get; set; } = GsaUnit.Millimetres;
    public string MappingStreamId { get; set; } = "";
    public List<StreamState> ReceiverStreamStates { get => this.StreamStates; }

    public ReceiverTab() : base(GSALayer.Design)
    {

    }
  }
}
