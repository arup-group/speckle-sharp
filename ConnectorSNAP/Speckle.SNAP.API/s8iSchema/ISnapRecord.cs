using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API.s8iSchema
{
  public interface ISnapRecord
  {
  }

  public interface ISnapRecordNamed : ISnapRecord
  { 
    string Name { get; }
  }
}
