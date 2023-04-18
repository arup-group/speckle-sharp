using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Objects.Structural.ApplicationSpecific.GSA.GeneralData
{
  public class GSAList : Base
  {
    public int? nativeId { get; set; }
    public string name { get; set; }
    public GSAListType listType { get; set; }

    [DetachProperty]
    public List<Base> definition { get; set; }
    public List<string> definitionRefs { get; set; }

    public GSAList()
    {

    }

    [SchemaInfo("GSAList", "Creates a Speckle object for a GSA List", "GSA", "General Data")]
    public GSAList(string name, GSAListType listType, List<Base> definition, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.listType = listType;
      this.definition = definition;
      this.definitionRefs = definition.Select(i => i.applicationId).ToList();
    }
  }

  public enum GSAListType
  {
    Node,
    Element,
    Member,
    Case
  }
}
