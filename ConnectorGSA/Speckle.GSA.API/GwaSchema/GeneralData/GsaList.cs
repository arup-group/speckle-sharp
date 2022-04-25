using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.GSA.API.GwaSchema
{
  public class GsaList : GsaRecord
  {
    public string Name { get => name; set { name = value; } }
    public string Type;
    public List<int> Definition;
  }
}
