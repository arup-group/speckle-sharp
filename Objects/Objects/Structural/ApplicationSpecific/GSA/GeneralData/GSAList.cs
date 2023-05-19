using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.Loading;

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

    [SchemaInfo("GSAList", "Creates a Speckle object for a GSA List (to be used for defining GSA lists and receiving into GSA only, not as input to other objects/components)", "GSA", "General Data")]
    public GSAList(
      string name, 
      GSAListType listType, 
      [SchemaParamInfo("NOTE: only objects matching specified list type are supported. Passing GSAList objects of this type as input is NOT currently supported.")]List<Base> definition, 
      int? nativeId = null)
    {
      if ((listType == GSAListType.Node && !definition.All(o => o is Node)) ||
        (listType == GSAListType.Member && !definition.All(o => o is GSAMember1D)) ||
        (listType == GSAListType.Element && (!definition.All(o => o is Element1D) || definition.Any(o => o is GSAMember1D))))
      {
        throw new ArgumentException($"GSA list contains objects that do not match type: {listType}", nameof(definition));
      }

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
    //Case,
    //Unspecified
  }
}
