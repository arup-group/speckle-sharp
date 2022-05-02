using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API.s8iSchema
{
  public class Girder : LinearBase, ISnapRecord
  {
    public string BoundaryConditionI { get; set; }
    public string BoundaryConditionJ { get; set; }
    //All other parameters are default values for now
  }
}
