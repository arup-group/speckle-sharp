﻿using Objects.Structural.Analysis;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Materials;
using Objects.Structural.GSA.Properties;
using Objects.Structural.Materials;
using Objects.Structural.Properties;
using Objects.Structural.Properties.Profiles;
using Speckle.Core.Models;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSNAP
{
  public partial class ConverterSNAP
  {
    private const double geoTolerance = 0.01;
    private Dictionary<Objects.Structural.ShapeType, Action<Property1D, Section>> profileFns = new Dictionary<Objects.Structural.ShapeType, Action<Property1D, Section>>();
    //This is like its own mini-cache - similar to the provisionals that was part of the cache for the GSA connector
    private Dictionary<byte, string> endReleasesAppIdByBools = new Dictionary<byte, string>();
    //private List<string> materialSteelNames = new List<string>();
    //private Dictionary<string, string> materialsBySection = new Dictionary<string, string>();
    //private Dictionary<string, string> sectionsByPropertyAppId = new Dictionary<string, string>();

    private ModelUnits modelUnits;
    private int decimalRoundingPlaces = 6;

    private void SetupToNativeFns()
    {
      endReleasesAppIdByBools.Clear();
      //materialSteelNames.Clear();
      //materialsBySection.Clear();
      //sectionsByPropertyAppId.Clear();

      profileFns = new Dictionary<Objects.Structural.ShapeType, Action<Property1D, Section>>()
      {
        { Objects.Structural.ShapeType.I, Property1DToNativeProfileI },
        { Objects.Structural.ShapeType.Channel, Property1DToNativeProfileChannel },
        { Objects.Structural.ShapeType.Rectangular, Property1DToNativeProfileRectangular },
        { Objects.Structural.ShapeType.Circular, Property1DToNativeProfileCircular }
      };

      ToNativeFns = new Dictionary<Type, Func<Base, List<object>>>()
      {
        { typeof(Model), ModelToNative },
        { typeof(Element1D), Element1DToNative },
        { typeof(Objects.Structural.Geometry.Node), NodeToNative },
        { typeof(Restraint), RestraintToNative },
        { typeof(Property1D), Property1DToNative },
        { typeof(Steel), SteelToNative }
      };

      var structuralAssembly = Assembly.GetAssembly(ToNativeFns.Keys.First());
      var toNativeFnsToAdd = new Dictionary<Type, Func<Base, List<object>>>();
      var toNativeFnTypes = ToNativeFns.Keys.ToList();
      foreach (var t in toNativeFnTypes)
      {
        var subclasses = structuralAssembly.GetTypes().Where(st => st.BaseType == t);
        if (subclasses != null && subclasses.Count() > 0)
        {
          foreach (var sc in subclasses)
          {
            ToNativeFns.Add(sc, ToNativeFns[t]);
          }
        }
      }
    }

    //This is the only method with a LIST of objects as an input - and intentionally not part of NativeFns list
    private List<object> SpeckleObjectsToNative(List<Base> speckleObjects)
    {
      //var retList = new List<object>();
      var nativeObjects = new List<object>();

      var speckleDependencyTree = Helper.SpeckleDependencyTree();

      var objectsByType = speckleObjects.GroupBy(o => o.GetType()).ToDictionary(g => g.Key, g => g.ToList());

      foreach (var gen in speckleDependencyTree)
      {
        var genNativeObjs = new List<object>();
        foreach (var t in gen)
        {
          if (objectsByType.ContainsKey(t))
          {
            foreach (Base so in objectsByType[t])
            {
              try
              {
                if (CanConvertToNative(so))
                {
                  var natives = ToNativeFns[t](so);
                  //retList.AddRange(natives);
                  genNativeObjs.AddRange(natives);
                }
              }
              catch
              {
                ConversionErrors.Add(new Exception("Unable to convert " + t.Name + " " + (so.applicationId ?? so.id)
                  + " - refer to logs for more information"));
              }
            }
          }
        }
        if (genNativeObjs != null && genNativeObjs.Count > 0)
        {
          //Add this generation's objects
          Instance.SnapModel.Cache.Upsert(genNativeObjs);
          nativeObjects.AddRange(genNativeObjs);
        }
      }
      //return retList;
      return nativeObjects;
    }

    private List<object> ModelToNative(Base speckleObject)
    {
      var model = (Model)speckleObject;
      if (model.specs != null && model.specs.settings != null && model.specs.settings.modelUnits != null)
      {
        modelUnits = model.specs.settings.modelUnits;
      }
      var speckleObjects = new List<Base>();
      speckleObjects.AddRangeIfNotNull(model.nodes);
      speckleObjects.AddRangeIfNotNull(model.elements);
      speckleObjects.AddRangeIfNotNull(model.loads);
      speckleObjects.AddRangeIfNotNull(model.restraints);
      speckleObjects.AddRangeIfNotNull(model.properties);
      speckleObjects.AddRangeIfNotNull(model.materials);

      return SpeckleObjectsToNative(speckleObjects);
    }

    private List<object> SteelToNative(Base speckleObject)
    {
      var retList = new List<object>();
      if (speckleObject == null || !speckleObject.GetType().InheritsOrImplements(typeof(Steel)))
      {
        return retList;
      }
      var speckleSteel = (Steel)speckleObject;

      //Currently hard-wired to produce this one steel type

      var snapSteel = new MaterialSteel() { Name = speckleSteel.name };
      //materialSteelNames.Add(snapSteel.Name);
      retList.Add(snapSteel);

      return retList;
    }

    private string ToS8iName(string v)
    {
      var rv = v.Replace("/", "").Replace("-", "");
      return rv.Substring(0, Math.Min(rv.Length, 15));
    }

    private List<object> NodeToNative(Base speckleObject)
    {
      var retList = new List<object>();
      if (speckleObject == null || !speckleObject.GetType().InheritsOrImplements(typeof(Objects.Structural.Geometry.Node)))
      {
        return retList;
      }
      var speckleNode = (Objects.Structural.Geometry.Node)speckleObject;

      double scalingFactor = 1;
      if (modelUnits != null)
      {
        if (modelUnits.length == "m") scalingFactor = 1000;
        else if (modelUnits.length == "cm") scalingFactor = 100;
        else if (modelUnits.length == "in") scalingFactor = 25.4;
        else if (modelUnits.length == "ft") scalingFactor = 304.8;
      }

      if (MapsToSecondaryNode(speckleNode))
      {
        var snapSecondaryNode = new SecondaryNode()
        {
          Name = ToS8iName(speckleNode.applicationId),
          X = Math.Round(speckleNode.basePoint.x * scalingFactor, decimalRoundingPlaces),
          Y = Math.Round(speckleNode.basePoint.y * scalingFactor, decimalRoundingPlaces),
          Z = Math.Round(speckleNode.basePoint.z * scalingFactor, decimalRoundingPlaces)
        };

        retList.Add(snapSecondaryNode);
      }
      else
      {
        var snapNode = new Speckle.SNAP.API.s8iSchema.Node()
        {
          Name = ToS8iName(speckleNode.applicationId),
          X = Math.Round(speckleNode.basePoint.x * scalingFactor, decimalRoundingPlaces),
          Y = Math.Round(speckleNode.basePoint.y * scalingFactor, decimalRoundingPlaces), 
          Z = Math.Round(speckleNode.basePoint.z * scalingFactor, decimalRoundingPlaces)
        };

        if (speckleNode.massProperty != null)
        {
          snapNode.AdditionalMass = speckleNode.massProperty.mass;
        }

        retList.Add(snapNode);
      }
      return retList;
    }

    private List<object> Element1DToNative(Base speckleObject)
    {
      var retList = new List<object>();
      if (speckleObject == null || !speckleObject.GetType().InheritsOrImplements(typeof(Element1D)))
      {
        return retList;
      }
      var speckleElement = (Element1D)speckleObject;

      //Review: should this be in there?

      bool isVertical = false;
      if (speckleElement.end1Node != null && speckleElement.end2Node != null)
      {
        isVertical = ((speckleElement.end1Node.basePoint.x.EqualsWithinTolerance(speckleElement.end2Node.basePoint.x, geoTolerance))
          && (speckleElement.end1Node.basePoint.y.EqualsWithinTolerance(speckleElement.end2Node.basePoint.y, geoTolerance)));
      }

      if (isVertical)
      {
        //No matter what the type is, it should be treated as a column
      }
      else if (speckleElement.type == ElementType1D.Beam)
      {
        var snapGirder = new Girder();
        if (speckleElement.end1Node != null)
        {
          snapGirder.NodeI = ToS8iName(speckleElement.end1Node.applicationId);
        }
        if (speckleElement.end2Node != null)
        {
          snapGirder.NodeJ = ToS8iName(speckleElement.end2Node.applicationId);
        }
        if (speckleElement.end1Releases != null)
        {
          var endRel1Objs = RestraintToNative(speckleElement.end1Releases);
          if ((endRel1Objs != null) && (endRel1Objs.Count > 0) && (endRel1Objs.First() != null) && (endRel1Objs.First() is EndReleases))
          {
            var endRel1 = endRel1Objs.Cast<EndReleases>().FirstOrDefault();
            var boolVals = ConvertBoolArrayToByte(endRel1.Restraints);
            if (!endReleasesAppIdByBools.ContainsKey(boolVals))
            {
              endRel1.Name = ToS8iName(Guid.NewGuid().ToString());
              endReleasesAppIdByBools.Add(boolVals, endRel1.Name);
              retList.AddRange(endRel1Objs);
            }
            snapGirder.BoundaryConditionI = endReleasesAppIdByBools[boolVals];
          }
        }
        if (speckleElement.end2Releases != null)
        {
          var endRel2Objs = RestraintToNative(speckleElement.end2Releases);
          if ((endRel2Objs != null) && (endRel2Objs.Count > 0) && (endRel2Objs.First() != null) && (endRel2Objs.First() is EndReleases))
          {
            var endRel2 = endRel2Objs.Cast<EndReleases>().FirstOrDefault();
            var boolVals = ConvertBoolArrayToByte(endRel2.Restraints);
            if (!endReleasesAppIdByBools.ContainsKey(boolVals))
            {
              endRel2.Name = ToS8iName(Guid.NewGuid().ToString());
              endReleasesAppIdByBools.Add(boolVals, endRel2.Name);
              retList.AddRange(endRel2Objs);
            }
            snapGirder.BoundaryConditionJ = endReleasesAppIdByBools[boolVals];
          }
        }
        if (speckleElement.property != null)
        {
          object sectionObject;
          if ((!string.IsNullOrEmpty(speckleElement.property.name) 
            && Instance.SnapModel.Cache.Contains<Section>(speckleElement.property.name, out sectionObject))
            ||
              (!string.IsNullOrEmpty(speckleElement.property.applicationId)
            && Instance.SnapModel.Cache.Contains<Section>(speckleElement.property.applicationId, out sectionObject)))
          {
            var section = (Section)sectionObject;
            snapGirder.CrossSection = section.Name;
            snapGirder.Material = section.Material;
          }
        }

        retList.Add(snapGirder);
      }
      else if (speckleElement.type == ElementType1D.Null) //At the moment, this block won't actually get invoked but left here for future use if the logic check is upgraded
      {
        //Assume a SNAP beam in this case
        var snapBeam = new Beam();

        if (speckleElement.end1Node != null)
        {
          snapBeam.NodeI = speckleElement.end1Node.applicationId;
          snapBeam.NodeIType = MapsToSecondaryNode(speckleElement.end1Node) ? NodeType.SecondaryNode : NodeType.Node;
        }
        if (speckleElement.end2Node != null)
        {
          snapBeam.NodeJ = speckleElement.end2Node.applicationId;
          snapBeam.NodeJType = MapsToSecondaryNode(speckleElement.end2Node) ? NodeType.SecondaryNode : NodeType.Node;
        }
        if (speckleElement.property != null)
        {
          snapBeam.CrossSection = speckleElement.property.applicationId;
          if (speckleElement.property.material != null)
          {
            snapBeam.Material = speckleElement.property.material.applicationId;
            var materialType = speckleElement.property.material.GetType();
            snapBeam.StructureType = materialType.InheritsOrImplements(typeof(Steel))
              ? StructureType.Steel
              : materialType.InheritsOrImplements(typeof(Concrete))
                ? StructureType.ReinforcedConcrete
                : StructureType.Other;
          }
        }

        retList.Add(snapBeam);
      }

      return retList;
    }

    private List<object> Property1DToNative(Base speckleObject)
    {
      var retList = new List<object>();
      if (speckleObject == null || !speckleObject.GetType().InheritsOrImplements(typeof(Property1D)))
      {
        return retList;
      }
      var speckleProperty = (Property1D)speckleObject;

      if (speckleProperty.profile == null || !profileFns.ContainsKey(speckleProperty.profile.shapeType))
      {
        return retList;
      }

      var snapSection = new Section() { Name = ToS8iName(speckleProperty.name) };

      profileFns[speckleProperty.profile.shapeType](speckleProperty, snapSection);

      //This is to handle both application IDs and names in case the ToNative changes which one it uses
      var materialAppId = "";
      var materialName = "";
      Type materialType = default(Type);
      if (speckleProperty.material != null)
      {
        materialAppId = speckleProperty.material.applicationId;
        materialName = speckleProperty.material.name;
        materialType = speckleProperty.material.GetType();
      }
      if (speckleObject is GSAProperty1D && ((GSAProperty1D)speckleProperty).designMaterial != null)
      {
        materialAppId = ((GSAProperty1D)speckleProperty).designMaterial.applicationId;
        materialName = ((GSAProperty1D)speckleProperty).designMaterial.name;
        materialType = ((GSAProperty1D)speckleProperty).designMaterial.GetType();
      }

      if (materialType != null && materialType.InheritsOrImplements(typeof(Steel)))
      {
        //if (materialSteelNames.Contains(materialName))
        if (Instance.SnapModel.Cache.Contains<MaterialSteel>(materialName, out _))
        {
          snapSection.Material = materialName;
        }
        //else if (materialSteelNames.Contains(materialAppId))
        if (Instance.SnapModel.Cache.Contains<MaterialSteel>(materialAppId, out _))
        {
          snapSection.Material = materialAppId;
        }

        //materialsBySection.Add(snapSection.Name, snapSection.Material);
      }

      retList.Add(snapSection);

      return retList;
    }

    private void Property1DToNativeProfileI(Property1D speckleProperty, Section section)
    {
      section.SectionType = SectionType.HSection;
      var speckleSection = (ISection)speckleProperty.profile;
      section.StandardDimensions = new double[4] { speckleSection.depth, speckleSection.width, speckleSection.flangeThickness, speckleSection.webThickness  };
    }

    private void Property1DToNativeProfileChannel(Property1D speckleProperty, Section section)
    {
      section.SectionType = SectionType.Channel;
      var speckleSection = (Channel)speckleProperty.profile;
      section.StandardDimensions = new double[4] { speckleSection.depth, speckleSection.width, speckleSection.flangeThickness, speckleSection.webThickness };
    }

    private void Property1DToNativeProfileRectangular(Property1D speckleProperty, Section section)
    {
      section.SectionType = SectionType.Box;
      var speckleSection = (Rectangular)speckleProperty.profile;
      section.StandardDimensions = new double[4] { speckleSection.depth, speckleSection.width, speckleSection.flangeThickness, speckleSection.webThickness };
    }

    private void Property1DToNativeProfileCircular(Property1D speckleProperty, Section section)
    {
      section.SectionType = SectionType.UserDefined;
      var speckleSection = (Circular)speckleProperty.profile;
      section.CustomCatalogueFields = new string[2] { "9003", "1" };
      section.CatalogueItemName = string.Join("_", "STD", "CHS", speckleSection.radius, speckleSection.wallThickness);
    }

    private List<object> RestraintToNative(Base speckleObject)
    {
      var speckleRestraint = (Restraint)speckleObject;

      var snapEndReleases = new EndReleases()
      {
        Name = ToS8iName(speckleRestraint.applicationId),
        Restraints = new bool[6]
        {
          speckleRestraint.stiffnessX.ToBoolean(),
          speckleRestraint.stiffnessY.ToBoolean(),
          speckleRestraint.stiffnessZ.ToBoolean(),
          speckleRestraint.stiffnessXX.ToBoolean(),
          speckleRestraint.stiffnessYY.ToBoolean(),
          speckleRestraint.stiffnessZZ.ToBoolean()
        }
      };

      return new List<object> { snapEndReleases };
    }

    private bool MapsToSecondaryNode(Objects.Structural.Geometry.Node n)
    {
      return (n.restraint == null && n.massProperty == null);
    }

    //https://stackoverflow.com/questions/24322417/how-to-convert-bool-array-in-one-byte-and-later-convert-back-in-bool-array
    private static byte ConvertBoolArrayToByte(bool[] source)
    {
      byte result = 0;
      // This assumes the array never contains more than 8 elements!
      int index = 8 - source.Length;

      // Loop through the array
      foreach (bool b in source)
      {
        // if the element is 'true' set the bit at that position
        if (b)
          result |= (byte)(1 << (7 - index));

        index++;
      }

      return result;
    }
    private static bool[] ConvertByteToBoolArray(byte b, int numValues = 8)
    {
      // prepare the return result
      bool[] result = new bool[numValues];

      // check each bit in the byte. if 1 set to true, if 0 set to false
      for (int i = 0; i < numValues; i++)
      {
        result[i] = (b & (1 << i)) == 0 ? false : true;
      }
      // reverse the array
      Array.Reverse(result);

      return result;
    }
  }
}
