using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API.s8iSchema
{
  public enum NodeType
  {
    Node = 1,
    SecondaryNode = 0
  }

  public enum StructureType
  {
    [StringValue("RC/SRC")]
    ReinforcedConcrete = 0,
    [StringValue("S")]
    Steel = 1,
    [StringValue("Wood")]
    Wood = 2,
    [StringValue("Other")]
    Other = 3
  }

  public enum SectionType
  {
    HSection = 0,
    Channel = 1,
    Box = 4,
    UserDefined = 9
  }

  public enum SectionRelevance
  {
    I = 0,
    Centre = 2,
    J = 3,
    EntireLength = 6
  }
}
