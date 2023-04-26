using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.GSA.Materials;
using Objects.Structural.GSA.Properties;
using Objects.Structural.Properties;
using Speckle.Core.Models;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Structural.GSA.Loading;
using Objects.Structural.GSA.Analysis;
using Objects.Structural.GSA.Bridge;
using GwaAxisDirection6 = Speckle.GSA.API.GwaSchema.AxisDirection6;
using AxisDirection6 = Objects.Structural.GSA.Geometry.AxisDirection6;
using Restraint = Objects.Structural.Geometry.Restraint;
using Speckle.Core.Kits;
using Objects.Structural.Loading;
using Objects.Structural;
using Objects.Structural.Materials;
using Objects;
using Objects.Structural.Properties.Profiles;
using Objects.Structural.Analysis;
using Objects.Structural.ApplicationSpecific.GSA.GeneralData;
using Speckle.GSA.API.GwaSchema.Loading.Beam;

namespace ConverterGSA
{
  //Container just for ToNative methods, and their helper methods
  public partial class ConverterGSA
  {
    private Dictionary<Type, Func<Base, List<GsaRecord>>> ToNativeFns;

    private static Point Origin = new Point(0, 0, 0);
    private static Vector UnitX = new Vector(1, 0, 0);
    private static Vector UnitY = new Vector(0, 1, 0);
    private static Vector UnitZ = new Vector(0, 0, 1);
    private static int GeometricDecimalPlaces = 6;

    void SetupToNativeFns()
    {
      ToNativeFns = new Dictionary<Type, Func<Base, List<GsaRecord>>>()
      {
        // typeof(Model), ModelToNative },
        { typeof(ModelInfo), ModelInfoToNative },
        { typeof(ModelUnits), ModelUnitsToNative },
        //Geometry
        { typeof(Axis), AxisToNative },
        { typeof(Point), PointToNative },
        { typeof(GSANode), GSANodeToNative },
        { typeof(Node), NodeToNative },
        { typeof(GSAElement1D), GSAElement1dToNative },
        { typeof(Element1D), Element1dToNative },
        { typeof(GSAElement2D), GSAElement2dToNative },
        { typeof(Element2D), Element2dToNative },
        { typeof(GSAMember1D), GSAMember1dToNative },
        { typeof(GSAMember2D), GSAMember2dToNative },
        { typeof(GSAAssembly), GSAAssemblyToNative },
        { typeof(GSAGridLine), GSAGridLineToNative },
        { typeof(Storey), StoreyToNative },
        { typeof(GSAGridPlane), GSAGridPlaneToNative },
        { typeof(GSAGridSurface), GSAGridSurfaceToNative },
        { typeof(GSAPolyline), GSAPolylineToNative },
        //Loading
        { typeof(GSALoadCase), GSALoadCaseToNative },
        { typeof(LoadCase), LoadCaseToNative },
        { typeof(GSAAnalysisTask), GSAAnalysisTaskToNative },
        //{ typeof(GSAAnalysisCase), GSAAnalysisCaseToNative },
        { typeof(GSACombinationCase), GSACombinationCaseToNative },
        { typeof(LoadCombination), LoadCombinationToNative },
        { typeof(GSALoadBeam), GSALoadBeamToNative },
        { typeof(LoadBeam), LoadBeamToNative },
        { typeof(GSALoadFace), GSALoadFaceToNative },
        { typeof(LoadFace), LoadFaceToNative },
        { typeof(GSALoadNode), GSALoadNodeToNative },
        { typeof(LoadNode), LoadNodeToNative },
        { typeof(GSALoadGravity), GSALoadGravityToNative },
        { typeof(LoadGravity), LoadGravityToNative },
        { typeof(GSALoadThermal1d), GSALoadThermal1dToNative },
        { typeof(GSALoadThermal2d), GSALoadThermal2dToNative },
        { typeof(GSALoadGridPoint), GSALoadGridPointToNative },
        { typeof(GSALoadGridLine), GSALoadGridLineToNative },
        { typeof(GSALoadGridArea), GSALoadGridAreaToNative },
        //Materials
        { typeof(GSASteel), GSASteelToNative },
        { typeof(Steel), SteelToNative },
        { typeof(GSAConcrete), GSAConcreteToNative },
        { typeof(Concrete), ConcreteToNative },
        { typeof(GSAMaterial), GSAAnalysisMaterialToNative },
        //Properties
        { typeof(Property1D), Property1dToNative },
        { typeof(GSAProperty1D), GsaProperty1dToNative },
        { typeof(Property2D), Property2dToNative },
        { typeof(GSAProperty2D), GsaProperty2dToNative },
        { typeof(PropertySpring), PropertySpringToNative },
        { typeof(PropertyMass), PropertyMassToNative },
        { typeof(GSAPropertyLink), GSAPropertyLinkToNative },
        //Constraints
        { typeof(GSARigidConstraint), GSARigidConstraintToNative },
        { typeof(GSAGeneralisedRestraint), GSAGeneralisedRestraintToNative },
        // Bridge
        { typeof(GSAInfluenceNode), InfNodeToNative},
        { typeof(GSAInfluenceBeam), InfBeamToNative},
        { typeof(GSAAlignment), AlignToNative },
        { typeof(GSAPath), PathToNative },
        { typeof(GSAUserVehicle), GSAUserVehicleToNative },
        // Analysis
        { typeof(GSAStage), AnalStageToNative },
        { typeof(GSAStageProp), AnalStagePropToNative },
        // General Data
        { typeof(GSAList), GSAListToNative }
      };
    }

    #region ToNative
    //TO DO: implement conversion code for ToNative

    private List<Base> FlattenModel(Model model)
    {
      var speckleObjects = new List<Base>();
      speckleObjects.AddRangeIfNotNull(model.nodes);
      speckleObjects.AddRangeIfNotNull(model.elements);
      speckleObjects.AddRangeIfNotNull(model.loads);
      speckleObjects.AddRangeIfNotNull(model.restraints);
      speckleObjects.AddRangeIfNotNull(model.properties);
      speckleObjects.AddRangeIfNotNull(model.materials);
      return speckleObjects;
    }

    private List<GsaRecord> ModelInfoToNative(Base speckleObject)
    {
      var modelInfo = (ModelInfo)speckleObject;
      return (modelInfo.settings != null && modelInfo.settings.modelUnits != null)
        ? ModelUnitsToNative(modelInfo.settings.modelUnits) : null;
    }

    private List<GsaRecord> ModelUnitsToNative(Base speckleObject)
    {
      var modelUnits = (ModelUnits)speckleObject;
      if (modelUnits != null)
      {
        conversionFactors = new UnitConversion(modelUnits);
      }
      return null;
    }

    #region Geometry
    private List<GsaRecord> AxisToNative(Base speckleObject)
    {
      var speckleAxis = (Axis)speckleObject;
      var gsaAxis = new GsaAxis()
      {
        ApplicationId = speckleAxis.applicationId,
        Index = speckleAxis.GetIndex<GsaAxis>(),
        Name = speckleAxis.name,
      };
      if (speckleAxis.definition != null)
      {
        var scaleFactor = speckleAxis.definition.origin.GetScaleFactor(conversionFactors);

        if (speckleAxis.definition.origin != null)
        {
          gsaAxis.OriginX = speckleAxis.definition.origin.x * scaleFactor;
          gsaAxis.OriginY = speckleAxis.definition.origin.y * scaleFactor;
          gsaAxis.OriginZ = speckleAxis.definition.origin.z * scaleFactor;
        }
        if (speckleAxis.definition.xdir != null && speckleAxis.definition.xdir.Norm() != 0)
        {
          gsaAxis.XDirX = speckleAxis.definition.xdir.x * scaleFactor;
          gsaAxis.XDirY = speckleAxis.definition.xdir.y * scaleFactor;
          gsaAxis.XDirZ = speckleAxis.definition.xdir.z * scaleFactor;
        }
        if (speckleAxis.definition.ydir != null && speckleAxis.definition.ydir.Norm() != 0)
        {
          gsaAxis.XYDirX = speckleAxis.definition.ydir.x * scaleFactor;
          gsaAxis.XYDirY = speckleAxis.definition.ydir.y * scaleFactor;
          gsaAxis.XYDirZ = speckleAxis.definition.ydir.z * scaleFactor;
        }
      }

      return new List<GsaRecord> { gsaAxis };
    }

    private List<GsaRecord> PointToNative(Base speckleObject)
    {
      var basePoint = (Point)speckleObject;
      var retList = new List<GsaRecord>();

      var speckleNode = new Node(basePoint, null, null, null);

      var gsaNode = new GsaNode()
      {
        ApplicationId = speckleObject.id, // should be identical if xyz coordinates the same
        Index = basePoint.NodeAt(conversionFactors),
        Name = speckleNode.name,
        SpringPropertyIndex = IndexByConversionOrLookup<GsaPropSpr>(speckleNode.springProperty, ref retList),
        MassPropertyIndex = IndexByConversionOrLookup<GsaPropMass>(speckleNode.massProperty, ref retList),
      };

      if (GetRestraint(speckleNode.restraint, out var gsaNodeRestraint, out var gsaRestraint))
      {
        gsaNode.NodeRestraint = gsaNodeRestraint;
        gsaNode.Restraints = gsaRestraint;
      }
      if (GetAxis(speckleNode.constraintAxis, out NodeAxisRefType gsaAxisRefType, out var gsaAxisIndex, ref retList))
      {
        gsaNode.AxisRefType = gsaAxisRefType;
        gsaNode.AxisIndex = gsaAxisIndex;
      }

      //Unit conversions
      if (basePoint != null)
      {
        var factor = basePoint.GetScaleFactor(conversionFactors);
        gsaNode.X = basePoint.x * factor;
        gsaNode.Y = basePoint.y * factor;
        gsaNode.Z = basePoint.z * factor;
      }

      retList.Add(gsaNode);
      return retList;
    }

    private List<GsaRecord> GSANodeToNative(Base speckleObject)
    {
      var gsaNode = (GsaNode)NodeToNative(speckleObject).First(o => o is GsaNode);
      var speckleNode = (GSANode)speckleObject;
      gsaNode.Colour = speckleNode.colour?.ColourToNative() ?? Colour.NotSet;
      gsaNode.MeshSize = speckleNode.localElementSize.IsPositiveOrNull();
      return new List<GsaRecord>() { gsaNode };
    }

    private List<GsaRecord> NodeToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleNode = (Node)speckleObject;
      var gsaNode = new GsaNode()
      {
        ApplicationId = speckleNode.applicationId,
        Index = speckleNode.NodeAt(conversionFactors),
        Name = speckleNode.name,
        SpringPropertyIndex = IndexByConversionOrLookup<GsaPropSpr>(speckleNode.springProperty, ref retList),
        MassPropertyIndex = IndexByConversionOrLookup<GsaPropMass>(speckleNode.massProperty, ref retList),
      };
      if (GetRestraint(speckleNode.restraint, out var gsaNodeRestraint, out var gsaRestraint))
      {
        gsaNode.NodeRestraint = gsaNodeRestraint;
        gsaNode.Restraints = gsaRestraint;
      }
      if (GetAxis(speckleNode.constraintAxis, out NodeAxisRefType gsaAxisRefType, out var gsaAxisIndex, ref retList))
      {
        gsaNode.AxisRefType = gsaAxisRefType;
        gsaNode.AxisIndex = gsaAxisIndex;
      }

      //Unit conversions
      if (speckleNode.basePoint != null)
      {
        var factor = speckleNode.GetScaleFactor(conversionFactors);
        gsaNode.X = speckleNode.basePoint.x * factor;
        gsaNode.Y = speckleNode.basePoint.y * factor;
        gsaNode.Z = speckleNode.basePoint.z * factor;
      }

      retList.Add(gsaNode);
      return retList;
    }

    private List<GsaRecord> GSAElement1dToNative(Base speckleObject)
    {
      var gsaRecords = Element1dToNative(speckleObject);
      var gsaElement = (GsaEl)gsaRecords.First(o => o is GsaEl);
      var speckleElement = (GSAElement1D)speckleObject;
      gsaElement.Type = speckleElement.type.ToNative();
      gsaElement.Colour = speckleElement.colour?.ColourToNative() ?? Colour.NotSet;
      gsaElement.Dummy = speckleElement.isDummy;
      gsaElement.Group = speckleElement.group.IsPositiveOrNull();
      return gsaRecords;

      //TODO:
      //SpeckleObject:
      //  string action
    }

    private List<GsaRecord> Element1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleElement = (Element1D)speckleObject;

      if (speckleElement.memberType != Objects.Structural.Geometry.MemberType.NotSet)
      {
        var speckleMember = new GSAMember1D()
        {
          applicationId = speckleElement.applicationId,
          id = speckleElement.id,
          name = speckleElement.name,
          baseLine = speckleElement.baseLine,
          property = speckleElement.property,
          memberType = speckleElement.memberType,
          end1Releases = speckleElement.end1Releases,
          end2Releases = speckleElement.end2Releases,
          end1Offset = speckleElement.end1Offset,
          end2Offset = speckleElement.end2Offset,
          orientationNode = speckleElement.orientationNode,
          orientationAngle = speckleElement.orientationAngle,
          localAxis = speckleElement.localAxis,
          end1Node = speckleElement.end1Node,
          end2Node = speckleElement.end2Node,
          topology = speckleElement.topology,
          topologyRefs = speckleElement.topologyRefs
        };
        return GSAMember1dToNative(speckleMember);
      }

      var gsaElement = new GsaEl()
      {
        ApplicationId = speckleElement.applicationId,
        Index = speckleElement.GetIndex<GsaEl>(),
        Name = speckleElement.name,
        Type = speckleElement.type.ToNative(),
        OrientationNodeIndex = speckleElement.orientationNode.NodeAt(conversionFactors),
        //TaperOffsetPercentageEnd1 - currently not supported
        //TaperOffsetPercentageEnd2 - currently not supported
        PropertyIndex = IndexByConversionOrLookup<GsaSection>(speckleElement.property, ref retList),
        ParentIndex = IndexByConversionOrLookup<GsaMemb>(speckleElement.parent, ref retList)
      };

      if (speckleElement.topology != null && speckleElement.topology.Count > 0)
      {
        gsaElement.NodeIndices = speckleElement.topology.NodeAt(conversionFactors);
      }
      else if (speckleElement.topologyRefs != null && speckleElement.topologyRefs.Count > 0)
      {
        var nodes = speckleElement.topologyRefs.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();
        gsaElement.NodeIndices = nodes.NodeAt(conversionFactors);
      }
      else if (speckleElement.baseLine != null)
      {
        var specklePoints = Get1dTopolopgy(speckleElement.baseLine);
        if (specklePoints != null)
          gsaElement.NodeIndices = specklePoints.NodeAt(conversionFactors);
      }

      if (speckleElement.end1Releases != null && GetReleases(speckleElement.end1Releases, out var gsaRelease1, out var gsaStiffnesses1, out var gsaReleaseInclusion1))
      {
        gsaElement.Releases1 = gsaRelease1;
        gsaElement.Stiffnesses1 = gsaStiffnesses1;
        gsaElement.ReleaseInclusion = gsaReleaseInclusion1;
      }
      if (speckleElement.end2Releases != null && GetReleases(speckleElement.end2Releases, out var gsaRelease2, out var gsaStiffnesses2, out var gsaReleaseInclusion2))
      {
        gsaElement.Releases2 = gsaRelease2;
        gsaElement.Stiffnesses2 = gsaStiffnesses2;
        gsaElement.ReleaseInclusion = gsaReleaseInclusion2;
      }

      // If offset vector units are not set, fallback to general conversion factors
      var end1OffsetScaleFactor = speckleElement.end1Offset != null ? (string.IsNullOrEmpty(speckleElement.end1Offset.units) ? conversionFactors.length : conversionFactors.ConversionFactorToNative(UnitDimension.Length, speckleElement.end1Offset.units)) : conversionFactors.length;
      var end2OffsetScaleFactor = speckleElement.end2Offset != null ? (string.IsNullOrEmpty(speckleElement.end2Offset.units) ? conversionFactors.length : conversionFactors.ConversionFactorToNative(UnitDimension.Length, speckleElement.end2Offset.units)) : conversionFactors.length;

      if (speckleElement.end1Offset != null && speckleElement.end1Offset.x != 0)
      {
        gsaElement.End1OffsetX = end1OffsetScaleFactor * speckleElement.end1Offset.x;
      }
      if (speckleElement.end2Offset != null && speckleElement.end2Offset.x != 0)
      {
        gsaElement.End2OffsetX = end2OffsetScaleFactor * speckleElement.end2Offset.x;
      }
      if (speckleElement.end1Offset != null && speckleElement.end2Offset != null)
      {
        if (speckleElement.end1Offset.y == speckleElement.end2Offset.y)
        {
          if (speckleElement.end1Offset.y != 0) gsaElement.OffsetY = end1OffsetScaleFactor * speckleElement.end1Offset.y;
        }
        else
        {
          gsaElement.OffsetY = end1OffsetScaleFactor * speckleElement.end1Offset.y;
          Report.ConversionErrors.Add(new Exception("Element1dToNative: "
            + "Error converting element1d with application id (" + speckleElement.applicationId + "). "
            + "Different y offsets were assigned at either end."
            + "end 1 y offset of " + gsaElement.OffsetY.ToString() + " has been applied"));
        }
        if (speckleElement.end1Offset.z == speckleElement.end2Offset.z)
        {
          if (speckleElement.end1Offset.z != 0) gsaElement.OffsetZ = end1OffsetScaleFactor * speckleElement.end1Offset.z;
        }
        else
        {
          gsaElement.OffsetZ = end1OffsetScaleFactor * speckleElement.end1Offset.z;
          Report.ConversionErrors.Add(new Exception("Element1dToNative: "
            + "Error converting element1d with application id (" + speckleElement.applicationId + "). "
            + "Different z offsets were assigned at either end."
            + "end 1 z offset of " + gsaElement.OffsetY.ToString() + " has been applied"));
        }
      }
      if (speckleElement.orientationAngle != 0) gsaElement.Angle = conversionFactors.ConversionFactorToDegrees() * speckleElement.orientationAngle;

      retList.Add(gsaElement);
      return retList;
    }

    private List<GsaRecord> GSAElement2dToNative(Base speckleObject)
    {
      var gsaRecords = Element2dToNative(speckleObject);
      var gsaElement = (GsaEl)gsaRecords.First(o => o is GsaEl);
      var speckleElement = (GSAElement2D)speckleObject;

      gsaElement.Type = speckleElement.type.ToNative();
      gsaElement.Colour = speckleElement.colour?.ColourToNative() ?? Colour.NotSet;
      gsaElement.Dummy = speckleElement.isDummy;
      gsaElement.Group = speckleElement.group.IsPositiveOrNull();
      return gsaRecords;
    }

    private List<GsaRecord> Element2dToNative(Base speckleObject) //an element2d obj from csi, or from a revit floor/slab, is more akin to a gsamember2d -> expect to convert to member instead of element2d
    {
      var retList = new List<GsaRecord>();
      var speckleElement = (Element2D)speckleObject;

      if (speckleElement.memberType != Objects.Structural.Geometry.MemberType.NotSet)
      {
        var speckleMember = new GSAMember2D()
        {
          applicationId = speckleElement.applicationId,
          topology = speckleElement.topology,
          topologyRefs = speckleElement.topologyRefs,
          name = speckleElement.name,
          property = speckleElement.property,
          memberType = speckleElement.memberType,
          offset = speckleElement.offset,
          orientationAngle = speckleElement.orientationAngle,
          outline = speckleElement.outline,
          voids = speckleElement.voids,
          units = speckleElement.units
        };
        return GSAMember2dToNative(speckleMember);
      }

      var gsaElement = new GsaEl()
      {
        ApplicationId = speckleElement.applicationId,
        Index = speckleElement.GetIndex<GsaEl>(),
        Name = speckleElement.name,
        PropertyIndex = IndexByConversionOrLookup<GsaProp2d>(speckleElement.property, ref retList),
        ReleaseInclusion = ReleaseInclusion.NotIncluded,
        ParentIndex = IndexByConversionOrLookup<GsaMemb>(speckleElement.parent, ref retList)
      };
      if (speckleElement.topology != null && speckleElement.topology.Count > 0)
      {
        gsaElement.NodeIndices = speckleElement.topology.NodeAt(conversionFactors);
        if (speckleElement.topology.Count == 4) gsaElement.Type = ElementType.Quad4;
        else if (speckleElement.topology.Count == 8) gsaElement.Type = ElementType.Quad8;
        else if (speckleElement.topology.Count == 3) gsaElement.Type = ElementType.Triangle3;
        else if (speckleElement.topology.Count == 6) gsaElement.Type = ElementType.Triangle6;
      }
      else if (speckleElement.topologyRefs != null && speckleElement.topologyRefs.Count > 0)
      {
        var nodes = speckleElement.topologyRefs.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();  //ContextObjects.Where(o => speckleElement.topologyRefs.Contains(o.applicationId)).Select(n => (Node)n).ToList();
        gsaElement.NodeIndices = nodes.NodeAt(conversionFactors);

        if (speckleElement.topologyRefs.Count == 4) gsaElement.Type = ElementType.Quad4;
        else if (speckleElement.topologyRefs.Count == 8) gsaElement.Type = ElementType.Quad8;
        else if (speckleElement.topologyRefs.Count == 3) gsaElement.Type = ElementType.Triangle3;
        else if (speckleElement.topologyRefs.Count == 6) gsaElement.Type = ElementType.Triangle6;
      }

      if (speckleElement.orientationAngle != 0) gsaElement.Angle = conversionFactors.ConversionFactorToDegrees() * speckleElement.orientationAngle;
      if (speckleElement.offset != 0) gsaElement.OffsetZ = conversionFactors.length * speckleElement.offset;

      retList.Add(gsaElement);
      return retList;
    }

    private int? IndexByConversionOrLookup<N>(Base obj, ref List<GsaRecord> extra)
    {
      if (obj == null || string.IsNullOrEmpty(obj.applicationId))
      {
        return null;
      }

      //if it's somehow been converted or provisioned by the usual conversion code block (i.e. not through embeddedToBeConverted)
      int? index = Instance.GsaModel.Cache.LookupIndex<N>(obj.applicationId);
      var alreadyConverted = (index != null);
      if (index == null)
      {
        index = Instance.GsaModel.Cache.ResolveIndex<N>(obj.applicationId);
      }
      if (index == null)
      {
        return null;
      }

#if !DEBUG
      lock (embeddedToBeConvertedLock)
      {
#endif
      /*
      if (!embeddedToBeConverted.ContainsKey(obj.applicationId))
      {
        embeddedToBeConverted.Add(obj.applicationId, new List<Base>());
      }
      embeddedToBeConverted[obj.applicationId].Add(obj);
      */
      if (!alreadyConverted && !embeddedToBeConverted.ContainsKey(obj.applicationId))
      {
        embeddedToBeConverted[obj.applicationId] = obj;
      }
#if !DEBUG
      }
#endif

      return index;
    }



    private List<GsaRecord> GSAMember1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleMember = (GSAMember1D)speckleObject;
      var gsaMember = new GsaMemb()
      {
        ApplicationId = speckleMember.applicationId,
        Index = speckleMember.GetIndex<GsaMemb>(),
        Name = speckleMember.name,
        Type = ToNative(speckleMember.memberType, 1),
        Colour = speckleMember.colour?.ColourToNative() ?? Colour.NotSet,
        Dummy = speckleMember.isDummy,
        IsIntersector = true,
        OrientationNodeIndex = speckleMember.orientationNode.NodeAt(conversionFactors),
        //TaperOffsetPercentageEnd1 - currently not supported
        //TaperOffsetPercentageEnd2 - currently not supported
        PropertyIndex = IndexByConversionOrLookup<GsaSection>(speckleMember.property, ref retList),
      };

      if (speckleMember.topology != null && speckleMember.topology.Count > 0)
      {
        gsaMember.NodeIndices = speckleMember.topology.NodeAt(conversionFactors);
      }
      else if (speckleMember.topologyRefs != null && speckleMember.topologyRefs.Count > 0)
      {
        var nodes = speckleMember.topologyRefs.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();
        gsaMember.NodeIndices = nodes.NodeAt(conversionFactors);
      }
      else if (speckleMember.baseLine != null)
      {
        var specklePoints = Get1dTopolopgy(speckleMember.baseLine);
        if (specklePoints != null)
        {
          gsaMember.NodeIndices = specklePoints.NodeAt(conversionFactors);
        }
      }

      var dynamicMembers = speckleMember.GetMembers();

      //Dynamic properties
      var exposure = speckleMember.GetDynamicEnum<ExposedSurfaces>("Exposure", dynamicMembers);
      gsaMember.Exposure = exposure == ExposedSurfaces.NotSet ? ExposedSurfaces.ALL : exposure;
      var analysisType = speckleMember.GetDynamicEnum<AnalysisType>("AnalysisType", dynamicMembers);
      gsaMember.AnalysisType = analysisType == AnalysisType.NotSet ? AnalysisType.BEAM : analysisType;
      gsaMember.Fire = speckleMember.GetDynamicEnum<FireResistance>("Fire", dynamicMembers);
      gsaMember.RestraintEnd1 = speckleMember.GetDynamicEnum<Speckle.GSA.API.GwaSchema.Restraint>("RestraintEnd1", dynamicMembers);
      gsaMember.RestraintEnd2 = speckleMember.GetDynamicEnum<Speckle.GSA.API.GwaSchema.Restraint>("RestraintEnd2", dynamicMembers);
      gsaMember.EffectiveLengthType = speckleMember.GetDynamicEnum<EffectiveLengthType>("EffectiveLengthType", dynamicMembers);
      gsaMember.LoadHeightReferencePoint = speckleMember.GetDynamicEnum<LoadHeightReferencePoint>("LoadHeightReferencePoint", dynamicMembers);
      gsaMember.CreationFromStartDays = speckleMember.GetDynamicValue<int>("CreationFromStartDays", dynamicMembers);
      gsaMember.StartOfDryingDays = speckleMember.GetDynamicValue<int>("StartOfDryingDays", dynamicMembers);
      gsaMember.AgeAtLoadingDays = speckleMember.GetDynamicValue<int>("AgeAtLoadingDays", dynamicMembers);
      gsaMember.RemovedAtDays = speckleMember.GetDynamicValue<int>("RemovedAtDays", dynamicMembers);
      gsaMember.MemberHasOffsets = speckleMember.GetDynamicValue<bool>("MemberHasOffsets", dynamicMembers);
      gsaMember.End1AutomaticOffset = speckleMember.GetDynamicValue<bool>("End1AutomaticOffset", dynamicMembers);
      gsaMember.End2AutomaticOffset = speckleMember.GetDynamicValue<bool>("End2AutomaticOffset", dynamicMembers);
      gsaMember.LimitingTemperature = conversionFactors.TemperatureToNative(speckleMember.GetDynamicValue<double?>("LimitingTemperature", dynamicMembers));
      gsaMember.LoadHeight = conversionFactors.length * speckleMember.GetDynamicValue<double?>("LoadHeight", dynamicMembers);
      gsaMember.EffectiveLengthYY = conversionFactors.length * speckleMember.GetDynamicValue<double>("EffectiveLengthYY", dynamicMembers).IsPositiveOrNull();
      gsaMember.PercentageYY = speckleMember.GetDynamicValue<double>("PercentageYY", dynamicMembers).IsPositiveOrNull();
      gsaMember.EffectiveLengthZZ = conversionFactors.length * speckleMember.GetDynamicValue<double>("EffectiveLengthZZ", dynamicMembers).IsPositiveOrNull();
      gsaMember.PercentageZZ = speckleMember.GetDynamicValue<double>("PercentageZZ", dynamicMembers).IsPositiveOrNull();
      gsaMember.EffectiveLengthLateralTorsional = conversionFactors.length
        * speckleMember.GetDynamicValue<double>("EffectiveLengthLateralTorsional", dynamicMembers).IsPositiveOrNull();
      gsaMember.FractionLateralTorsional = speckleMember.GetDynamicValue<double>("FractionLateralTorsional", dynamicMembers).IsPositiveOrNull();

      if (speckleMember.end1Releases != null && GetReleases(speckleMember.end1Releases, out var gsaRelease1, out var gsaStiffnesses1))
      {
        gsaMember.Releases1 = gsaRelease1;
        gsaMember.Stiffnesses1 = gsaStiffnesses1;
      }
      if (speckleMember.end2Releases != null && GetReleases(speckleMember.end2Releases, out var gsaRelease2, out var gsaStiffnesses2))
      {
        gsaMember.Releases2 = gsaRelease2;
        gsaMember.Stiffnesses2 = gsaStiffnesses2;
      }
      if (speckleMember.end1Offset != null && speckleMember.end1Offset.x != 0) gsaMember.End1OffsetX = conversionFactors.length * speckleMember.end1Offset.x;
      if (speckleMember.end2Offset != null && speckleMember.end2Offset.x != 0) gsaMember.End2OffsetX = conversionFactors.length * speckleMember.end2Offset.x;
      if (speckleMember.end1Offset != null && speckleMember.end2Offset != null)
      {
        if (speckleMember.end1Offset.y == speckleMember.end2Offset.y)
        {
          if (speckleMember.end1Offset.y != 0) gsaMember.OffsetY = conversionFactors.length * speckleMember.end1Offset.y;
        }
        else
        {
          gsaMember.OffsetY = conversionFactors.length * speckleMember.end1Offset.y;
          Report.ConversionErrors.Add(new Exception("GSAMember1dToNative: "
            + "Error converting element1d with application id (" + speckleMember.applicationId + "). "
            + "Different y offsets were assigned at either end."
            + "end 1 y offset of " + gsaMember.OffsetY.ToString() + " has been applied"));
        }
        if (speckleMember.end1Offset.z == speckleMember.end2Offset.z)
        {
          if (speckleMember.end1Offset.z != 0) gsaMember.OffsetZ = conversionFactors.length * speckleMember.end1Offset.z;
        }
        else
        {
          gsaMember.OffsetZ = conversionFactors.length * speckleMember.end1Offset.z;
          Report.ConversionErrors.Add(new Exception("GSAMember1dToNative: "
            + "Error converting element1d with application id (" + speckleMember.applicationId + "). "
            + "Different z offsets were assigned at either end."
            + "end 1 z offset of " + gsaMember.OffsetY.ToString() + " has been applied"));
        }
      }

      if (speckleMember.orientationAngle != 0) gsaMember.Angle = conversionFactors.ConversionFactorToDegrees() * speckleMember.orientationAngle;
      else gsaMember.Angle = 0;
      if (speckleMember.group > 0) gsaMember.Group = speckleMember.group;
      if (speckleMember.targetMeshSize > 0) gsaMember.MeshSize = conversionFactors.length * speckleMember.targetMeshSize;

      //Dynamic properties
      var members = speckleMember.GetMembers();
      if (members.ContainsKey("Voids") && speckleMember["Voids"] is List<List<Node>>)
      {
        var speckleVoids = speckleObject["Voids"] as List<List<Node>>;
        gsaMember.Voids = speckleVoids.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("Points") && speckleMember["Points"] is List<Node>)
      {
        var specklePoints = speckleObject["Points"] as List<Node>;
        gsaMember.PointNodeIndices = specklePoints.NodeAt(conversionFactors);
      }
      if (members.ContainsKey("Lines") && speckleMember["Lines"] is List<List<Node>>)
      {
        var speckleLines = speckleObject["Lines"] as List<List<Node>>;
        gsaMember.Polylines = speckleLines.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("Areas") && speckleMember["Areas"] is List<List<Node>>)
      {
        var speckleAreas = speckleObject["Areas"] as List<List<Node>>;
        gsaMember.AdditionalAreas = speckleAreas.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("SpanRestraints") && speckleMember["SpanRestraints"] is List<RestraintDefinition>)
      {
        var speckleSpanRestraints = speckleObject["SpanRestraints"] as List<RestraintDefinition>;
        gsaMember.SpanRestraints = speckleSpanRestraints.Select(s => new RestraintDefinition() { All = s.All, Index = s.Index, Restraint = s.Restraint }).ToList();
      }
      if (members.ContainsKey("PointRestraints") && speckleMember["PointRestraints"] is List<RestraintDefinition>)
      {
        var specklePointRestraints = speckleObject["PointRestraints"] as List<RestraintDefinition>;
        gsaMember.PointRestraints = specklePointRestraints.Select(s => new RestraintDefinition() { All = s.All, Index = s.Index, Restraint = s.Restraint }).ToList();
      }

      retList.Add(gsaMember);
      return retList;
    }

    private List<GsaRecord> GSAMember2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleMember = (GSAMember2D)speckleObject;

      //Dynamic properties
      var dynamicMembers = speckleMember.GetMembers();
      var memberType = Enum.Parse(typeof(MemberType2D), speckleMember.memberType.ToString());

      var gsaMember = new GsaMemb()
      {
        ApplicationId = speckleMember.applicationId,
        Index = speckleMember.GetIndex<GsaMemb>(),
        Name = speckleMember.name,
        Type = ToNative((MemberType2D)memberType),
        AnalysisType = ToNative(speckleMember.analysisType),
        Colour = speckleMember.colour?.ColourToNative() ?? Colour.NotSet,
        Dummy = speckleMember.isDummy,
        IsIntersector = true,

        //Dynamic properties
        Fire = speckleMember.GetDynamicEnum<FireResistance>("Fire", dynamicMembers),
        CreationFromStartDays = speckleMember.GetDynamicValue<int>("CreationFromStartDays", dynamicMembers),
        StartOfDryingDays = speckleMember.GetDynamicValue<int>("StartOfDryingDays", dynamicMembers),
        AgeAtLoadingDays = speckleMember.GetDynamicValue<int>("AgeAtLoadingDays", dynamicMembers),
        RemovedAtDays = speckleMember.GetDynamicValue<int>("RemovedAtDays", dynamicMembers),
        OffsetAutomaticInternal = speckleMember.GetDynamicValue<bool>("OffsetAutomaticInternal", dynamicMembers),
        LimitingTemperature = conversionFactors.TemperatureToNative(speckleMember.GetDynamicValue<double?>("LimitingTemperature", dynamicMembers)),
      };


      var exposure = speckleMember.GetDynamicEnum<ExposedSurfaces>("Exposure");
      gsaMember.Exposure = exposure == ExposedSurfaces.NotSet ? ExposedSurfaces.ALL : exposure;
      var analysisType = speckleMember.GetDynamicEnum<AnalysisType>("AnalysisType");
      //gsaMember.AnalysisType = analysisType == AnalysisType.NotSet ? AnalysisType.LINEAR : analysisType;

      if (speckleMember.property != null)
      {
        gsaMember.PropertyIndex = IndexByConversionOrLookup<GsaProp2d>(speckleMember.property, ref retList);
      }
      if (speckleMember.topology != null && speckleMember.topology.Count > 0)
      {
        gsaMember.NodeIndices = speckleMember.topology.NodeAt(conversionFactors);
      }
      else if (speckleMember.topologyRefs != null && speckleMember.topologyRefs.Count > 0)
      {
        var nodes = speckleMember.topologyRefs.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();
        //var nodes = ContextObjects.Where(o => speckleMember.topologyRefs.Contains(o.applicationId)).Select(n => (Node)n).ToList();
        gsaMember.NodeIndices = nodes.NodeAt(conversionFactors);
      }

      if (speckleMember.orientationAngle != 0) gsaMember.Angle = conversionFactors.ConversionFactorToDegrees() * speckleMember.orientationAngle;
      if (speckleMember.offset != 0) gsaMember.Offset2dZ = conversionFactors.length * speckleMember.offset;
      if (speckleMember.group > 0) gsaMember.Group = speckleMember.group;
      if (speckleMember.targetMeshSize > 0) gsaMember.MeshSize = conversionFactors.length * speckleMember.targetMeshSize;


      if (dynamicMembers.ContainsKey("voids") && speckleMember.voids is List<List<Node>>)
      {
        var speckleVoids = speckleObject["voids"] as List<List<Node>>;
        gsaMember.Voids = speckleVoids.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      else if (speckleMember.voidRefs != null && speckleMember.voidRefs.Count > 0)
      {
        var voidNodes = new List<List<Node>>() { };
        foreach (var loop in speckleMember.voidRefs)
        {
          var nodes = loop.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();
          //var nodes = ContextObjects.Where(o => loop.Contains(o.applicationId)).Select(n => (Node)n).ToList();
          voidNodes.Add(nodes);
        }
        gsaMember.Voids = voidNodes.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (dynamicMembers.ContainsKey("Points"))
      {
        var points = speckleObject["Points"] as List<object>;
        if (points != null)
        {
          var specklePoints = new List<Node> { };
          foreach (var point in points)
            specklePoints.Add(ContextObjects.Where(o => o.applicationId == (string)point).FirstOrDefault() as Node);
          if (specklePoints.Count > 0) gsaMember.PointNodeIndices = specklePoints.NodeAt(conversionFactors);
        }
      }
      if (dynamicMembers.ContainsKey("Lines"))
      {
        var lines = speckleObject["Lines"] as List<object>;
        if (lines != null)
        {
          var speckleLines = new List<List<Node>> { };
          foreach (var line in lines)
          {
            var ln = line as List<object>;
            speckleLines.Add(ln.Select(l => ContextObjects.Where(o => o.applicationId == (string)l).FirstOrDefault() as Node).ToList());
          }
          if (speckleLines.Count > 0) gsaMember.Polylines = speckleLines.Select(v => v.NodeAt(conversionFactors)).ToList();
        }
      }
      if (dynamicMembers.ContainsKey("Areas"))
      {
        var areas = speckleObject["Areas"] as List<object>;
        if (areas != null)
        {
          var speckleAreas = new List<List<Node>> { };
          foreach (var area in areas)
          {
            var ar = area as List<object>;
            speckleAreas.Add(ar.Select(a => ContextObjects.Where(o => o.applicationId == (string)a).FirstOrDefault() as Node).ToList());
          }
          if (speckleAreas.Count > 0) gsaMember.AdditionalAreas = speckleAreas.Select(v => v.NodeAt(conversionFactors)).ToList();
        }
      }
      retList.Add(gsaMember);
      return retList;
    }

    private List<GsaRecord> GSAAssemblyToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleAssembly = (GSAAssembly)speckleObject;
      var gsaAssembly = new GsaAssembly()
      {
        ApplicationId = speckleAssembly.applicationId,
        Index = speckleAssembly.GetIndex<GsaAssembly>(),
        Name = speckleAssembly.name,
        SizeY = conversionFactors.length * speckleAssembly.sizeY,
        SizeZ = conversionFactors.length * speckleAssembly.sizeZ,
        CurveType = Enum.TryParse(speckleAssembly.curveType, true, out CurveType ct) ? ct : CurveType.NotSet,
        PointDefn = Enum.TryParse(speckleAssembly.pointDefinition, true, out PointDefinition pd) ? pd : PointDefinition.NotSet,
        Topo1 = speckleAssembly.end1Node.NodeAt(conversionFactors),
        Topo2 = speckleAssembly.end2Node.NodeAt(conversionFactors),
        OrientNode = speckleAssembly.orientationNode.NodeAt(conversionFactors),
        StoreyIndices = new List<int>(),
        ExplicitPositions = new List<double>(),
      };

      if (speckleAssembly.curveOrder > 0) gsaAssembly.CurveOrder = speckleAssembly.curveOrder;
      if (speckleAssembly.points != null)
      {
        switch (gsaAssembly.PointDefn)
        {
          case PointDefinition.Points:
            gsaAssembly.NumberOfPoints = (int)speckleAssembly.points[0];
            break;
          case PointDefinition.Spacing:
            gsaAssembly.Spacing = conversionFactors.length * speckleAssembly.points[0];
            break;
          case PointDefinition.Storey:
            gsaAssembly.StoreyIndices = speckleAssembly.points.Select(i => (int)i).ToList();
            break;
          case PointDefinition.Explicit:
            gsaAssembly.ExplicitPositions = speckleAssembly.points.Select(p => conversionFactors.length * p).ToList();
            break;
        }
      }
      if (speckleAssembly.entities != null)
      {
        var speckleNodes = speckleAssembly.entities.FindAll(e => e is Node).Select(e => (Node)e).ToList();
        gsaAssembly.IntTopo = speckleNodes.NodeAt(conversionFactors) ?? new List<int>();
        gsaAssembly.ElementIndices = new List<int>();
        gsaAssembly.ElementIndices.AddRange(IndexByConversionOrLookup<GsaEl>(speckleAssembly.entities.FindAll(e => e is Element1D), ref gsaRecords) ?? new List<int>());
        gsaAssembly.ElementIndices.AddRange(IndexByConversionOrLookup<GsaEl>(speckleAssembly.entities.FindAll(e => e is Element2D), ref gsaRecords) ?? new List<int>());
        gsaAssembly.MemberIndices = new List<int>();
        gsaAssembly.MemberIndices.AddRange(IndexByConversionOrLookup<GsaMemb>(speckleAssembly.entities.FindAll(e => e is GSAMember1D), ref gsaRecords) ?? new List<int>());
        gsaAssembly.MemberIndices.AddRange(IndexByConversionOrLookup<GsaMemb>(speckleAssembly.entities.FindAll(e => e is GSAMember2D), ref gsaRecords) ?? new List<int>());
        if (gsaAssembly.ElementIndices.Count() > 0) gsaAssembly.Type = GSAEntity.ELEMENT;
        else if (gsaAssembly.MemberIndices.Count() > 0) gsaAssembly.Type = GSAEntity.MEMBER;
      }
      gsaRecords.Add(gsaAssembly);
      return gsaRecords;
    }

    private List<GsaRecord> GSAGridLineToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleGridLine = (GSAGridLine)speckleObject;

      var gsaGridLine = new GsaGridLine()
      {
        Index = speckleGridLine.GetIndex<GsaGridLine>(),
        Name = speckleGridLine.label,
        ApplicationId = speckleGridLine.applicationId
      };
      if (speckleGridLine.baseLine is Arc)
      {
        gsaGridLine.Type = GridLineType.Arc;
        var speckleArc = (Arc)speckleGridLine.baseLine;
        if (speckleArc.radius != null) gsaGridLine.Length = conversionFactors.length * speckleArc.radius;
        if (speckleArc.plane != null && speckleArc.plane.origin != null)
        {
          var factor = speckleArc.plane.origin.GetScaleFactor(conversionFactors);
          gsaGridLine.XCoordinate = factor * speckleArc.plane.origin.x;
          gsaGridLine.YCoordinate = factor * speckleArc.plane.origin.y;
        }

        if (speckleArc.startAngle != null)
        {
          gsaGridLine.Theta1 = speckleArc.startAngle.Value.Degrees();
        }
        if (speckleArc.endAngle != null)
        {
          gsaGridLine.Theta2 = speckleArc.endAngle.Value.Degrees();
        }
      }
      else if (speckleGridLine.baseLine is Line)
      {
        gsaGridLine.Type = GridLineType.Line;
        var speckleLine = (Line)speckleGridLine.baseLine;
        if (speckleLine.start != null && speckleLine.end != null)
        {
          gsaGridLine.XCoordinate = conversionFactors.length * speckleLine.start.x;
          gsaGridLine.YCoordinate = conversionFactors.length * speckleLine.start.y;

          var a = (speckleLine.end.x - speckleLine.start.x);
          var o = (speckleLine.end.y - speckleLine.start.y);
          var h = Hypotenuse(a, o);

          gsaGridLine.Length = conversionFactors.length * h;
          gsaGridLine.Theta1 = Math.Acos(a / h).Degrees();
        }

      }
      gsaRecords.Add(gsaGridLine);
      return gsaRecords;
    }

    private List<GsaRecord> StoreyToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleStorey = (Storey)speckleObject;

      var gsaGridLine = new GsaGridPlane()
      {
        Index = speckleStorey.GetIndex<GsaGridPlane>(),
        Name = speckleStorey.name,
        ApplicationId = speckleStorey.applicationId,
        Elevation = conversionFactors.length * speckleStorey.elevation,
      };
      gsaRecords.Add(gsaGridLine);
      return gsaRecords;
    }

    private List<GsaRecord> GSAGridPlaneToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleGridPlane = (GSAGridPlane)speckleObject;
      var gsaRecords = StoreyToNative(speckleObject);
      if (gsaRecords == null || gsaRecords.Count == 0 || (!(gsaRecords.First() is GsaGridPlane)))
      {
        return null;
      }

      var gsaGridPlane = (GsaGridPlane)(gsaRecords.First());
      if (speckleGridPlane.toleranceBelow.HasValue || speckleGridPlane.toleranceAbove.HasValue)
      {
        gsaGridPlane.Type = GridPlaneType.Storey;
        if (speckleGridPlane.toleranceAbove == null)
        {
          gsaGridPlane.StoreyToleranceAboveAuto = true;
        }
        else
        {
          gsaGridPlane.StoreyToleranceAbove = conversionFactors.length * speckleGridPlane.toleranceAbove;
        }
        if (speckleGridPlane.toleranceBelow == null)
        {
          gsaGridPlane.StoreyToleranceBelowAuto = true;
        }
        else
        {
          gsaGridPlane.StoreyToleranceBelow = conversionFactors.length * speckleGridPlane.toleranceBelow;
        }
      }
      else
      {
        gsaGridPlane.Type = GridPlaneType.General;
      }

      if (speckleGridPlane.axis == null)
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.NotSet;
      }
      else if (IsGlobalAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.Global;
      }
      else if (IsXElevationAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.XElevation;
      }
      else if (IsYElevationAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.YElevation;
      }
      else
      {
        var axisIndex = IndexByConversionOrLookup<GsaAxis>(speckleGridPlane.axis, ref retList);
        if (axisIndex.IsIndex())
        {
          gsaGridPlane.AxisIndex = axisIndex;
          gsaGridPlane.AxisRefType = GridPlaneAxisRefType.Reference;
        }
      }

      retList.Add(gsaGridPlane);

      return retList;
    }

    private List<GsaRecord> GSAGridSurfaceToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleGridSurface = (GSAGridSurface)speckleObject;

      var gsaGridSurface = new GsaGridSurface()
      {
        Index = speckleGridSurface.GetIndex<GsaGridSurface>(),
        ApplicationId = speckleGridSurface.applicationId,
        Name = speckleGridSurface.name,
        Tolerance = conversionFactors.length * speckleGridSurface.tolerance,
        Angle = conversionFactors.ConversionFactorToDegrees() * speckleGridSurface.spanDirection,
        Expansion = speckleGridSurface.loadExpansion.ToNative(),
        Span = speckleGridSurface.span.ToNative()
      };

      if (speckleGridSurface.elements != null)
      {
        //The ElementIndices collection should be initialised by the GsaGridSurface constructor
        foreach (var e in speckleGridSurface.elements)
        {
          var isMemb = ((e is GSAMember1D) || (e is GSAMember2D));
          bool found = false;
          if (!string.IsNullOrEmpty(e.applicationId))
          {
            if (isMemb)
            {
              var index = Instance.GsaModel.Cache.LookupIndex<GsaMemb>(e.applicationId);
              if (index.HasValue)
              {
                gsaGridSurface.MemberIndices.Add(index.Value);
                found = true;
              }
            }
            else
            {
              var index = Instance.GsaModel.Cache.LookupIndex<GsaEl>(e.applicationId);
              if (index.HasValue)
              {
                gsaGridSurface.ElementIndices.Add(index.Value);
                found = true;
              }
            }
          }
          if (!found)
          {
            int? index = null;
            var nativeObjects = new List<GsaRecord>();
            if (isMemb)
            {
              if (e is GSAMember1D)
              {
                nativeObjects.AddRange(GSAMember1dToNative(e));
              }
              else if (e is GSAMember2D)
              {
                nativeObjects.AddRange(GSAMember2dToNative(e));
              }
              if (nativeObjects.Count > 0)
              {
                var newMemb = nativeObjects.FirstOrDefault(o => o is GsaMemb);
                if (newMemb != null)
                {
                  index = newMemb.Index;
                }
              }
              if (index.HasValue)
              {
                gsaGridSurface.MemberIndices.Add(index.Value);
              }
            }
            else  //element by default otherwise
            {
              if (e is GSAElement1D)
              {
                nativeObjects.AddRange(GSAElement1dToNative(e));
              }
              else if (e is Element1D)
              {
                nativeObjects.AddRange(Element1dToNative(e));
              }
              else if (e is GSAElement2D)
              {
                nativeObjects.AddRange(GSAMember2dToNative(e));
              }
              else if (e is Element2D)
              {
                nativeObjects.AddRange(Element2dToNative(e));
              }
              if (nativeObjects.Count > 0)
              {
                var newEl = nativeObjects.FirstOrDefault(o => o is GsaEl);
                if (newEl != null)
                {
                  index = newEl.Index;
                }
              }
              if (index.HasValue)
              {
                gsaGridSurface.ElementIndices.Add(index.Value);
              }
            }
            retList.AddRange(nativeObjects);
          }
        }

        if (speckleGridSurface.elements.Any(e => ((e is GSAElement1D) || (e is GSAMember1D) || (e is Element1D))))
        {
          gsaGridSurface.Type = GridSurfaceElementsType.OneD;
        }
        else if (speckleGridSurface.elements.Any(e => ((e is GSAElement2D) || (e is GSAMember2D) || (e is Element2D))))
        {
          gsaGridSurface.Type = GridSurfaceElementsType.TwoD;
        }
      }

      if (speckleGridSurface.gridPlane != null)
      {
        if (!string.IsNullOrEmpty(speckleGridSurface.gridPlane.applicationId))
        {
          var planeIndex = Instance.GsaModel.Cache.LookupIndex<GsaGridPlane>(speckleGridSurface.gridPlane.applicationId);
          if (planeIndex.IsIndex())
          {
            gsaGridSurface.PlaneIndex = planeIndex;
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.Reference;
          }
          else
          {
            var planeGsaRecords = GSAGridPlaneToNative(speckleGridSurface.gridPlane);
            var gsaPlane = planeGsaRecords.FirstOrDefault(r => r is GsaGridPlane);
            if (gsaPlane != null && gsaPlane.Index.IsIndex())
            {
              gsaGridSurface.PlaneIndex = gsaPlane.Index;
              gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.Reference;
              retList.Add(gsaPlane);
            }
          }
        }
        else if (speckleGridSurface.gridPlane.axis != null)
        {
          if (IsGlobalAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.Global;
          }
          else if (IsXElevationAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.XElevation;
          }
          else if (IsYElevationAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.YElevation;
          }
        }
      }

      retList.Add(gsaGridSurface);
      return retList;
    }

    private double Hypotenuse(double a, double o) => Math.Sqrt((a * a) + (o * o));

    private bool IsGlobalAxis(Axis x) => ((x.axisType == AxisType.Cartesian) && ((x.definition == null)
      || (x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
          && x.definition.xdir.Equals(UnitX, GeometricDecimalPlaces)
          && x.definition.ydir.Equals(UnitY, GeometricDecimalPlaces)
          && x.definition.normal.Equals(UnitZ, GeometricDecimalPlaces))));

    private bool IsXElevationAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitY * -1, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitX * -1, GeometricDecimalPlaces));

    private bool IsYElevationAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitX, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitY * -1, GeometricDecimalPlaces));

    /* not used (yet) - review if needed
    private bool IsVerticalAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitY, GeometricDecimalPlaces));
    */

    private List<GsaRecord> GSAPolylineToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var specklePolyline = (GSAPolyline)speckleObject;
      var gsaPolyline = new GsaPolyline()
      {
        ApplicationId = specklePolyline.applicationId,
        Index = specklePolyline.GetIndex<GsaPolyline>(),
        Name = specklePolyline.name,
        GridPlaneIndex = IndexByConversionOrLookup<GsaGridPlane>(specklePolyline.gridPlane, ref gsaRecords),
        NumDim = specklePolyline.description.Is3d() ? 3 : 2,
        Values = specklePolyline.description.GetValues().Select(v => conversionFactors.length * v).ToList(),
        //Units = specklePolyline.units, //TO DO: remove units from interim schema as its not used in Gwa string
        Colour = specklePolyline.colour.ColourToNative(),
      };
      gsaRecords.Add(gsaPolyline);
      return gsaRecords;
    }

    #endregion

    #region Loading
    private List<GsaRecord> GSALoadCaseToNative(Base speckleObject)
    {
      var gsaRecords = LoadCaseToNative(speckleObject);
      var gsaLoadCase = (GsaLoadCase)gsaRecords.First(o => o is GsaLoadCase);
      var speckleLoadCase = (GSALoadCase)speckleObject;
      //if (speckleLoadCase.direction != null) gsaLoadCase.Direction = speckleLoadCase.direction.ToNative(); //TO DO: figure out if direction keyword is actually used in GSA?
      gsaLoadCase.Include = speckleLoadCase.include.IncludeOptionToNative();
      if (speckleLoadCase.bridge) gsaLoadCase.Bridge = true;
      return gsaRecords;
    }

    private List<GsaRecord> LoadCaseToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoadCase = (LoadCase)speckleObject;
      var gsaLoadCase = new GsaLoadCase()
      {
        ApplicationId = speckleLoadCase.applicationId,
        Index = speckleLoadCase.GetIndex<GsaLoadCase>(),
        Title = speckleLoadCase.name,
        CaseType = speckleLoadCase.loadType.ToNative()
      };
      if (!string.IsNullOrEmpty(speckleLoadCase.description)) gsaLoadCase.Category = speckleLoadCase.description.LoadCategoryToNative();
      else gsaLoadCase.Category = LoadCategory.NotSet;
      if (!string.IsNullOrEmpty(speckleLoadCase.group) && int.TryParse(speckleLoadCase.group, out int group))
      {
        gsaLoadCase.Source = group;
      }
      gsaRecords.Add(gsaLoadCase);
      return gsaRecords;
    }

    private List<GsaRecord> GSAAnalysisTaskToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleTask = (GSAAnalysisTask)speckleObject;
      var gsaTask = new GsaTask()
      {
        ApplicationId = speckleTask.applicationId,
        Index = speckleTask.GetIndex<GsaTask>(),
        Name = speckleTask.name,
        StageIndex = IndexByConversionOrLookup<GsaAnalStage>(speckleTask.stage, ref gsaRecords),
        Solver = speckleTask.solutionType.ToNativeSolver(),
        Solution = speckleTask.solutionType.ToNative(),
        Mode1 = speckleTask.modeParameter1,
        Mode2 = speckleTask.modeParameter2,
        NumIter = speckleTask.numIterations,
        PDelta = speckleTask.PDeltaOption,
        PDeltaCase = speckleTask.PDeltaCase,
        Prestress = speckleTask.PrestressCase,
        Result = speckleTask.resultSyntax,
        Prune = speckleTask.prune.ToNative(),
        Geometry = speckleTask.geometry.ToNative(),
        Lower = speckleTask.lower,
        Upper = speckleTask.upper,
        Raft = speckleTask.raft.ToNative(),
        Residual = speckleTask.residual.ToNative(),
        Shift = speckleTask.shift,
        Stiff = speckleTask.stiff,
        MassFilter = speckleTask.massFilter,
        MaxCycle = speckleTask.maxCycle
      };

      if (speckleTask.analysisCases != null)
      {
        foreach (var speckleLoadCase in speckleTask.analysisCases)
        {
          var gsaLoadCase = new GsaAnal()
          {
            ApplicationId = speckleLoadCase.applicationId,
            Index = speckleLoadCase.GetIndex<GsaAnal>(),
            Name = speckleLoadCase.name,
            TaskIndex = gsaTask.Index,
            Desc = GetAnalysisCaseDescription(speckleLoadCase.loadCases, speckleLoadCase.loadFactors, ref gsaRecords),
          };
          gsaRecords.Add(gsaLoadCase);
        }
      }

      if (speckleTask.stage != null)
      {
        var speckleStage = speckleTask.stage;
        var gsaStage = AnalStageToNative(speckleStage);
        gsaRecords.AddRange(gsaStage);
      }
      gsaRecords.Add(gsaTask);
      return gsaRecords;
    }

    private List<GsaRecord> GSAAnalysisCaseToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleCase = (GSAAnalysisCase)speckleObject;
      var caseIndex = IndexByConversionOrLookup<GsaAnal>(speckleCase, ref gsaRecords);
      if (caseIndex == null)
      {
        var gsaCase = new GsaAnal()
        {
          ApplicationId = speckleCase.applicationId,
          Index = speckleCase.GetIndex<GsaAnal>(),
          Name = speckleCase.name,
          Desc = GetAnalysisCaseDescription(speckleCase.loadCases, speckleCase.loadFactors, ref gsaRecords),
        };
        if ((GSAAnalysisTask)speckleCase["@task"] != null) gsaCase.TaskIndex = IndexByConversionOrLookup<GsaTask>((GSAAnalysisTask)speckleCase["@task"], ref gsaRecords);
        gsaRecords.Add(gsaCase);
      }
      return gsaRecords;
    }

    private List<GsaRecord> GSACombinationCaseToNative(Base speckleObject)
    {
      var gsaRecords = LoadCombinationToNative(speckleObject);
      var GSACombinationCase = (GsaCombination)gsaRecords.First(o => o is GsaCombination);
      var speckleLoadCombination = (GSACombinationCase)speckleObject;
      GSACombinationCase.Bridge = speckleLoadCombination.GetDynamicValue<bool?>("bridge");
      GSACombinationCase.Note = speckleLoadCombination.GetDynamicValue<string>("note");
      return gsaRecords;
    }

    private List<GsaRecord> LoadCombinationToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoadCombination = (LoadCombination)speckleObject;
      var GSACombinationCase = new GsaCombination()
      {
        ApplicationId = speckleLoadCombination.applicationId,
        Index = speckleLoadCombination.GetIndex<GsaCombination>(),
        Name = speckleLoadCombination.name,
        Desc = GetLoadCombinationDescription(speckleLoadCombination.combinationType, speckleLoadCombination.loadCases, speckleLoadCombination.loadFactors, ref gsaRecords),
      };
      gsaRecords.Add(GSACombinationCase);
      return gsaRecords;
    }

    #region LoadBeam
    private List<GsaRecord> GSALoadBeamToNative(Base speckleObject)
    {
      var gsaRecords = LoadBeamToNative(speckleObject);
      var gsaLoad = (GsaLoadBeam)gsaRecords.First(o => o is GsaLoadBeam);
      var speckleLoad = (GSALoadBeam)speckleObject;
      //Add any app specific conversions here
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (LoadBeam)speckleObject;

      var fns = new Dictionary<BeamLoadType, Func<LoadBeam, List<GsaRecord>>>
      { { BeamLoadType.Uniform, LoadBeamUniformToNative },
        { BeamLoadType.Linear, LoadBeamLinearToNative },
        { BeamLoadType.Point, LoadBeamPointToNative },
        { BeamLoadType.Patch, LoadBeamPatchToNative },
        { BeamLoadType.TriLinear, LoadBeamTriLinearToNative },
      };

      //Apply spring type specific properties
      if (fns.ContainsKey(speckleLoad.loadType))
      {
        gsaRecords.AddRange(fns[speckleLoad.loadType](speckleLoad));
      }
      else
      {
        Report.ConversionErrors.Add(new Exception("LoadBeamToNative: beam load type (" + speckleLoad.loadType.ToString() + ") is not currently supported"));
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamUniformToNative(LoadBeam speckleLoad)
    {
      var gsaRecords = LoadBeamBaseToNative<GsaLoadBeamUdl>(speckleLoad);
      var gsaLoad = (GsaLoadBeamUdl)gsaRecords.First(o => o is GsaLoadBeamUdl);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 1)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Load = factor * speckleLoad.values[0];
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamLinearToNative(LoadBeam speckleLoad)
    {
      var gsaRecords = LoadBeamBaseToNative<GsaLoadBeamLine>(speckleLoad);
      var gsaLoad = (GsaLoadBeamLine)gsaRecords.First(o => o is GsaLoadBeamLine);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Load1 = factor * speckleLoad.values[0];
        gsaLoad.Load2 = factor * speckleLoad.values[1];
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamPointToNative(LoadBeam speckleLoad)
    {
      var gsaRecords = LoadBeamBaseToNative<GsaLoadBeamPoint>(speckleLoad);
      var gsaLoad = (GsaLoadBeamPoint)gsaRecords.First(o => o is GsaLoadBeamPoint);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 1)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Load = factor * speckleLoad.values[0];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 1)
      {
        gsaLoad.Position = conversionFactors.length * speckleLoad.positions[0];
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamPatchToNative(LoadBeam speckleLoad)
    {
      var gsaRecords = LoadBeamBaseToNative<GsaLoadBeamPatch>(speckleLoad);
      var gsaLoad = (GsaLoadBeamPatch)gsaRecords.First(o => o is GsaLoadBeamPatch);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Load1 = factor * speckleLoad.values[0];
        gsaLoad.Load2 = factor * speckleLoad.values[1];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.Position1 = conversionFactors.length * speckleLoad.positions[0];
        gsaLoad.Position2Percent = conversionFactors.length * speckleLoad.positions[1];
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamTriLinearToNative(LoadBeam speckleLoad)
    {
      var gsaRecords = LoadBeamBaseToNative<GsaLoadBeamTrilin>(speckleLoad);
      var gsaLoad = (GsaLoadBeamTrilin)gsaRecords.First(o => o is GsaLoadBeamTrilin);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Load1 = factor * speckleLoad.values[0];
        gsaLoad.Load2 = factor * speckleLoad.values[1];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.Position1 = conversionFactors.length * speckleLoad.positions[0];
        gsaLoad.Position2Percent = conversionFactors.length * speckleLoad.positions[1];
      }
      return gsaRecords;
    }

    private List<GsaRecord> LoadBeamBaseToNative<T>(LoadBeam speckleLoad) where T : GsaLoadBeam
    {
      var gsaRecords = new List<GsaRecord>();
      var gsaLoad = (T)Activator.CreateInstance(typeof(T));
      gsaLoad.ApplicationId = speckleLoad.applicationId;
      gsaLoad.Index = speckleLoad.GetIndex<T>();
      gsaLoad.Name = speckleLoad.name;
      gsaLoad.LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords);
      gsaLoad.Projected = speckleLoad.isProjected;
      gsaLoad.LoadDirection = speckleLoad.direction.ToNative();
      if (speckleLoad.elements != null)
      {
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleLoad.elements.FindAll(o => o is Element1D || o is Element2D), ref gsaRecords) ?? new List<int>();
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleLoad.elements.FindAll(o => o is GSAMember1D || o is GSAMember2D), ref gsaRecords) ?? new List<int>();
      }
      else if (speckleLoad.elementRefs != null)
      {
        var elements = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is Element1D || o is Element2D);
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(elements, ref gsaRecords) ?? new List<int>();

        var members = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is GSAMember1D || o is GSAMember2D);
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(members, ref gsaRecords) ?? new List<int>();
      }
      if (speckleLoad.loadAxis == null)
      {
        gsaLoad.AxisRefType = speckleLoad.loadAxisType.ToNativeBeamAxisRefType();
      }
      else
      {
        if (GetLoadAxis(speckleLoad.loadAxis, out LoadBeamAxisRefType gsaAxisRefType, out var gsaAxisIndex, ref gsaRecords))
        {
          gsaLoad.AxisRefType = gsaAxisRefType;
          gsaLoad.AxisIndex = gsaAxisIndex;
        }
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }
    #endregion

    private List<GsaRecord> GSALoadFaceToNative(Base speckleObject)
    {
      var gsaRecords = LoadFaceToNative(speckleObject);
      var gsaLoad = (GsaLoad2dFace)gsaRecords.First(o => o is GsaLoad2dFace);
      var speckleLoad = (GSALoadFace)speckleObject;
      //Add any app specific conversions here
      return gsaRecords;
    }

    private List<GsaRecord> LoadFaceToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (LoadFace)speckleObject;
      var factor = speckleLoad.GetScaleFactor(conversionFactors);
      var gsaLoad = new GsaLoad2dFace()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoad2dFace>(),
        Name = speckleLoad.name,
        Type = speckleLoad.loadType.ToNative(),
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        Values = speckleLoad.values.Select(v => factor * v).ToList(),
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
      };
      if (speckleLoad.elements != null)
      {
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleLoad.elements.FindAll(o => o is Element1D || o is Element2D), ref gsaRecords) ?? new List<int>();
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleLoad.elements.FindAll(o => o is GSAMember1D || o is GSAMember2D), ref gsaRecords) ?? new List<int>();
      }
      else if (speckleLoad.elementRefs != null)
      {
        var elements = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is Element1D || o is Element2D);
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(elements, ref gsaRecords) ?? new List<int>();

        var members = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is GSAMember1D || o is GSAMember2D);
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(members, ref gsaRecords) ?? new List<int>();
      }
      if (GetLoadAxis(speckleLoad.loadAxis, speckleLoad.loadAxisType, out var gsaAxisRefType, out var gsaAxisIndex, ref gsaRecords))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.R = conversionFactors.length * speckleLoad.positions[0];
        gsaLoad.S = conversionFactors.length * speckleLoad.positions[1];
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadNodeToNative(Base speckleObject)
    {
      var gsaRecords = LoadNodeToNative(speckleObject);
      var gsaLoad = (GsaLoadNode)gsaRecords.First(o => o is GsaLoadNode);
      var speckleLoad = (GSALoadNode)speckleObject;
      //Add any app specific conversions here
      return gsaRecords;
    }

    private List<GsaRecord> LoadNodeToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (LoadNode)speckleObject;
      var gsaLoad = new GsaLoadNode()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadNode>(),
        Name = speckleLoad.name,
        LoadDirection = speckleLoad.direction.ToNative(),
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        NodeIndices = speckleLoad.nodes.NodeAt(conversionFactors),
      };
      if (speckleLoad.nodes != null)
      {
        gsaLoad.NodeIndices = speckleLoad.nodes.NodeAt(conversionFactors);
      }
      else if (speckleLoad.nodeRefs != null)
      {
        var nodes = ContextObjects.Where(o => speckleLoad.nodeRefs.Contains(o.applicationId)).Select(n => (Node)n).ToList();
        gsaLoad.NodeIndices = nodes.NodeAt(conversionFactors);
      }
      if (speckleLoad.value != 0)
      {
        var factor = speckleLoad.GetScaleFactor(conversionFactors);
        gsaLoad.Value = factor * speckleLoad.value;
      }
      if ((speckleLoad.loadAxis == null) || (speckleLoad.loadAxis.definition.IsGlobal()))
      {
        gsaLoad.GlobalAxis = true;
      }
      else
      {
        gsaLoad.GlobalAxis = false;
        gsaLoad.AxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleLoad.loadAxis, ref gsaRecords);
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadGravityToNative(Base speckleObject)
    {
      var gsaRecords = LoadGravityToNative(speckleObject);
      var gsaLoad = (GsaLoadGravity)gsaRecords.First(o => o is GsaLoadGravity);
      var speckleLoad = (GSALoadGravity)speckleObject;
      //Add any app specific conversions here
      return gsaRecords;
    }

    private List<GsaRecord> LoadGravityToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (LoadGravity)speckleObject;
      var gsaLoad = new GsaLoadGravity()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGravity>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords)
      };

      if (speckleLoad.nodes != null)
      {
        gsaLoad.Nodes = speckleLoad.nodes.Select(n => (Node)n).ToList().NodeAt(conversionFactors);
      }
      else if (speckleLoad.nodeRefs != null)
      {
        var nodes = ContextObjects.Where(o => speckleLoad.nodeRefs.Contains(o.applicationId)).Select(n => (Node)n).ToList();
        gsaLoad.Nodes = nodes.Select(n => (Node)n).ToList().NodeAt(conversionFactors);
      }
      if (speckleLoad.elements != null)
      {
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleLoad.elements.FindAll(o => o is Element1D || o is Element2D), ref gsaRecords) ?? new List<int>();
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleLoad.elements.FindAll(o => o is GSAMember1D || o is GSAMember2D), ref gsaRecords) ?? new List<int>();
      }
      else if (speckleLoad.elementRefs != null)
      {
        var elements = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is Element1D || o is Element2D);
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(elements, ref gsaRecords) ?? new List<int>();

        var members = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is GSAMember1D || o is GSAMember2D);
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(members, ref gsaRecords) ?? new List<int>();
      }
      if (speckleLoad.gravityFactors != null)
      {
        //both speckle and native objects should be in units of g so no conversion factors should be required
        if (speckleLoad.gravityFactors.x != 0) gsaLoad.X = speckleLoad.gravityFactors.x;
        if (speckleLoad.gravityFactors.y != 0) gsaLoad.Y = speckleLoad.gravityFactors.y;
        if (speckleLoad.gravityFactors.z != 0) gsaLoad.Z = speckleLoad.gravityFactors.z;
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadThermal2dToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (GSALoadThermal2d)speckleObject;
      var gsaLoad = new GsaLoad2dThermal()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoad2dThermal>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        Type = speckleLoad.type.ToNative(),
        Values = speckleLoad.values.Select(v => (double)conversionFactors.TemperatureToNative(v)).ToList(),
      };
      if (speckleLoad.elements != null)
      {
        var speckleElements = speckleLoad.elements.Select(o => (Base)o).ToList();
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleElements.FindAll(o => o is Element2D), ref gsaRecords) ?? new List<int>();
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleElements.FindAll(o => o is GSAMember2D), ref gsaRecords) ?? new List<int>();
      }
      else if (speckleLoad.elementRefs != null)
      {
        var elements = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is Element2D);
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(elements, ref gsaRecords) ?? new List<int>();

        var members = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is GSAMember2D);
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(members, ref gsaRecords) ?? new List<int>();
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadThermal1dToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (GSALoadThermal1d)speckleObject;
      var gsaLoad = new GsaLoad1dThermal()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoad1dThermal>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        Type = speckleLoad.type.ToNative(),
        Values = speckleLoad.values.Select(v => (double)conversionFactors.TemperatureToNative(v)).ToList()
      };

      if (speckleLoad.elements != null)
      {
        var speckleElements = speckleLoad.elements.Select(o => (Base)o).ToList();
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleElements.FindAll(o => o is Element1D), ref gsaRecords) ?? new List<int>();
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleElements.FindAll(o => o is GSAMember1D), ref gsaRecords) ?? new List<int>();
      }
      else if (speckleLoad.elementRefs != null)
      {
        var elements = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is Element1D);
        gsaLoad.ElementIndices = IndexByConversionOrLookup<GsaEl>(elements, ref gsaRecords) ?? new List<int>();

        var members = ContextObjects.Where(o => speckleLoad.elementRefs.Contains(o.applicationId)).ToList().FindAll(o => o is GSAMember1D);
        gsaLoad.MemberIndices = IndexByConversionOrLookup<GsaMemb>(members, ref gsaRecords) ?? new List<int>();
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadGridPointToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (GSALoadGridPoint)speckleObject;
      var gsaLoad = new GsaLoadGridPoint()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridPoint>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        GridSurfaceIndex = IndexByConversionOrLookup<GsaGridSurface>(speckleLoad.gridSurface, ref gsaRecords),
        LoadDirection = speckleLoad.direction.ToNative(),
      };
      if (speckleLoad.value != 0)
      {
        //var factor = string.IsNullOrEmpty(speckleLoad.units) ? conversionFactors.force : conversionFactors.ConversionFactorToNative(UnitDimension.Force, speckleLoad.units);
        //gsaLoad.Value = factor * speckleLoad.value;
        gsaLoad.Value = conversionFactors.force * speckleLoad.value;
      }
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex, ref gsaRecords))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.position != null)
      {
        if (speckleLoad.position.x != 0) gsaLoad.X = conversionFactors.length * speckleLoad.position.x;
        if (speckleLoad.position.y != 0) gsaLoad.Y = conversionFactors.length * speckleLoad.position.y;
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadGridLineToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (GSALoadGridLine)speckleObject;
      var gsaLoad = new GsaLoadGridLine()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridLine>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        GridSurfaceIndex = IndexByConversionOrLookup<GsaGridSurface>(speckleLoad.gridSurface, ref gsaRecords),
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
      };
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex, ref gsaRecords))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.values != null && speckleLoad.values.Count >= 2)
      {
        var factor = conversionFactors.force / conversionFactors.length; //TO DO: handle case where units are specified within the speckle object
        if (speckleLoad.values[0] != 0) gsaLoad.Value1 = factor * speckleLoad.values[0];
        if (speckleLoad.values[1] != 0) gsaLoad.Value2 = factor * speckleLoad.values[1];
      }
      if (GetPolyline(speckleLoad.polyline, out LoadLineOption gsaOption, out var gsaPolygon, out var gsaPolygonIndex))
      {
        gsaLoad.Line = gsaOption;
        gsaLoad.Polygon = gsaPolygon;
        gsaLoad.PolygonIndex = gsaPolygonIndex;
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }

    private List<GsaRecord> GSALoadGridAreaToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleLoad = (GSALoadGridArea)speckleObject;
      var gsaLoad = new GsaLoadGridArea()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridArea>(),
        Name = speckleLoad.name,
        LoadCaseIndex = IndexByConversionOrLookup<GsaLoadCase>(speckleLoad.loadCase, ref gsaRecords),
        GridSurfaceIndex = IndexByConversionOrLookup<GsaGridSurface>(speckleLoad.gridSurface, ref gsaRecords),
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
      };
      if (speckleLoad.value != 0)
      {
        var factor = conversionFactors.force / Math.Pow(conversionFactors.length, 2); //TO DO: handle case where units are specified within the speckle object
        gsaLoad.Value = factor * speckleLoad.value;
      }
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex, ref gsaRecords))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (GetPolyline(speckleLoad.polyline, out LoadAreaOption gsaOption, out var gsaPolygon, out var gsaPolygonIndex))
      {
        gsaLoad.Area = gsaOption;
        gsaLoad.Polygon = gsaPolygon;
        gsaLoad.PolygonIndex = gsaPolygonIndex;
      }
      gsaRecords.Add(gsaLoad);
      return gsaRecords;
    }
    #endregion

    #region Materials
    private List<GsaRecord> GSASteelToNative(Base speckleObject)
    {
      var gsaRecords = SteelToNative(speckleObject);
      var gsaSteel = (GsaMatSteel)gsaRecords.First(o => o is GsaMatSteel);
      var speckleSteel = (GSASteel)speckleObject;
      var speckleMat = GetMat(speckleSteel.GetDynamicValue<Base>("Mat"));
      if (speckleMat != null)
      {
        gsaSteel.Mat = speckleMat;
      }
      return gsaRecords;
    }

    private List<GsaRecord> SteelToNative(Base speckleObject)
    {
      //Values based on GSA10.1 with design code AS4100-1998, material grade 200-450 from AS3678
      var speckleSteel = (Steel)speckleObject;
      double? e = null, eh = null, fy = null, fu = null, nu = null, g = null, rho = null, alpha = null, damp = null, epsMax = null, eps = null, cost = null;

      if (speckleSteel.yieldStrength > 0)
      {
        fy = speckleSteel.yieldStrength * conversionFactors.stress;
        eps = GetSteelStrain(speckleSteel.yieldStrength) * StrainUnits.GetConversionFactor(StrainUnits.Strain, conversionFactors.nativeModelUnits.strain);
      }
      if (speckleSteel.elasticModulus > 0) e = speckleSteel.elasticModulus * conversionFactors.stress;
      if (speckleSteel.poissonsRatio > 0) nu = speckleSteel.poissonsRatio;
      if (speckleSteel.shearModulus > 0) g = speckleSteel.shearModulus * conversionFactors.stress;
      if (speckleSteel.density > 0) rho = speckleSteel.density * conversionFactors.DensityFactorToNative();
      if (speckleSteel.thermalExpansivity > 0) alpha = speckleSteel.thermalExpansivity * conversionFactors.ThermalExapansionFactorToNative();
      if (speckleSteel.maxStrain > 0) epsMax = speckleSteel.maxStrain * conversionFactors.strain;
      if (speckleSteel.ultimateStrength > 0) fu = speckleSteel.ultimateStrength * conversionFactors.stress;
      if (speckleSteel.strainHardeningModulus > 0) eh = speckleSteel.strainHardeningModulus * conversionFactors.stress;
      if (speckleSteel.cost > 0) cost = speckleSteel.cost;

      var gsaSteel = new GsaMatSteel()
      {
        ApplicationId = speckleSteel.applicationId,
        Index = speckleSteel.GetIndex<GsaMatSteel>(),
        Mat = new GsaMat()
        {
          Name = speckleSteel.name,
          E = e,
          F = fy,
          Nu = nu,
          G = g,
          Rho = rho,
          Alpha = alpha,
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = e,
            Nu = nu,
            Rho = rho,
            Alpha = alpha,
            G = g,
            Damp = damp,
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = null,
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = null,
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = null,
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = null,
          Eps = epsMax,
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = eps,
            StrainElasticTension = eps,
            StrainPlasticCompression = eps,
            StrainPlasticTension = eps,
            StrainFailureCompression = epsMax,
            StrainFailureTension = epsMax,
            GammaF = 1,
            GammaE = 1,
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = eps,
            StrainElasticTension = eps,
            StrainPlasticCompression = eps,
            StrainPlasticTension = eps,
            StrainFailureCompression = epsMax,
            StrainFailureTension = epsMax,
            GammaF = 1,
            GammaE = 1,
          },
          Cost = cost,
          Type = MatType.STEEL,
        },
        Fy = fy,
        Fu = fu,
        EpsP = null,
        Eh = eh,
      };

      if (!string.IsNullOrEmpty(speckleSteel.name))
      {
        gsaSteel.Name = speckleSteel.name;
      }

      //TODO:
      //SpeckleObject:
      //  string grade
      //  string designCode
      //  string codeYear
      //  double strength
      //  double materialSafetyFactor
      return new List<GsaRecord>() { gsaSteel };
    }

    private List<GsaRecord> GSAConcreteToNative(Base speckleObject)
    {
      var gsaRecords = ConcreteToNative(speckleObject);
      var gsaConcrete = (GsaMatConcrete)gsaRecords.First(o => o is GsaMatConcrete);
      var speckleConcrete = (GSAConcrete)speckleObject;

      var dynamicMembers = speckleConcrete.GetMembers();

      //Get dynamic properties from base object

      //The speckle object could have its own override of the material properties
      var speckleMat = GetMat(speckleConcrete.GetDynamicValue<Base>("Mat", dynamicMembers));
      if (speckleMat != null)
      {
        gsaConcrete.Mat = speckleMat;
      }
      gsaConcrete.Type = speckleConcrete.GetDynamicEnum<MatConcreteType>("Type", dynamicMembers);
      gsaConcrete.Cement = speckleConcrete.GetDynamicEnum<MatConcreteCement>("Cement", dynamicMembers);
      gsaConcrete.XdMin = speckleConcrete.GetDynamicValue<double>("XdMin", dynamicMembers);
      gsaConcrete.XdMax = speckleConcrete.GetDynamicValue<double>("XdMax", dynamicMembers);
      var Fcd = speckleConcrete.GetDynamicValue<double?>("Fcd", dynamicMembers);
      var Fcdc = speckleConcrete.GetDynamicValue<double?>("Fcdc", dynamicMembers);
      var Fcfib = speckleConcrete.GetDynamicValue<double?>("Fcfib", dynamicMembers);
      var EmEs = speckleConcrete.GetDynamicValue<double?>("EmEs", dynamicMembers);
      var N = speckleConcrete.GetDynamicValue<double?>("N", dynamicMembers);
      var Emod = speckleConcrete.GetDynamicValue<double?>("Emod", dynamicMembers);
      var Eps = speckleConcrete.GetDynamicValue<double?>("Eps", dynamicMembers);
      var EpsPeak = speckleConcrete.GetDynamicValue<double?>("EpsPeak", dynamicMembers);
      var EpsMax = speckleConcrete.GetDynamicValue<double?>("EpsMax", dynamicMembers);
      var EpsAx = speckleConcrete.GetDynamicValue<double?>("EpsAx", dynamicMembers);
      var EpsTran = speckleConcrete.GetDynamicValue<double?>("EpsTran", dynamicMembers);
      var EpsAxs = speckleConcrete.GetDynamicValue<double?>("EpsAxs", dynamicMembers);
      var Beta = speckleConcrete.GetDynamicValue<double?>("Beta", dynamicMembers);
      var Shrink = speckleConcrete.GetDynamicValue<double?>("Shrink", dynamicMembers);
      var Confine = speckleConcrete.GetDynamicValue<double?>("Confine", dynamicMembers);
      var Fcc = speckleConcrete.GetDynamicValue<double?>("Fcc", dynamicMembers);
      var EpsPlasC = speckleConcrete.GetDynamicValue<double?>("EpsPlasC", dynamicMembers);
      var EpsUC = speckleConcrete.GetDynamicValue<double?>("EpsUC", dynamicMembers);

      //test if positive and convert units
      if (Fcd.HasValue && Fcd > 0) gsaConcrete.Fcd = Fcd * conversionFactors.stress;
      if (Fcdc.HasValue && Fcdc > 0) gsaConcrete.Fcdc = Fcdc * conversionFactors.stress;
      if (Fcfib.HasValue && Fcfib > 0) gsaConcrete.Fcfib = Fcfib * conversionFactors.stress;
      if (EmEs.HasValue && EmEs > 0) gsaConcrete.EmEs = EmEs * conversionFactors.stress;
      if (N.HasValue && N > 0) gsaConcrete.N = N;
      if (Emod.HasValue && Emod > 0) gsaConcrete.Emod = Emod * conversionFactors.stress;
      if (Eps.HasValue && Eps > 0) gsaConcrete.Eps = Eps * conversionFactors.strain;
      if (EpsPeak.HasValue && EpsPeak > 0) gsaConcrete.EpsPeak = EpsPeak * conversionFactors.strain;
      if (EpsMax.HasValue && EpsMax > 0) gsaConcrete.EpsMax = EpsMax * conversionFactors.strain;
      if (EpsAx.HasValue && EpsAx > 0) gsaConcrete.EpsAx = EpsAx * conversionFactors.strain;
      if (EpsTran.HasValue && EpsTran > 0) gsaConcrete.EpsTran = EpsTran * conversionFactors.strain;
      if (EpsAxs.HasValue && EpsAxs > 0) gsaConcrete.EpsAxs = EpsAxs * conversionFactors.strain;
      if (Beta.HasValue && Beta > 0) gsaConcrete.Beta = Beta;
      if (Shrink.HasValue && Shrink > 0) gsaConcrete.Shrink = Shrink * conversionFactors.strain;
      if (Confine.HasValue && Confine > 0) gsaConcrete.Confine = Confine * conversionFactors.stress;
      if (Fcc.HasValue && Fcc > 0) gsaConcrete.Fcc = Fcc * conversionFactors.stress;
      if (EpsPlasC.HasValue && EpsPlasC > 0) gsaConcrete.EpsPlasC = EpsPlasC * conversionFactors.strain;
      if (EpsUC.HasValue && EpsUC > 0) gsaConcrete.EpsUC = EpsUC * conversionFactors.strain;

      return gsaRecords;
    }

    private List<GsaRecord> ConcreteToNative(Base speckleObject)
    {
      //Values based on GSA10.1 with design code AS3600-2018
      var speckleConcrete = (Concrete)speckleObject;
      double? e = null, fc = null, ft = null, nu = null, g = null, rho = null, alpha = null,
        damp = null, eps = null, epsT = null, epsC = null, cost = null, agg = null, beta = null;
      var strainFactor = StrainUnits.GetConversionFactor(StrainUnits.Strain, conversionFactors.nativeModelUnits.strain);

      if (speckleConcrete.compressiveStrength > 0)
      {
        fc = speckleConcrete.compressiveStrength * conversionFactors.stress;
        beta = GetBeta(speckleConcrete.compressiveStrength);
        eps = GetEpsMax(speckleConcrete.compressiveStrength) * strainFactor;
      }
      if (speckleConcrete.elasticModulus > 0) e = speckleConcrete.elasticModulus * conversionFactors.stress;
      if (speckleConcrete.tensileStrength > 0) ft = speckleConcrete.tensileStrength * conversionFactors.stress;
      if (speckleConcrete.poissonsRatio > 0) nu = speckleConcrete.poissonsRatio;
      if (speckleConcrete.shearModulus > 0) g = speckleConcrete.shearModulus * conversionFactors.stress;
      if (speckleConcrete.density > 0) rho = speckleConcrete.density * conversionFactors.DensityFactorToNative();
      if (speckleConcrete.thermalExpansivity > 0) alpha = speckleConcrete.thermalExpansivity * conversionFactors.ThermalExapansionFactorToNative();
      if (speckleConcrete.dampingRatio > 0) damp = speckleConcrete.dampingRatio;
      if (speckleConcrete.maxCompressiveStrain > 0) epsC = speckleConcrete.maxCompressiveStrain * conversionFactors.strain;
      if (speckleConcrete.maxTensileStrain > 0) epsT = speckleConcrete.maxTensileStrain * conversionFactors.strain;
      if (speckleConcrete.cost > 0) cost = speckleConcrete.cost;
      if (speckleConcrete.maxAggregateSize > 0) agg = speckleConcrete.maxAggregateSize * conversionFactors.length;

      var gsaConcrete = new GsaMatConcrete()
      {
        ApplicationId = speckleConcrete.applicationId,
        Index = speckleConcrete.GetIndex<GsaMatConcrete>(),
        Name = speckleConcrete.name,
        Mat = new GsaMat()
        {
          Name = speckleConcrete.name,
          E = e,
          F = fc,
          Nu = nu,
          G = g,
          Rho = rho,
          Alpha = alpha,
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = e,
            Nu = nu,
            Rho = rho,
            Alpha = alpha,
            G = g,
            Damp = damp,
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = null,
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = null,
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = null,
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = null,
          Eps = null,
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.RECTANGLE, MatCurveParamType.NO_TENSION },
            StrainElasticCompression = eps,
            StrainElasticTension = null,
            StrainPlasticCompression = eps,
            StrainPlasticTension = null,
            StrainFailureCompression = 0.003 * strainFactor,
            StrainFailureTension = 1,
            GammaF = 1,
            GammaE = 1,
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.LINEAR, MatCurveParamType.INTERPOLATED },
            StrainElasticCompression = 0.003 * strainFactor,
            StrainElasticTension = null,
            StrainPlasticCompression = 0.003 * strainFactor,
            StrainPlasticTension = null,
            StrainFailureCompression = 0.003 * strainFactor,
            StrainFailureTension = epsT,
            GammaF = 1,
            GammaE = 1,
          },
          Cost = cost,
          Type = MatType.CONCRETE,
        },
        Type = MatConcreteType.CYLINDER, //strength type
        Cement = MatConcreteCement.N, //cement class
        Fc = fc, //concrete strength
        Fcd = 0.85 * fc, //design strength
        Fcdc = 0.4 * fc, //cracked strength
        Fcdt = ft, //tensile strength
        Fcfib = 0.6 * ft, //peak strength for FIB/Popovics curves
        EmEs = null, //ratio of initial elastic modulus to secant modulus
        N = 2, //parabolic coefficient (normally 2)
        Emod = 1, //modifier on elastic stiffness typically in range (0.8:1.2)
        EpsPeak = 0.003 * strainFactor, //concrete strain at peak SLS stress
        EpsMax = eps, //maximum conrete SLS strain
        EpsU = epsC, //concrete ULS failure strain
        EpsAx = 0.0025 * strainFactor, //concrete max compressive ULS strain
        EpsTran = 0.002 * strainFactor, //slab transition strain
        EpsAxs = 0.0025 * strainFactor, //slab axial strain limit
        Light = speckleConcrete.lightweight, //lightweight flag
        Agg = agg, //maximum aggregate size
        XdMin = 0, //minimum x/d in flexure
        XdMax = 1, //maximum x/d in flexure
        Beta = beta, //depth of rectangular stress block
        Shrink = null, //shrinkage strain
        Confine = null, //confining stress
        Fcc = null, //concrete strength [confined]
        EpsPlasC = null, //plastic strain (ULS) [confined]
        EpsUC = null, //concrete failure strain [confined]
      };
      //TODO:
      //SpeckleObject:
      //  string grade
      //  string designCode
      //  string codeYear
      //  double strength
      //  double materialSafetyFactor
      //  double flexuralStrength
      return new List<GsaRecord>() { gsaConcrete };
    }

    private List<GsaRecord> GSAAnalysisMaterialToNative(Base speckleObject)
    {
      var speckleMatAnal = (GSAMaterial)speckleObject;

      var gsaMatAnal = new GsaMatAnal()
      {
        ApplicationId = speckleMatAnal.applicationId,
        Index = speckleMatAnal.GetIndex<GsaMatAnal>(),
        Name = speckleMatAnal.name,
        Colour = speckleMatAnal.colour?.ColourToNative() ?? Colour.NotSet,
      };

      double? e = null, nu = null, g = null, rho = null, alpha = null, damp = null;
      if (speckleMatAnal.strength > 0) e = speckleMatAnal.elasticModulus * conversionFactors.stress;
      if (speckleMatAnal.elasticModulus > 0) e = speckleMatAnal.elasticModulus * conversionFactors.stress;
      if (speckleMatAnal.poissonsRatio > 0) nu = speckleMatAnal.poissonsRatio;
      if (speckleMatAnal.shearModulus > 0) g = speckleMatAnal.shearModulus * conversionFactors.stress;
      if (speckleMatAnal.density > 0) rho = speckleMatAnal.density * conversionFactors.DensityFactorToNative();
      if (speckleMatAnal.thermalExpansivity > 0) alpha = speckleMatAnal.thermalExpansivity * conversionFactors.ThermalExapansionFactorToNative();
      if (speckleMatAnal.dampingRatio > 0) damp = speckleMatAnal.dampingRatio;
      gsaMatAnal.E = e;
      gsaMatAnal.Nu = nu;
      gsaMatAnal.Rho = rho;
      gsaMatAnal.Alpha = alpha;
      gsaMatAnal.G = g;
      gsaMatAnal.Damp = damp;

      var dynamicMembers = speckleMatAnal.GetMembers();
      gsaMatAnal.NumParams = speckleMatAnal.GetDynamicValue<int>("NumParams", dynamicMembers);

      var type = speckleMatAnal.GetDynamicEnum<MatAnalType>("Type", dynamicMembers);
      gsaMatAnal.Type = type;
      switch (type)
      {
        case MatAnalType.MAT_ELAS_ISO:
          break;
        case MatAnalType.MAT_ELAS_PLAS_ISO:
          double? yield = speckleMatAnal.GetDynamicValue<double?>("Yield", dynamicMembers) * conversionFactors.stress;
          double? ultimate = speckleMatAnal.GetDynamicValue<double?>("Ultimate", dynamicMembers) * conversionFactors.stress;
          double? eh = speckleMatAnal.GetDynamicValue<double?>("Eh", dynamicMembers) * conversionFactors.stress;
          gsaMatAnal.Yield = yield;
          gsaMatAnal.Ultimate = ultimate;
          gsaMatAnal.Eh = eh;
          gsaMatAnal.Beta = speckleMatAnal.GetDynamicValue<double?>("Beta", dynamicMembers);
          break;
        case MatAnalType.MAT_DRUCKER_PRAGER:
          double? cohesion = speckleMatAnal.GetDynamicValue<double?>("Cohesion", dynamicMembers) * conversionFactors.stress;
          gsaMatAnal.Cohesion = cohesion;
          gsaMatAnal.Phi = speckleMatAnal.GetDynamicValue<double?>("Phi", dynamicMembers);
          gsaMatAnal.Psi = speckleMatAnal.GetDynamicValue<double?>("Psi", dynamicMembers);
          gsaMatAnal.Scribe = speckleMatAnal.GetDynamicValue<double?>("Scribe", dynamicMembers);
          break;
        case MatAnalType.MAT_ELAS_ORTHO:
          double? ex = speckleMatAnal.GetDynamicValue<double?>("Ex", dynamicMembers) * conversionFactors.stress;
          double? ey = speckleMatAnal.GetDynamicValue<double?>("Ey", dynamicMembers) * conversionFactors.stress;
          double? ez = speckleMatAnal.GetDynamicValue<double?>("Ez", dynamicMembers) * conversionFactors.stress;
          double? nuxy = speckleMatAnal.GetDynamicValue<double?>("Nuxy", dynamicMembers);
          double? nuyz = speckleMatAnal.GetDynamicValue<double?>("Nuyz", dynamicMembers);
          double? nuzx = speckleMatAnal.GetDynamicValue<double?>("Nuzx", dynamicMembers);
          double? alphax = speckleMatAnal.GetDynamicValue<double?>("Alphax", dynamicMembers) * conversionFactors.ThermalExapansionFactorToNative();
          double? alphay = speckleMatAnal.GetDynamicValue<double?>("Alphay", dynamicMembers) * conversionFactors.ThermalExapansionFactorToNative();
          double? alphaz = speckleMatAnal.GetDynamicValue<double?>("Alphaz", dynamicMembers) * conversionFactors.ThermalExapansionFactorToNative();
          double? gxy = speckleMatAnal.GetDynamicValue<double?>("Gxy", dynamicMembers) * conversionFactors.stress;
          double? gyz = speckleMatAnal.GetDynamicValue<double?>("Gyz", dynamicMembers) * conversionFactors.stress;
          double? gzx = speckleMatAnal.GetDynamicValue<double?>("Gzx", dynamicMembers) * conversionFactors.stress;
          gsaMatAnal.Ex = ex;
          gsaMatAnal.Ey = ey;
          gsaMatAnal.Ez = ez;
          gsaMatAnal.Nuxy = nuxy;
          gsaMatAnal.Nuyz = nuyz;
          gsaMatAnal.Nuzx = nuzx;
          gsaMatAnal.Alphax = alphax;
          gsaMatAnal.Alphay = alphay;
          gsaMatAnal.Alphaz = alphaz;
          gsaMatAnal.Gxy = gxy;
          gsaMatAnal.Gyz = gyz;
          gsaMatAnal.Gzx = gzx;
          break;
        case MatAnalType.MAT_FABRIC:
          ex = speckleMatAnal.GetDynamicValue<double?>("Ex", dynamicMembers) * (conversionFactors.force / conversionFactors.length);
          ey = speckleMatAnal.GetDynamicValue<double?>("Ey", dynamicMembers) * (conversionFactors.force / conversionFactors.length);
          g = speckleMatAnal.shearModulus * (conversionFactors.force / conversionFactors.length);
          gsaMatAnal.Ex = ex;
          gsaMatAnal.Ey = ey;
          gsaMatAnal.G = g;
          gsaMatAnal.Comp = speckleMatAnal.GetDynamicValue<double?>("Comp", dynamicMembers);
          break;
        default:
          Report.ConversionErrors.Add(new Exception("GSAMaterial: material model type (" + type.ToString() + ") is not currently supported"));
          break;
      }

      return new List<GsaRecord>() { gsaMatAnal };
    }

    #endregion

    #region Properties
    private List<GsaRecord> GsaProperty1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (GSAProperty1D)speckleObject;
      var natives = Property1dToNative(speckleObject);
      retList.AddRange(natives);
      var gsaSection = (GsaSection)natives.FirstOrDefault(n => n is GsaSection);
      if (gsaSection != null)
      {
        gsaSection.Type = speckleProperty.memberType.ToNativeSection();
        gsaSection.Colour = (Enum.TryParse(speckleProperty.colour, true, out Colour gsaColour) ? gsaColour : Colour.NO_RGB);
        gsaSection.Mass = (speckleProperty.additionalMass > 0) ? (double?)(speckleProperty.additionalMass * conversionFactors.mass / conversionFactors.length) : null;
        gsaSection.Cost = (speckleProperty.cost == 0) ? null : (double?)(speckleProperty.cost / conversionFactors.mass); //units: e.g. $/kg
        if (speckleProperty.designMaterial != null && gsaSection.Components != null && gsaSection.Components.Count > 0)
        {
          var sectionComp = (SectionComp)gsaSection.Components.First();
          sectionComp.MaterialType = speckleProperty.designMaterial.materialType.ToNative();
          if (speckleProperty.designMaterial.materialType == MaterialType.Steel && speckleProperty.designMaterial != null)
          {
            sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.designMaterial, ref retList);

            var gsaSectionSteel = new SectionSteel()
            {
              //GradeIndex = 0,
              //Defaults
              PlasElas = 1,
              NetGross = 1,
              Exposed = 1,
              Beta = 0.4,
              Type = SectionSteelSectionType.Undefined,
              Plate = SectionSteelPlateType.Undefined,
              Locked = false
            };
            gsaSection.Components.Add(gsaSectionSteel);
          }
          else if (speckleProperty.designMaterial.materialType == MaterialType.Concrete && speckleProperty.designMaterial != null)
          {
            sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.designMaterial, ref retList);

            var gsaSectionConc = new SectionConc();
            var gsaSectionLink = new SectionLink();
            var gsaSectionCover = new SectionCover();
            var gsaSectionTmpl = new SectionTmpl();
            gsaSection.Components.Add(gsaSectionConc);
            gsaSection.Components.Add(gsaSectionLink);
            gsaSection.Components.Add(gsaSectionCover);
            gsaSection.Components.Add(gsaSectionTmpl);
          }
          else
          {
            if (speckleProperty.material != null)
            {
              sectionComp.MatAnalIndex = IndexByConversionOrLookup<GsaMatAnal>(speckleProperty.material, ref retList);
            }
            //Not supported yet
          }
        }
      }
      return retList;
    }

    //Note: there should be no ToNative for SectionProfile because it's not a type that will create a first-class citizen in the GSA model
    //      so there is basically a ToNative of that class here in this method too
    private List<GsaRecord> Property1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (Property1D)speckleObject;

      //unit conversion for offsets - section offsets are always specified in mm in GSA!
      double offsetFactor = 1;
      if (speckleProperty.profile != null && !string.IsNullOrEmpty(speckleProperty.profile.units))
      {
        offsetFactor = Units.GetConversionFactor(speckleProperty.profile.units, Units.Millimeters);
      }
      else if (conversionFactors.speckleModelUnits != null && !string.IsNullOrEmpty(conversionFactors.speckleModelUnits.displacements))
      {
        offsetFactor = Units.GetConversionFactor(conversionFactors.speckleModelUnits.displacements, Units.Millimeters);
      }

      var gsaSection = new GsaSection()
      {
        Index = speckleProperty.GetIndex<GsaSection>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        //PoolIndex = 0,
        ReferencePoint = speckleProperty.referencePoint.ToNative(),
        RefY = (speckleProperty.offsetY == 0) ? null : (double?)speckleProperty.offsetY * offsetFactor,
        RefZ = (speckleProperty.offsetZ == 0) ? null : (double?)speckleProperty.offsetZ * offsetFactor,
        Fraction = 1,
        //Left = 0,
        //Right = 0,
        //Slab = 0,
        Components = new List<GsaSectionComponentBase>()
      };

      var sectionComp = new SectionComp()
      {
        Name = (speckleProperty.profile == null || string.IsNullOrEmpty(speckleProperty.profile.name)) ? null : speckleProperty.profile.name
      };
      if (speckleProperty.material != null)
      {
        if (speckleProperty.material.materialType == MaterialType.Steel && speckleProperty.material != null)
        {
          sectionComp.MaterialType = Section1dMaterialType.STEEL;
          sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.material, ref retList);
          //var steelMaterial = (Steel)speckleProperty.material;
          var gsaSectionSteel = new SectionSteel()
          {
            //GradeIndex = 0,
            //Defaults
            PlasElas = 1,
            NetGross = 1,
            Exposed = 1,
            Beta = 0.4,
            Type = SectionSteelSectionType.Undefined,
            Plate = SectionSteelPlateType.Undefined,
            Locked = false
          };
          ;
          gsaSection.Components.Add(sectionComp);
          gsaSection.Components.Add(gsaSectionSteel);
        }
        else if (speckleProperty.material.materialType == MaterialType.Concrete && speckleProperty.material != null)
        {
          sectionComp.MaterialType = Section1dMaterialType.CONCRETE;
          sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.material, ref retList);
          gsaSection.Components.Add(sectionComp);

          var gsaSectionConc = new SectionConc();
          var gsaSectionLink = new SectionLink();
          var gsaSectionCover = new SectionCover();
          var gsaSectionTmpl = new SectionTmpl();
          gsaSection.Components.Add(gsaSectionConc);
          gsaSection.Components.Add(gsaSectionLink);
          gsaSection.Components.Add(gsaSectionCover);
          gsaSection.Components.Add(gsaSectionTmpl);
        }
        else
        {
          sectionComp.MaterialType = Section1dMaterialType.GENERIC;
          sectionComp.MatAnalIndex = IndexByConversionOrLookup<GsaMatAnal>(speckleProperty.material, ref retList);
          gsaSection.Components.Add(sectionComp);

          //Not supported yet
        }
      }
      else
      {
        gsaSection.Components.Add(sectionComp);
      }

      if (speckleProperty.profile != null)
      {
        // Attempt to map property1d to catalogue based on software mappingDB
        var profileName = speckleProperty.profile != null ? speckleProperty.profile.name : speckleProperty.name.Split(':')[1];
        var mappings = UseMappings ? GetMappingFromProfileName(profileName) : null;

        // If successfully mapped, apply catalogue profile
        if (mappings != null)
        {
          var catalogueProfile = new Catalogue()
          {
            description = $"CAT {mappings["profileType"]} {mappings["familyType"]}"
          };

          catalogueProfile.shapeType = ShapeType.Catalogue;

          speckleProperty.profile = catalogueProfile;
        }

        Property1dProfileToNative(speckleProperty.profile, out sectionComp.ProfileDetails, out sectionComp.ProfileGroup);
      }

      retList.Add(gsaSection);
      return retList;
    }

    private bool CurveToGsaOutline(Polyline outline, ref List<double?> Y, ref List<double?> Z, ref List<string> actions)
    {
      if (!(outline is Polyline))
      {
        return false;
      }
      var pointCoords = ((Polyline)outline).value.GroupBy(3).Select(g => g.ToList()).ToList();
      foreach (var coords in pointCoords)
      {
        Y.Add(coords[1]);
        Z.Add(coords[2]);
      }
      actions.Add("M");
      actions.AddRange(Enumerable.Repeat("L", (pointCoords.Count() - 1)));
      return true;
    }

    private bool Property1dProfileToNative(SectionProfile sectionProfile, out ProfileDetails gsaProfileDetails, out Section1dProfileGroup gsaProfileGroup)
    {
      var lengthFactor = string.IsNullOrEmpty(sectionProfile.units) ? conversionFactors.displacements : 1;

      if (sectionProfile.shapeType == ShapeType.Catalogue)
      {
        var p = (Catalogue)sectionProfile;
        gsaProfileDetails = new ProfileDetailsCatalogue()
        {
          Profile = p.description
        };
        gsaProfileGroup = Section1dProfileGroup.Catalogue;
      }
      else if (sectionProfile.shapeType == ShapeType.Explicit)
      {
        var p = (Explicit)sectionProfile;
        gsaProfileDetails = new ProfileDetailsExplicit()
        {
          Area = p.area * Math.Pow(lengthFactor, 2),
          Iyy = p.Iyy * Math.Pow(lengthFactor, 4),
          Izz = p.Izz * Math.Pow(lengthFactor, 4),
          J = p.J * Math.Pow(lengthFactor, 4),
          Ky = p.Ky,
          Kz = p.Kz
        };
        gsaProfileGroup = Section1dProfileGroup.Explicit;
      }
      else if (sectionProfile.shapeType == ShapeType.Perimeter)
      {
        var p = (Perimeter)sectionProfile;
        var hollow = (p.voids != null && p.voids.Count > 0);
        List<string> actions = null;
        List<double?> y = null, z = null;

        if (p.outline is Polyline && (p.voids == null || (p.voids.All(v => v is Polyline))))
        {
          actions = new List<string>();
          y = new List<double?>();
          z = new List<double?>();

          CurveToGsaOutline(p.outline, ref y, ref z, ref actions);

          if (hollow)
          {
            foreach (var v in p.voids)
            {
              CurveToGsaOutline(v, ref y, ref z, ref actions);
            }
          }
        }
        gsaProfileGroup = Section1dProfileGroup.Perimeter;
        gsaProfileDetails = new ProfileDetailsPerimeter()
        {
          Type = "P",
          Actions = actions,
          Y = y.Select(v => v * lengthFactor).ToList(),
          Z = z.Select(v => v * lengthFactor).ToList(),
        };
      }
      else
      {
        gsaProfileGroup = Section1dProfileGroup.Standard;
        if (sectionProfile.shapeType == ShapeType.Rectangular)
        {
          var p = (Rectangular)sectionProfile;
          var hollow = (p.flangeThickness > 0 || p.webThickness > 0);
          if (hollow)
          {
            gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.RectangularHollow };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor, p.webThickness * lengthFactor, p.flangeThickness * lengthFactor);
          }
          else
          {
            gsaProfileDetails = new ProfileDetailsRectangular() { ProfileType = Section1dStandardProfileType.Rectangular };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor);
          }
        }
        else if (sectionProfile.shapeType == ShapeType.Circular)
        {
          var p = (Circular)sectionProfile;
          var hollow = (p.wallThickness > 0);
          if (hollow)
          {
            gsaProfileDetails = new ProfileDetailsCircularHollow() { ProfileType = Section1dStandardProfileType.CircularHollow };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.radius * 2 * lengthFactor, p.wallThickness * lengthFactor);
          }
          else
          {
            gsaProfileDetails = new ProfileDetailsCircular() { ProfileType = Section1dStandardProfileType.Circular };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.radius * 2 * lengthFactor);
          }
        }
        else if (sectionProfile.shapeType == ShapeType.Angle)
        {
          var p = (Angle)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Angle };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor, p.webThickness * lengthFactor, p.flangeThickness * lengthFactor);
        }
        else if (sectionProfile.shapeType == ShapeType.Channel)
        {
          var p = (Channel)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Channel };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor, p.webThickness * lengthFactor, p.flangeThickness * lengthFactor);
        }
        else if (sectionProfile.shapeType == ShapeType.I)
        {
          var p = sectionProfile as ISection;
          if (p != null)
          {
            gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.ISection };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor, p.webThickness * lengthFactor, p.flangeThickness * lengthFactor);
          }
          else
          {
            gsaProfileDetails = new ProfileDetailsGeneralI() { ProfileType = Section1dStandardProfileType.GeneralI };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(sectionProfile["depth"].ToDouble() * lengthFactor, sectionProfile["topFlangeWidth"].ToDouble() * lengthFactor, sectionProfile["botFlangeWidth"].ToDouble() * lengthFactor,
              sectionProfile["topFlangeThickness"].ToDouble() * lengthFactor, sectionProfile["botFlangeThickness"].ToDouble() * lengthFactor, sectionProfile["webThickness"].ToDouble() * lengthFactor);
          }
        }
        else if (sectionProfile.shapeType == ShapeType.Tee)
        {
          var p = (Tee)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Tee };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth * lengthFactor, p.width * lengthFactor, p.webThickness * lengthFactor, p.flangeThickness * lengthFactor);
        }
        else
        {
          gsaProfileDetails = null;
        }
      }
      if (gsaProfileDetails != null && sectionProfile.units != null)
      {
        gsaProfileDetails.Units = sectionProfile.units;
      }

      return true;
    }

    private List<GsaRecord> GsaProperty2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (GSAProperty2D)speckleObject;
      var natives = Property2dToNative(speckleObject);
      retList.AddRange(natives);
      var gsaProp2d = (GsaProp2d)natives.FirstOrDefault(n => n is GsaProp2d);
      if (gsaProp2d != null)
      {
        if (speckleProperty.type == PropertyType2D.Load)
        {
          gsaProp2d.Support = speckleProperty.support.ToNative();
          gsaProp2d.Edge = speckleProperty.referenceEdge;
          return retList;
        }
        //unit conversion scale factors (GSA units are messed up!)
        var thicknessFactor = conversionFactors.displacements;
        var inPlaneFactor = Math.Pow(conversionFactors.displacements, 2) / conversionFactors.length;
        var bendingFactor = Math.Pow(conversionFactors.sections, 4) / conversionFactors.length;
        var shearFactor = Math.Pow(conversionFactors.displacements, 2) / conversionFactors.length;
        var volumeFactor = Math.Pow(conversionFactors.sections, 3) / Math.Pow(conversionFactors.length, 2);

        gsaProp2d.InPlane = speckleProperty.modifierInPlane > 0 ? (double?)speckleProperty.modifierInPlane * inPlaneFactor : null;
        gsaProp2d.Bending = speckleProperty.modifierBending > 0 ? (double?)speckleProperty.modifierBending * bendingFactor : null;
        gsaProp2d.Shear = speckleProperty.modifierShear > 0 ? (double?)speckleProperty.modifierShear * shearFactor : null;
        gsaProp2d.Volume = speckleProperty.modifierVolume > 0 ? (double?)speckleProperty.modifierVolume * volumeFactor : null;
        gsaProp2d.InPlaneStiffnessPercentage = speckleProperty.modifierInPlane < 0 ? -(double?)speckleProperty.modifierInPlane * 100 : null;
        gsaProp2d.BendingStiffnessPercentage = speckleProperty.modifierBending < 0 ? -(double?)speckleProperty.modifierBending * 100 : null;
        gsaProp2d.ShearStiffnessPercentage = speckleProperty.modifierShear < 0 ? -(double?)speckleProperty.modifierShear * 100 : null;
        gsaProp2d.VolumePercentage = speckleProperty.modifierVolume < 0 ? -(double?)speckleProperty.modifierVolume * 100 : null;

        gsaProp2d.Colour = (Enum.TryParse(speckleProperty.colour, true, out Colour gsaColour) ? gsaColour : Colour.NO_RGB);
        gsaProp2d.Mass = speckleProperty.additionalMass * conversionFactors.mass / Math.Pow(conversionFactors.length, 2);
        gsaProp2d.Profile = speckleProperty.concreteSlabProp;
        if (speckleProperty.designMaterial != null)
        {
          if (speckleProperty.designMaterial.materialType == MaterialType.Steel && (speckleProperty.designMaterial is GSASteel || speckleProperty.designMaterial is Steel))
          {
            gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.designMaterial, ref retList);
            gsaProp2d.MatType = Property2dMaterialType.Steel;
          }
          else if (speckleProperty.designMaterial.materialType == MaterialType.Concrete && (speckleProperty.designMaterial is GSAConcrete || speckleProperty.designMaterial is Concrete))
          {
            gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.designMaterial, ref retList);
            gsaProp2d.MatType = Property2dMaterialType.Concrete;
          }
          else
          {
            //Not supported yet
            gsaProp2d.MatType = Property2dMaterialType.Generic;
          }
        }
        else if (speckleProperty.material != null)
        {
          if (speckleProperty.material.materialType == MaterialType.Steel && (speckleProperty.material is GSASteel || speckleProperty.designMaterial is Steel))
          {
            gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.material, ref retList);
            gsaProp2d.MatType = Property2dMaterialType.Steel;
          }
          else if (speckleProperty.material.materialType == MaterialType.Concrete && (speckleProperty.material is GSAConcrete || speckleProperty.material is Concrete))
          {
            gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.material, ref retList);
            gsaProp2d.MatType = Property2dMaterialType.Concrete;
          }
          else
          {
            //Not supported yet
            gsaProp2d.MatType = Property2dMaterialType.Generic;
          }
        }
      }
      return retList;
    }

    //Note: there should be no ToNative for SectionProfile because it's not a type that will create a first-class citizen in the GSA model
    //      so there is basically a ToNative of that class here in this method too
    private List<GsaRecord> Property2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (Property2D)speckleObject;

      //unit conversion scale factors (GSA units are messed up!)
      var thicknessFactor = conversionFactors.displacements;

      var gsaProp2d = new GsaProp2d()
      {
        Index = speckleProperty.GetIndex<GsaProp2d>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        Thickness = (speckleProperty.thickness > 0) ? (double?)speckleProperty.thickness * thicknessFactor : null,
        RefZ = speckleProperty.zOffset * thicknessFactor,
        RefPt = speckleProperty.refSurface.ToNative(),
        Type = speckleProperty.type.ToNative(),
      };

      if (!string.IsNullOrEmpty(speckleProperty.units))
      {
        gsaProp2d.Units = speckleProperty.units;
      }
      if (speckleProperty.material != null)
      {
        if (speckleProperty.material.materialType == MaterialType.Concrete)
        {
          gsaProp2d.MatType = Property2dMaterialType.Concrete;
          gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.material, ref retList);
        }
        else if (speckleProperty.material.materialType == MaterialType.Steel)
        {
          gsaProp2d.MatType = Property2dMaterialType.Steel;
          gsaProp2d.GradeIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.material, ref retList);
        }
      }

      if (speckleProperty.orientationAxis == null || speckleProperty.orientationAxis.definition == null || speckleProperty.orientationAxis.definition.IsGlobal())
      {
        gsaProp2d.AxisRefType = AxisRefType.Global;
      }
      else
      {
        var axisIndex = IndexByConversionOrLookup<GsaAxis>(speckleProperty.orientationAxis, ref retList);
        if (axisIndex.IsIndex())
        {
          gsaProp2d.AxisIndex = axisIndex;
          gsaProp2d.AxisRefType = AxisRefType.Reference;
        }
        else
        {
          gsaProp2d.AxisRefType = AxisRefType.Local;
        }
      }
      retList.Add(gsaProp2d);
      return retList;
    }

    private List<GsaRecord> PropertyMassToNative(Base speckleObject)
    {
      var specklePropertyMass = (PropertyMass)speckleObject;
      var inertiaFactor = conversionFactors.mass * Math.Pow(conversionFactors.length, 2);
      var gsaPropMass = new GsaPropMass()
      {
        Index = specklePropertyMass.GetIndex<GsaPropMass>(),
        Name = specklePropertyMass.name,
        ApplicationId = specklePropertyMass.applicationId,
        Mass = specklePropertyMass.mass * conversionFactors.mass,
        Ixx = specklePropertyMass.inertiaXX * inertiaFactor,
        Iyy = specklePropertyMass.inertiaYY * inertiaFactor,
        Izz = specklePropertyMass.inertiaZZ * inertiaFactor,
        Ixy = specklePropertyMass.inertiaXY * inertiaFactor,
        Iyz = specklePropertyMass.inertiaYZ * inertiaFactor,
        Izx = specklePropertyMass.inertiaZX * inertiaFactor
      };
      gsaPropMass.Mod = (specklePropertyMass.massModified) ? MassModification.Modified : MassModification.Defined;
      if (specklePropertyMass.massModified)
      {
        gsaPropMass.ModX = MassModifierUnitConversion(specklePropertyMass.massModifierX);
        gsaPropMass.ModY = MassModifierUnitConversion(specklePropertyMass.massModifierY);
        gsaPropMass.ModZ = MassModifierUnitConversion(specklePropertyMass.massModifierZ);
      }

      return new List<GsaRecord>() { gsaPropMass };
    }

    private List<GsaRecord> GSAPropertyLinkToNative(Base speckleObject)
    {
      var specklePropertyLink = (GSAPropertyLink)speckleObject;
      var gsaPropLink = new GsaPropLink()
      {
        Index = specklePropertyLink.GetIndex<GsaPropLink>(),
        Name = specklePropertyLink.name,
        ApplicationId = specklePropertyLink.applicationId,
        Colour = specklePropertyLink.colour.ColourToNative(),
        Type = specklePropertyLink.type.ToNative(),
        LinkedCondition = GetRigidConstraint(specklePropertyLink.constraintCondition)
      };

      return new List<GsaRecord>() { gsaPropLink };
    }

    private List<GsaRecord> PropertySpringToNative(Base speckleObject)
    {
      var fns = new Dictionary<PropertyTypeSpring, Func<PropertySpring, GsaPropSpr, bool>>
      { { PropertyTypeSpring.Axial, SetPropertySpringAxial },
        { PropertyTypeSpring.Torsional, SetPropertySpringTorsional },
        { PropertyTypeSpring.CompressionOnly, SetPropertySpringCompression },
        { PropertyTypeSpring.TensionOnly, SetPropertySpringTension },
        { PropertyTypeSpring.LockUp, SetPropertySpringLockup },
        { PropertyTypeSpring.Gap, SetPropertySpringGap },
        { PropertyTypeSpring.Friction, SetPropertySpringFriction },
        { PropertyTypeSpring.General, SetPropertySpringGeneral }
        //CONNECT not yet supported
        //MATRIX not yet supported
      };

      var specklePropertySpring = (PropertySpring)speckleObject;
      var gsaPropSpr = new GsaPropSpr()
      {
        Index = specklePropertySpring.GetIndex<GsaPropSpr>(),
        Name = specklePropertySpring.name,
        ApplicationId = specklePropertySpring.applicationId,
        DampingRatio = specklePropertySpring.dampingRatio
      };
      if (fns.ContainsKey(specklePropertySpring.springType))
      {
        gsaPropSpr.Stiffnesses = new Dictionary<GwaAxisDirection6, double>();
        fns[specklePropertySpring.springType](specklePropertySpring, gsaPropSpr);
      }
      else
      {
        Report.ConversionErrors.Add(new Exception("PropertySpring: spring type (" + specklePropertySpring.springType.ToString() + ") is not currently supported"));
      }

      return new List<GsaRecord>() { gsaPropSpr };
    }

    private bool SetPropertySpringAxial(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Axial;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringTorsional(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Torsional;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.XX, specklePropertySpring.stiffnessXX * conversionFactors.force * conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringCompression(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Compression;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringTension(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Tension;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringLockup(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      //Also for LOCKUP, there are positive and negative parameters, but these aren't supported yet
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Lockup;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringGap(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Gap;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      return true;
    }

    private bool SetPropertySpringFriction(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Friction;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Y, specklePropertySpring.stiffnessY * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Z, specklePropertySpring.stiffnessZ * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.FrictionCoeff = specklePropertySpring.frictionCoefficient;
      return true;
    }

    private bool SetPropertySpringGeneral(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.General;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Y, specklePropertySpring.stiffnessY * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Z, specklePropertySpring.stiffnessZ * conversionFactors.force / conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.XX, specklePropertySpring.stiffnessXX * conversionFactors.force * conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.YY, specklePropertySpring.stiffnessYY * conversionFactors.force * conversionFactors.length);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.ZZ, specklePropertySpring.stiffnessZZ * conversionFactors.force * conversionFactors.length);
      return true;
    }

    #endregion

    #region Constraints
    private List<GsaRecord> GSAGeneralisedRestraintToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleGenRest = (GSAGeneralisedRestraint)speckleObject;
      var gsaGenRest = new GsaGenRest()
      {
        ApplicationId = speckleGenRest.applicationId,
        Index = speckleGenRest.GetIndex<GsaGenRest>(),
        Name = speckleGenRest.name,
        StageIndices = IndexByConversionOrLookup<GsaAnalStage>(speckleGenRest.stages.Select(s => (Base)s).ToList(), ref gsaRecords),
      };
      if (speckleGenRest.nodeRefs != null && speckleGenRest.nodeRefs.Count() > 0)
      {
        var nodes = ContextObjects.Where(n => speckleGenRest.nodeRefs.Contains(n.applicationId)).Select(no => (Node)no).ToList();
        gsaGenRest.NodeIndices = nodes.NodeAt(conversionFactors);
      }
      else if (speckleGenRest.nodes != null && speckleGenRest.nodes.Count() > 0)
      {
        gsaGenRest.NodeIndices = speckleGenRest.nodes.NodeAt(conversionFactors);
      }
      if (speckleGenRest.restraint != null && speckleGenRest.restraint.code.Length >= 6)
      {
        gsaGenRest.X = speckleGenRest.restraint.code[0] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.Y = speckleGenRest.restraint.code[1] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.Z = speckleGenRest.restraint.code[2] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.XX = speckleGenRest.restraint.code[3] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.YY = speckleGenRest.restraint.code[4] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.ZZ = speckleGenRest.restraint.code[5] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
      }
      gsaRecords.Add(gsaGenRest);
      return gsaRecords;
    }

    private List<GsaRecord> GSARigidConstraintToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleRigid = (GSARigidConstraint)speckleObject;
      var gsaRigid = new GsaRigid()
      {
        ApplicationId = speckleRigid.applicationId,
        Index = speckleRigid.GetIndex<GsaRigid>(),
        Name = speckleRigid.name,
        Type = speckleRigid.type.ToNative(),
        Link = GetRigidConstraint(speckleRigid.constraintCondition),
      };
      if (speckleRigid.primaryNodeRef != null)
      {
        var node = ContextObjects.Where(n => n.applicationId == speckleRigid.primaryNodeRef).Select(no => (Node)no).FirstOrDefault();
        gsaRigid.PrimaryNode = node.NodeAt(conversionFactors);
      }
      else if (speckleRigid.primaryNode != null)
      {
        gsaRigid.PrimaryNode = speckleRigid.primaryNode.NodeAt(conversionFactors);
      }
      if (speckleRigid.constrainedNodeRefs != null && speckleRigid.constrainedNodeRefs.Count() > 0)
      {
        var nodes = speckleRigid.constrainedNodeRefs.Select(t => ContextObjects.Where(o => o.applicationId == t).FirstOrDefault() as Node).ToList();
        gsaRigid.ConstrainedNodes = nodes.NodeAt(conversionFactors);
      }
      else if (speckleRigid.constrainedNodes != null && speckleRigid.constrainedNodes.Count() > 0)
      {
        gsaRigid.ConstrainedNodes = speckleRigid.constrainedNodes.NodeAt(conversionFactors);
      }
      if (speckleRigid.stages != null) gsaRigid.Stage = IndexByConversionOrLookup<GsaAnalStage>(speckleRigid.stages.Select(s => (Base)s).ToList(), ref gsaRecords);
      if (speckleRigid.parentMember != null) gsaRigid.ParentMember = IndexByConversionOrLookup<GsaMemb>(speckleRigid.parentMember, ref gsaRecords);
      gsaRecords.Add(gsaRigid);
      return gsaRecords;
    }
    #endregion

    #region Bridge

    private List<GsaRecord> AlignToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleAlign = (GSAAlignment)speckleObject;
      var gsaAlign = new GsaAlign()
      {
        ApplicationId = speckleAlign.applicationId,
        Index = speckleAlign.GetIndex<GsaAlign>(),
        Chain = speckleAlign.chainage.Select(v => conversionFactors.length * v).ToList(),
        Curv = speckleAlign.curvature.Select(v => (1 / conversionFactors.length) * v).ToList(),
        Name = speckleAlign.name,
        Sid = speckleAlign.id,
        GridSurfaceIndex = IndexByConversionOrLookup<GsaGridSurface>(speckleAlign.gridSurface, ref gsaRecords),
        NumAlignmentPoints = speckleAlign.GetNumAlignmentPoints(),
      };
      gsaRecords.Add(gsaAlign);
      return gsaRecords;
    }

    private List<GsaRecord> InfBeamToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleInfBeam = (GSAInfluenceBeam)speckleObject;
      var gsaInfBeam = new GsaInfBeam
      {
        ApplicationId = speckleInfBeam.applicationId,
        Index = speckleInfBeam.GetIndex<GsaInfBeam>(),
        Name = speckleInfBeam.name,
        Direction = speckleInfBeam.direction.ToNative(),
        Element = IndexByConversionOrLookup<GsaEl>(speckleInfBeam.element, ref gsaRecords),
        Factor = speckleInfBeam.factor,
        Position = conversionFactors.length * speckleInfBeam.position, //TO DO: how do I know if this is a percentage or distance?
        Sid = speckleObject.id,
        Type = speckleInfBeam.type.ToNative(),
      };
      gsaRecords.Add(gsaInfBeam);
      return gsaRecords;
    }

    private List<GsaRecord> InfNodeToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleInfNode = (GSAInfluenceNode)speckleObject;
      var gsaInfNode = new GsaInfNode()
      {
        ApplicationId = speckleObject.applicationId,
        Index = speckleInfNode.GetIndex<GsaInfNode>(),
        Name = speckleInfNode.name,
        Direction = speckleInfNode.direction.ToNative(),
        Factor = speckleInfNode.factor,
        Sid = speckleObject.id,
        Type = speckleInfNode.type.ToNative(),
        Node = speckleInfNode.node.NodeAt(conversionFactors),
      };
      if (GetAxis(speckleInfNode.axis, out AxisRefType gsaRefType, out var axisIndex, ref gsaRecords))
      {
        gsaInfNode.AxisRefType = gsaRefType;
        gsaInfNode.AxisIndex = axisIndex;
      }
      gsaRecords.Add(gsaInfNode);
      return gsaRecords;
    }

    private List<GsaRecord> PathToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var specklePath = (GSAPath)speckleObject;
      var gsaPath = new GsaPath()
      {
        ApplicationId = specklePath.applicationId,
        Index = specklePath.GetIndex<GsaPath>(),
        Name = specklePath.name,
        Sid = speckleObject.id,
        Factor = specklePath.factor,
        Alignment = IndexByConversionOrLookup<GsaAlign>(specklePath.alignment, ref gsaRecords),
        Group = specklePath.group.IsPositiveOrNull(),
        NumMarkedLanes = specklePath.numMarkedLanes.IsPositiveOrNull(),
        Type = specklePath.type.ToNative(),
      };
      if (specklePath.left != 0) gsaPath.Left = conversionFactors.length * specklePath.left;
      if (specklePath.right != 0) gsaPath.Right = conversionFactors.length * specklePath.right;
      gsaRecords.Add(gsaPath);
      return gsaRecords;
    }

    private List<GsaRecord> GSAUserVehicleToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var speckleVehicle = (GSAUserVehicle)speckleObject;
      var gsaVehicle = new GsaUserVehicle()
      {
        ApplicationId = speckleVehicle.applicationId,
        Index = speckleVehicle.GetIndex<GsaUserVehicle>(),
        Name = speckleVehicle.name,
        NumAxle = speckleVehicle.axlePositions.Count(),
        AxlePosition = speckleVehicle.axlePositions.Select(v => conversionFactors.length * v).ToList(),
        AxleOffset = speckleVehicle.axleOffsets.Select(v => conversionFactors.length * v).ToList(),
        AxleLeft = speckleVehicle.axleLeft.Select(v => conversionFactors.force * v).ToList(),
        AxleRight = speckleVehicle.axleRight.Select(v => conversionFactors.force * v).ToList(),
      };
      if (speckleVehicle.width > 0) gsaVehicle.Width = conversionFactors.length * speckleVehicle.width;
      gsaRecords.Add(gsaVehicle);
      return gsaRecords;
    }

    #endregion

    #region Analysis Stage
    public List<GsaRecord> AnalStageToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var analStage = (GSAStage)speckleObject;
      var gsaAnalStage = new GsaAnalStage()
      {
        ApplicationId = analStage.applicationId,
        Index = analStage.GetIndex<GsaAnalStage>(),
        Name = analStage.name,
        Days = analStage.stageTime.IsPositiveOrNull(),
        Phi = analStage.creepFactor.IsPositiveOrNull(),
      };
      if (analStage.colour != null)
      {
        gsaAnalStage.Colour = analStage.colour.ColourToNative();
      }
      if (analStage.elements != null)
      {
        var speckleElements = analStage.elements.FindAll(e => e is Element1D || e is Element2D);
        var speckleMembers = analStage.elements.FindAll(e => e is GSAMember1D || e is GSAMember2D);
        gsaAnalStage.ElementIndices = IndexByConversionOrLookup<GsaEl>(speckleElements, ref gsaRecords) ?? new List<int>();
        gsaAnalStage.MemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleMembers, ref gsaRecords) ?? new List<int>();
      }
      if (analStage.lockedElements != null)
      {
        var speckleElements = analStage.lockedElements.FindAll(e => e is Element1D || e is Element2D);
        var speckleMembers = analStage.lockedElements.FindAll(e => e is GSAMember1D || e is GSAMember2D);
        gsaAnalStage.LockElementIndices = IndexByConversionOrLookup<GsaEl>(speckleElements, ref gsaRecords) ?? new List<int>();
        gsaAnalStage.LockMemberIndices = IndexByConversionOrLookup<GsaMemb>(speckleMembers, ref gsaRecords) ?? new List<int>();
      }
      gsaRecords.Add(gsaAnalStage);
      return gsaRecords;
    }

    public List<GsaRecord> AnalStagePropToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();
      var analStageProp = (GSAStageProp)speckleObject;
      var gsaAnalStageProp = new GsaAnalStageProp()
      {
        ApplicationId = analStageProp.applicationId,
        Index = analStageProp.GetIndex<GsaAnalStageProp>(),
        StageIndex = IndexByConversionOrLookup<GsaAnalStage>(analStageProp.stage, ref gsaRecords),
      };

      var type = analStageProp.type.ToNative();
      gsaAnalStageProp.Type = type;

      switch (type)
      {
        case ElementPropertyType.Beam:
          gsaAnalStageProp.ElemPropIndex = IndexByConversionOrLookup<GsaPropSec>(analStageProp.elementProperty, ref gsaRecords);
          gsaAnalStageProp.StagePropIndex = IndexByConversionOrLookup<GsaPropSec>(analStageProp.stageProperty, ref gsaRecords);
          break;
        case ElementPropertyType.Spring:
          gsaAnalStageProp.ElemPropIndex = IndexByConversionOrLookup<GsaPropSpr>(analStageProp.elementProperty, ref gsaRecords);
          gsaAnalStageProp.StagePropIndex = IndexByConversionOrLookup<GsaPropSpr>(analStageProp.stageProperty, ref gsaRecords);
          break;
        case ElementPropertyType.Mass:
          gsaAnalStageProp.ElemPropIndex = IndexByConversionOrLookup<GsaPropMass>(analStageProp.elementProperty, ref gsaRecords);
          gsaAnalStageProp.StagePropIndex = IndexByConversionOrLookup<GsaPropMass>(analStageProp.stageProperty, ref gsaRecords);
          break;
        case ElementPropertyType.TwoD:
          gsaAnalStageProp.ElemPropIndex = IndexByConversionOrLookup<GsaProp2d>(analStageProp.elementProperty, ref gsaRecords);
          gsaAnalStageProp.StagePropIndex = IndexByConversionOrLookup<GsaProp2d>(analStageProp.stageProperty, ref gsaRecords);
          break;
      }

      gsaRecords.Add(gsaAnalStageProp);
      return gsaRecords;
    }

    #endregion

    #region General Data

    public List<GsaRecord> GSAListToNative(Base speckleObject)
    {
      var gsaRecords = new List<GsaRecord>();

      var speckleGsaList = (GSAList)speckleObject;

      var gsaList = new GsaList()
      {
        ApplicationId = speckleGsaList.applicationId,
        Index = speckleGsaList.GetIndex<GsaList>(),
        Name = speckleGsaList.name,
        Type = speckleGsaList.listType.ToNative(),
        Definition = new List<int>()
      };

      var definitionRecords = new List<GsaRecord>();

      switch (speckleGsaList.listType)
      {
        case GSAListType.Node:
          if (speckleGsaList.definition != null)
          {
            var speckleNodes = speckleGsaList.definition.OfType<Node>().ToList();
            gsaList.Definition = speckleNodes.NodeAt(conversionFactors) ?? new List<int>();
          }

          else if (speckleGsaList.definitionRefs != null)
          {
            var speckleNodes = ContextObjects.Where(o => speckleGsaList.definitionRefs.Contains(o.applicationId)).Select(n => (Node)n).ToList();
            gsaList.Definition = speckleNodes.NodeAt(conversionFactors) ?? new List<int>();
          }
          break;
        case GSAListType.Member:
          ListDefinitionLookup<GsaMemb, GSAMember1D>();
          break;
        case GSAListType.Element:
          ListDefinitionLookup<GsaEl, Element1D>();
          break;
        case GSAListType.Case:
          ListDefinitionLookup<GsaLoadCase, LoadCase>();
          break;
        default:
          break;
      }

      gsaRecords.Add(gsaList);

      return gsaRecords;

      
      void ListDefinitionLookup<T, U>()
      {
        if (speckleGsaList.definition != null)
        {
          gsaList.Definition = IndexByConversionOrLookup<T>(speckleGsaList.definition.FindAll(o => o is U), ref definitionRecords) ?? new List<int>();
        }
        else if (speckleGsaList.definitionRefs != null)
        {
          var speckleCases = ContextObjects.Where(o => speckleGsaList.definitionRefs.Contains(o.applicationId)).ToList().FindAll(o => o is U);
          gsaList.Definition = IndexByConversionOrLookup<T>(speckleCases, ref definitionRecords) ?? new List<int>();
        }
      }
    }

    #endregion

    #endregion

    #region Helper
    #region ToNative
    #region Geometry
    #region Axis
    private bool GetAxis(Axis speckleAxis, out NodeAxisRefType gsaAxisRefType, out int? gsaAxisIndex, ref List<GsaRecord> gsaRecords)
    {
      gsaAxisRefType = NodeAxisRefType.NotSet;
      gsaAxisIndex = null;
      if (speckleAxis == null || speckleAxis.definition == null)
      {
        gsaAxisRefType = NodeAxisRefType.Global;
        return true;
      }
      if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = NodeAxisRefType.Global;
      }
      else if (speckleAxis.definition.IsXElevation())
      {
        gsaAxisRefType = NodeAxisRefType.XElevation;
      }
      else if (speckleAxis.definition.IsYElevation())
      {
        gsaAxisRefType = NodeAxisRefType.YElevation;
      }
      else if (speckleAxis.definition.IsVertical())
      {
        gsaAxisRefType = NodeAxisRefType.Vertical;
      }
      else if (speckleAxis.applicationId != null)
      {
        gsaAxisRefType = NodeAxisRefType.Reference;
        gsaAxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleAxis, ref gsaRecords);
      }
      else
      {
        return false;
      }

      return true;
    }

    private bool GetAxis(Axis speckleAxis, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex, ref List<GsaRecord> gsaRecords)
    {
      gsaAxisRefType = AxisRefType.NotSet;
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = AxisRefType.Local;
      }
      else if (speckleAxis.definition != null && speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else if (speckleAxis.applicationId != null)
      {
        gsaAxisRefType = AxisRefType.Reference;
        gsaAxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleAxis, ref gsaRecords);
      }
      else
      {
        return false;
      }

      return true;
    }
    #endregion

    #region Node
    private bool GetRestraint(Restraint speckleRestraint, out NodeRestraint gsaNodeRestraint, out List<GwaAxisDirection6> gsaRestraint)
    {
      gsaRestraint = null; //default

      switch (speckleRestraint.code)
      {
        case "RRRRRR":
          gsaNodeRestraint = NodeRestraint.Free;
          break;
        case "FFFRRR":
          gsaNodeRestraint = NodeRestraint.Pin;
          break;
        case "FFFFFF":
          gsaNodeRestraint = NodeRestraint.Fix;
          break;
        default:
          gsaNodeRestraint = NodeRestraint.Custom;
          gsaRestraint = new List<GwaAxisDirection6>();
          int i = 0;
          foreach (char c in speckleRestraint.code)
          {
            if (c == 'F')
            {
              if (i == 0) gsaRestraint.Add(GwaAxisDirection6.X);
              else if (i == 1) gsaRestraint.Add(GwaAxisDirection6.Y);
              else if (i == 2) gsaRestraint.Add(GwaAxisDirection6.Z);
              else if (i == 3) gsaRestraint.Add(GwaAxisDirection6.XX);
              else if (i == 4) gsaRestraint.Add(GwaAxisDirection6.YY);
              else if (i == 5) gsaRestraint.Add(GwaAxisDirection6.ZZ);
            }
            i++;
          }
          break;
      }
      return true;
    }
    #endregion

    #region Element
    public Speckle.GSA.API.GwaSchema.MemberType ToNative(Objects.Structural.Geometry.MemberType speckleMemberType, int dimension)
    {
      if (dimension == 1)
      {
        switch (speckleMemberType)
        {
          case Objects.Structural.Geometry.MemberType.Beam: return Speckle.GSA.API.GwaSchema.MemberType.Beam;
          case Objects.Structural.Geometry.MemberType.Column: return Speckle.GSA.API.GwaSchema.MemberType.Column;
          case Objects.Structural.Geometry.MemberType.Generic1D: return Speckle.GSA.API.GwaSchema.MemberType.Generic1d;
          case Objects.Structural.Geometry.MemberType.VoidCutter1D: return Speckle.GSA.API.GwaSchema.MemberType.Void1d;
          default:
            Report.ConversionErrors.Add(new Exception(speckleMemberType.ToString() + " is not currently a supported member type for a 1D element."));
            return Speckle.GSA.API.GwaSchema.MemberType.NotSet;
        }
      }
      else if (dimension == 2)
      {
        switch (speckleMemberType)
        {
          case Objects.Structural.Geometry.MemberType.Slab: return Speckle.GSA.API.GwaSchema.MemberType.Slab;
          case Objects.Structural.Geometry.MemberType.Wall: return Speckle.GSA.API.GwaSchema.MemberType.Wall;
          case Objects.Structural.Geometry.MemberType.Generic2D: return Speckle.GSA.API.GwaSchema.MemberType.Generic2d;
          case Objects.Structural.Geometry.MemberType.VoidCutter2D: return Speckle.GSA.API.GwaSchema.MemberType.Void2d;
          default:
            Report.ConversionErrors.Add(new Exception(speckleMemberType.ToString() + " is not currently a supported member type for a 2D element."));
            return Speckle.GSA.API.GwaSchema.MemberType.NotSet;
        }
      }
      return Speckle.GSA.API.GwaSchema.MemberType.NotSet;
    }

    public Speckle.GSA.API.GwaSchema.MemberType ToNative(Objects.Structural.Geometry.MemberType2D speckleMemberType)
    {
      switch (speckleMemberType)
      {
        case Objects.Structural.Geometry.MemberType2D.Slab: return Speckle.GSA.API.GwaSchema.MemberType.Slab;
        case Objects.Structural.Geometry.MemberType2D.Wall: return Speckle.GSA.API.GwaSchema.MemberType.Wall;
        case Objects.Structural.Geometry.MemberType2D.Generic2D: return Speckle.GSA.API.GwaSchema.MemberType.Generic2d;
        case Objects.Structural.Geometry.MemberType2D.VoidCutter2D: return Speckle.GSA.API.GwaSchema.MemberType.Void2d;
        default:
          Report.ConversionErrors.Add(new Exception(speckleMemberType.ToString() + " is not currently a supported member type for a 2D element."));
          return Speckle.GSA.API.GwaSchema.MemberType.Generic2d;
      }
    }

    public Speckle.GSA.API.GwaSchema.AnalysisType ToNative(Objects.Structural.Geometry.AnalysisType2D speckleMemberAnalysisType)
    {
      switch (speckleMemberAnalysisType)
      {
        case Objects.Structural.Geometry.AnalysisType2D.Linear: return Speckle.GSA.API.GwaSchema.AnalysisType.LINEAR;
        case Objects.Structural.Geometry.AnalysisType2D.Quadratic: return Speckle.GSA.API.GwaSchema.AnalysisType.QUADRATIC;
        case Objects.Structural.Geometry.AnalysisType2D.RigidXY: return Speckle.GSA.API.GwaSchema.AnalysisType.RIGID;
        default:
          Report.ConversionErrors.Add(new Exception(speckleMemberAnalysisType.ToString() + " is not currently a supported analysis type for a 2D member."));
          return Speckle.GSA.API.GwaSchema.AnalysisType.LINEAR;
      }
    }

    private bool GetReleases(Restraint speckleRelease, out Dictionary<GwaAxisDirection6, ReleaseCode> gsaRelease, out List<double> gsaStiffnesses, out ReleaseInclusion gsaReleaseInclusion)
    {
      if (speckleRelease.code == "FFFFFF")
      {
        gsaReleaseInclusion = ReleaseInclusion.NotIncluded;
        gsaRelease = null;
        gsaStiffnesses = null;
      }
      else if (speckleRelease.code.ToUpperInvariant().Contains('K'))
      {
        gsaReleaseInclusion = ReleaseInclusion.Included;
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = new List<double>();
        if (speckleRelease.stiffnessX > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessX);
        if (speckleRelease.stiffnessY > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessY);
        if (speckleRelease.stiffnessZ > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessZ);
        if (speckleRelease.stiffnessXX > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessXX);
        if (speckleRelease.stiffnessYY > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessYY);
        if (speckleRelease.stiffnessZZ > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessZZ);
      }
      else
      {
        gsaReleaseInclusion = ReleaseInclusion.Included;
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = null;
      }
      return true;
    }

    private bool GetReleases(Restraint speckleRelease, out Dictionary<GwaAxisDirection6, ReleaseCode> gsaRelease, out List<double> gsaStiffnesses)
    {
      if (speckleRelease.code.ToUpperInvariant().IndexOf('K') > 0)
      {
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = new List<double>();
        if (speckleRelease.stiffnessX > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessX);
        if (speckleRelease.stiffnessY > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessY);
        if (speckleRelease.stiffnessZ > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessZ);
        if (speckleRelease.stiffnessXX > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessXX);
        if (speckleRelease.stiffnessYY > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessYY);
        if (speckleRelease.stiffnessZZ > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessZZ);
      }
      else
      {
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = null;
      }
      return true;
    }

    private List<Point> Get1dTopolopgy(ICurve speckleBasecurve)
    {
      var specklePoints = new List<Point> { };
      var baseLine = ((Base)speckleBasecurve);
      switch (baseLine)
      {
        case Curve _curve:
          var curve = (Curve)speckleBasecurve;
          var points = curve.GetPoints();
          if (points != null)
          {
            var startPoint = points.First();
            var endPoint = points.Last();
            if (startPoint != null && endPoint != null)
              specklePoints = new List<Point>() { startPoint, endPoint };
          }
          break;
        case Line _line:
          var line = (Line)speckleBasecurve;
          if (line.start != null && line.end != null)
            specklePoints = new List<Point>() { line.start, line.end };
          break;
        case Arc _arc:
          var arc = (Arc)speckleBasecurve;
          if (arc.startPoint != null && arc.endPoint != null)
            specklePoints = new List<Point>() { arc.startPoint, arc.endPoint };
          break;
        default:
          return null; // cannot extract meaningful 1d el/memb topology from Polycurve, Polyline, Ellipse - in these cases, a topology list, rather than base curve, should be provided
      }
      return specklePoints;
    }

    #endregion
    #endregion

    #region Loading
    private bool GetLoadAxis(Axis speckleAxis, out LoadBeamAxisRefType gsaAxisRefType, out int? gsaAxisIndex, ref List<GsaRecord> gsaRecords)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = LoadBeamAxisRefType.NotSet;
        return false;
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = LoadBeamAxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleAxis, ref gsaRecords);
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = LoadBeamAxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = LoadBeamAxisRefType.Reference;
        }
      }

      return true;
    }

    private bool GetLoadAxis(Axis speckleAxis, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex, ref List<GsaRecord> gsaRecords)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = AxisRefType.NotSet;
        return false;
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleAxis, ref gsaRecords);
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = AxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = AxisRefType.Reference;
        }
      }

      return true;
    }

    private bool GetLoadAxis(Axis speckleAxis, LoadAxisType speckleAxisType, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex, ref List<GsaRecord> gsaRecords)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = speckleAxisType.ToNative();
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = IndexByConversionOrLookup<GsaAxis>(speckleAxis, ref gsaRecords);
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = AxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = AxisRefType.Reference;
        }
      }

      return true;
    }

    private string GetAnalysisCaseDescription(List<LoadCase> speckleLoadCases, List<double> speckleLoadFactors, ref List<GsaRecord> gsaRecords)
    {
      var gsaDescription = "";
      for (var i = 0; i < speckleLoadCases.Count(); i++)
      {
        if (i > 0 && speckleLoadFactors[i] > 0) gsaDescription += "+";
        if (speckleLoadFactors[i] == 1)
        {
          //Do nothing
        }
        else if (speckleLoadFactors[i] == -1)
        {
          gsaDescription += "-";
        }
        else
        {
          gsaDescription += speckleLoadFactors[i].ToString();
        }
        gsaDescription += "L" + IndexByConversionOrLookup<GsaLoadCase>(speckleLoadCases[i], ref gsaRecords);
      }
      return gsaDescription;
    }

    private string GetLoadCombinationDescription(CombinationType type, List<Base> loadCases, List<double> loadFactors, ref List<GsaRecord> gsaRecords)
    {
      if (type != CombinationType.LinearAdd) return null; //TODO - handle other cases
      var desc = "";

      for (var i = 0; i < loadCases.Count(); i++)
      {
        if (loadCases[i] != null)
        {
          if (i > 0 && loadFactors[i] > 0) desc += "+";
          if (loadFactors[i] == 1)
          {
            //Do nothing
          }
          else if (loadFactors[i] == -1)
          {
            desc += "-";
          }
          else
          {
            desc += loadFactors[i].ToString();
          }
          if (loadCases[i].GetType() == typeof(GSACombinationCase))
          {
            desc += "C" + IndexByConversionOrLookup<GsaCombination>(loadCases[i], ref gsaRecords);
          }
          else if (loadCases[i].GetType() == typeof(GSAAnalysisCase))
          {
            desc += "A" + IndexByConversionOrLookup<GsaAnal>(loadCases[i], ref gsaRecords);
          }
          else if (loadCases[i].GetType() == typeof(GSAAnalysisTask))
          {
            desc += "T" + IndexByConversionOrLookup<GsaTask>(loadCases[i], ref gsaRecords);
          }
          else
          {
            return null;
          }
        }
      }
      return desc;
    }

    private bool GetPolyline(GSAPolyline specklePolyline, out LoadLineOption gsaOption, out string gsaPolygon, out int? gsaPolgonIndex)
    {
      //Defaults outputs
      gsaOption = LoadLineOption.NotSet;
      gsaPolygon = "";
      gsaPolgonIndex = null;

      //Try and find index, else create string
      if (specklePolyline == null) return false;
      //if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.LookupIndex<GsaPolyline>(specklePolyline.applicationId);
      if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.ResolveIndex<GsaPolyline>(specklePolyline.applicationId);
      if (gsaPolgonIndex == null) gsaPolygon = specklePolyline.description.ToGwaString();
      if (gsaPolgonIndex == null && gsaPolygon == "") return false;
      else if (gsaPolgonIndex != null) gsaOption = LoadLineOption.PolyRef;
      else gsaOption = LoadLineOption.Polygon;
      return true;
    }

    private bool GetPolyline(GSAPolyline specklePolyline, out LoadAreaOption gsaOption, out string gsaPolygon, out int? gsaPolgonIndex)
    {
      //Defaults outputs
      gsaOption = LoadAreaOption.Plane;
      gsaPolygon = "";
      gsaPolgonIndex = null;

      //Try and find index, else create string
      if (specklePolyline == null) return true;
      //if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.LookupIndex<GsaPolyline>(specklePolyline.applicationId);
      if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.ResolveIndex<GsaPolyline>(specklePolyline.applicationId);
      if (gsaPolgonIndex == null) gsaPolygon = specklePolyline.ToString();
      if (gsaPolgonIndex == null && gsaPolygon == "")
      {
        gsaOption = LoadAreaOption.NotSet;
        return false;
      }
      else if (gsaPolgonIndex != null) gsaOption = LoadAreaOption.PolyRef;
      else gsaOption = LoadAreaOption.Polygon;
      return true;
    }
    #endregion

    #region Materials
    private GsaMatSteel GsaMatSteelExample(Steel speckleSteel)
    {
      return new GsaMatSteel()
      {
        Index = speckleSteel.GetIndex<GsaMatSteel>(),
        ApplicationId = speckleSteel.applicationId,
        Name = "",
        Mat = new GsaMat()
        {
          Name = "",
          E = 2e11,
          F = 360000000,
          Nu = 0.3,
          G = 7.692307692e+10,
          Rho = 7850,
          Alpha = 1.2e-5,
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = 2e11,
            Nu = 0.3,
            Rho = 7850,
            Alpha = 1.2e-5,
            G = 7.692307692e+10,
            Damp = 0
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = new double[0],
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = new double[0],
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = new double[0],
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = new double[0],
          Eps = 0.05,
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.UNDEF },
            StrainElasticCompression = 0.0018,
            StrainElasticTension = 0.0018,
            StrainPlasticCompression = 0.0018,
            StrainPlasticTension = 0.0018,
            StrainFailureCompression = 0.05,
            StrainFailureTension = 0.05,
            GammaF = 1,
            GammaE = 1
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = 0.0018,
            StrainElasticTension = 0.0018,
            StrainPlasticCompression = 0.0018,
            StrainPlasticTension = 0.0018,
            StrainFailureCompression = 0.05,
            StrainFailureTension = 0.05,
            GammaF = 1,
            GammaE = 1
          },
          Cost = 0,
          Type = MatType.STEEL
        },
        Fy = 360000000,
        Fu = 450000000,
        EpsP = 0,
        Eh = 0,
      };
    }

    private GsaMat GetMat(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMat = new GsaMat();
      gsaMat.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      var e = speckleObject.GetDynamicValue<double?>("E", dynamicMembers);
      var f = speckleObject.GetDynamicValue<double?>("F", dynamicMembers);
      var nu = speckleObject.GetDynamicValue<double?>("Nu", dynamicMembers);
      var g = speckleObject.GetDynamicValue<double?>("G", dynamicMembers);
      var rho = speckleObject.GetDynamicValue<double?>("Rho", dynamicMembers);
      var alpha = speckleObject.GetDynamicValue<double?>("Alpha", dynamicMembers);
      gsaMat.Prop = GetMatAnal(speckleObject.GetDynamicValue<Base>("Prop", dynamicMembers));
      gsaMat.Uls = GetMatCurveParam(speckleObject.GetDynamicValue<Base>("Uls", dynamicMembers));
      gsaMat.Sls = GetMatCurveParam(speckleObject.GetDynamicValue<Base>("Sls", dynamicMembers));
      var eps = speckleObject.GetDynamicValue<double?>("Eps", dynamicMembers);
      var cost = speckleObject.GetDynamicValue<double?>("Cost", dynamicMembers);
      gsaMat.Type = speckleObject.GetDynamicEnum<MatType>("Type", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsUC", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsSC", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsUT", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsST", dynamicMembers);
      if (gsaMat.PtsUC != null)
      {
        gsaMat.NumUC = gsaMat.PtsUC.Length;
        gsaMat.AbsUC = speckleObject.GetDynamicEnum<Dimension>("AbsUC", dynamicMembers);
        gsaMat.OrdUC = speckleObject.GetDynamicEnum<Dimension>("OrdUC", dynamicMembers);
      }
      if (gsaMat.PtsSC != null)
      {
        gsaMat.NumSC = gsaMat.PtsSC.Length;
        gsaMat.AbsSC = speckleObject.GetDynamicEnum<Dimension>("AbsSC", dynamicMembers);
        gsaMat.OrdSC = speckleObject.GetDynamicEnum<Dimension>("OrdSC", dynamicMembers);
      }
      if (gsaMat.PtsUT != null)
      {
        gsaMat.NumUT = gsaMat.PtsUT.Length;
        gsaMat.AbsUT = speckleObject.GetDynamicEnum<Dimension>("AbsUT", dynamicMembers);
        gsaMat.OrdUT = speckleObject.GetDynamicEnum<Dimension>("OrdUT", dynamicMembers);
      }
      if (gsaMat.PtsST != null)
      {
        gsaMat.NumST = gsaMat.PtsST.Length;
        gsaMat.AbsST = speckleObject.GetDynamicEnum<Dimension>("AbsST", dynamicMembers);
        gsaMat.OrdST = speckleObject.GetDynamicEnum<Dimension>("OrdST", dynamicMembers);
      }

      //unit conversion
      var stressFactor = conversionFactors.stress;
      var strainFactor = conversionFactors.strain;
      var densityFactor = conversionFactors.DensityFactorToNative();
      var thermalFactor = conversionFactors.ThermalExapansionFactorToNative();
      if (e.HasValue && e > 0) gsaMat.E = stressFactor * e;
      if (f.HasValue && f > 0) gsaMat.F = stressFactor * f;
      if (nu.HasValue && nu > 0) gsaMat.Nu = nu;
      if (g.HasValue && g > 0) gsaMat.G = stressFactor * g;
      if (rho.HasValue && rho > 0) gsaMat.Rho = densityFactor * rho;
      if (alpha.HasValue && alpha > 0) gsaMat.Alpha = thermalFactor * alpha;
      if (eps.HasValue && eps > 0) gsaMat.Eps = strainFactor * eps;
      if (cost.HasValue && cost > 0) gsaMat.Cost = cost;

      return gsaMat;
    }

    private GsaMatAnal GetMatAnal(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMatAnal = new GsaMatAnal();
      gsaMatAnal.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      var index = speckleObject.GetDynamicValue<long?>("Index", dynamicMembers);
      if (index == null) index = speckleObject.GetDynamicValue<int?>("Index", dynamicMembers);
      gsaMatAnal.Index = (int?)index;
      gsaMatAnal.Type = speckleObject.GetDynamicEnum<MatAnalType>("Type", dynamicMembers);
      gsaMatAnal.NumParams = speckleObject.GetDynamicValue<int>("NumParams", dynamicMembers);
      gsaMatAnal.E = speckleObject.GetDynamicValue<double>("E", dynamicMembers).IsPositiveOrNull();
      var e = speckleObject.GetDynamicValue<double?>("E", dynamicMembers);
      var nu = speckleObject.GetDynamicValue<double?>("Nu", dynamicMembers);
      var rho = speckleObject.GetDynamicValue<double?>("Rho", dynamicMembers);
      var alpha = speckleObject.GetDynamicValue<double?>("Alpha", dynamicMembers);
      var g = speckleObject.GetDynamicValue<double?>("G", dynamicMembers);
      var damp = speckleObject.GetDynamicValue<double?>("Damp", dynamicMembers);
      var yield = speckleObject.GetDynamicValue<double?>("Yield", dynamicMembers);
      var ultimate = speckleObject.GetDynamicValue<double?>("Ultimate", dynamicMembers);
      var eh = speckleObject.GetDynamicValue<double?>("Eh", dynamicMembers);
      var beta = speckleObject.GetDynamicValue<double?>("Beta", dynamicMembers);
      var cohesion = speckleObject.GetDynamicValue<double?>("Cohesion", dynamicMembers);
      var phi = speckleObject.GetDynamicValue<double?>("Phi", dynamicMembers);
      var psi = speckleObject.GetDynamicValue<double?>("Psi", dynamicMembers);
      var scribe = speckleObject.GetDynamicValue<double?>("Scribe", dynamicMembers);
      var ex = speckleObject.GetDynamicValue<double?>("Ex", dynamicMembers);
      var ey = speckleObject.GetDynamicValue<double?>("Ey", dynamicMembers);
      var ez = speckleObject.GetDynamicValue<double?>("Ez", dynamicMembers);
      var nuxy = speckleObject.GetDynamicValue<double?>("Nuxy", dynamicMembers);
      var nuyz = speckleObject.GetDynamicValue<double?>("Nuyz", dynamicMembers);
      var nuzx = speckleObject.GetDynamicValue<double?>("Nuzx", dynamicMembers);
      var alphax = speckleObject.GetDynamicValue<double?>("Alphax", dynamicMembers);
      var alphay = speckleObject.GetDynamicValue<double?>("Alphay", dynamicMembers);
      var alphaz = speckleObject.GetDynamicValue<double?>("Alphaz", dynamicMembers);
      var gxy = speckleObject.GetDynamicValue<double?>("Gxy", dynamicMembers);
      var gyz = speckleObject.GetDynamicValue<double?>("Gyz", dynamicMembers);
      var gzx = speckleObject.GetDynamicValue<double?>("Gzx", dynamicMembers);
      var comp = speckleObject.GetDynamicValue<double?>("Comp", dynamicMembers);

      //unit conversion
      var stressFactor = conversionFactors.stress;
      var densityFactor = conversionFactors.DensityFactorToNative();
      var thermalFactor = conversionFactors.ThermalExapansionFactorToNative();
      var angleFactor = conversionFactors.angle;
      if (gsaMatAnal.Type == MatAnalType.MAT_FABRIC)
      {
        if (ex.HasValue && ex > 0) gsaMatAnal.Ex = conversionFactors.force / conversionFactors.length * ex;
        if (ey.HasValue && ey > 0) gsaMatAnal.Ey = conversionFactors.force / conversionFactors.length * ey;
        if (nu.HasValue && nu > 0) gsaMatAnal.Nu = nu;
        if (g.HasValue && g > 0) gsaMatAnal.G = conversionFactors.force / conversionFactors.length * g;
        if (comp.HasValue && comp > 0) gsaMatAnal.Comp = comp;
      }
      else
      {
        if (e.HasValue && e > 0) gsaMatAnal.E = stressFactor * e;
        if (nu.HasValue && nu > 0) gsaMatAnal.Nu = nu;
        if (rho.HasValue && rho > 0) gsaMatAnal.Rho = densityFactor * rho;
        if (alpha.HasValue && alpha > 0) gsaMatAnal.Alpha = thermalFactor * alpha;
        if (g.HasValue && g > 0) gsaMatAnal.G = stressFactor * g;
        if (damp.HasValue && damp > 0) gsaMatAnal.Damp = damp;
        if (yield.HasValue && yield > 0) gsaMatAnal.Yield = stressFactor * yield;
        if (ultimate.HasValue && ultimate > 0) gsaMatAnal.Ultimate = stressFactor * ultimate;
        if (eh.HasValue && eh > 0) gsaMatAnal.Eh = stressFactor * eh;
        if (beta.HasValue && beta > 0) gsaMatAnal.Beta = beta;
        if (cohesion.HasValue && cohesion > 0) gsaMatAnal.Cohesion = stressFactor * cohesion;
        if (phi.HasValue && phi > 0) gsaMatAnal.Phi = angleFactor * phi;
        if (psi.HasValue && psi > 0) gsaMatAnal.Psi = angleFactor * psi;
        if (scribe.HasValue && scribe > 0) gsaMatAnal.Scribe = scribe;
        if (ex.HasValue && ex > 0) gsaMatAnal.Ex = stressFactor * ex;
        if (ey.HasValue && ey > 0) gsaMatAnal.Ey = stressFactor * ey;
        if (ez.HasValue && ez > 0) gsaMatAnal.Ez = stressFactor * ez;
        if (nuxy.HasValue && nuxy > 0) gsaMatAnal.Nuxy = nuxy;
        if (nuyz.HasValue && nuyz > 0) gsaMatAnal.Nuyz = nuyz;
        if (nuzx.HasValue && nuzx > 0) gsaMatAnal.Nuzx = nuzx;
        if (alphax.HasValue && alphax > 0) gsaMatAnal.Alphax = thermalFactor * alphax;
        if (alphay.HasValue && alphay > 0) gsaMatAnal.Alphay = thermalFactor * alphay;
        if (alphaz.HasValue && alphaz > 0) gsaMatAnal.Alphaz = thermalFactor * alphaz;
        if (gxy.HasValue && gxy > 0) gsaMatAnal.Gxy = stressFactor * gxy;
        if (gyz.HasValue && gyz > 0) gsaMatAnal.Gyz = stressFactor * gyz;
        if (gzx.HasValue && gzx > 0) gsaMatAnal.Gzx = stressFactor * gzx;
      }

      return gsaMatAnal;
    }

    private GsaMatCurveParam GetMatCurveParam(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMatCurveParam = new GsaMatCurveParam();
      gsaMatCurveParam.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      var model = speckleObject.GetDynamicValue<List<string>>("Model", dynamicMembers);
      if (model == null)
      {
        gsaMatCurveParam.Model = new List<MatCurveParamType>() { MatCurveParamType.UNDEF };
      }
      else
      {
        gsaMatCurveParam.Model = model.Select(s => Enum.TryParse(s, true, out MatCurveParamType v) ? v : MatCurveParamType.UNDEF).ToList();
      }
      var epsEC = speckleObject.GetDynamicValue<double?>("StrainElasticCompression", dynamicMembers);
      var epsET = speckleObject.GetDynamicValue<double?>("StrainElasticTension", dynamicMembers);
      var epsPC = speckleObject.GetDynamicValue<double?>("StrainPlasticCompression", dynamicMembers);
      var epsPT = speckleObject.GetDynamicValue<double?>("StrainPlasticTension", dynamicMembers);
      var epsFC = speckleObject.GetDynamicValue<double?>("StrainFailureCompression", dynamicMembers);
      var epsFT = speckleObject.GetDynamicValue<double?>("StrainFailureTension", dynamicMembers);
      var gammaF = speckleObject.GetDynamicValue<double?>("GammaF", dynamicMembers);
      var gammaE = speckleObject.GetDynamicValue<double?>("GammaE", dynamicMembers);

      //unit conversions and tests to ensure positive value or null
      if (epsEC.HasValue && epsEC > 0) gsaMatCurveParam.StrainElasticCompression = epsEC * conversionFactors.strain;
      if (epsET.HasValue && epsET > 0) gsaMatCurveParam.StrainElasticTension = epsET * conversionFactors.strain;
      if (epsPC.HasValue && epsPC > 0) gsaMatCurveParam.StrainPlasticCompression = epsPC * conversionFactors.strain;
      if (epsPT.HasValue && epsPT > 0) gsaMatCurveParam.StrainPlasticTension = epsPT * conversionFactors.strain;
      if (epsFC.HasValue && epsFC > 0) gsaMatCurveParam.StrainFailureCompression = epsFC * conversionFactors.strain;
      if (epsFT.HasValue && epsFT > 0) gsaMatCurveParam.StrainFailureTension = epsFT * conversionFactors.strain;
      if (gammaF.HasValue && gammaF > 0) gsaMatCurveParam.GammaF = gammaF;
      if (gammaE.HasValue && gammaE > 0) gsaMatCurveParam.GammaE = gammaE;
      return gsaMatCurveParam;
    }

    private double GetBeta(double fc)
    {
      //assumes fc is compressive strength of concrete in speckle stress units
      fc = Math.Abs(fc) * StressUnits.GetConversionFactor(conversionFactors.speckleModelUnits.stress, StressUnits.Pascal);
      return LinearInterp(20e6, 100e6, 0.92, 0.72, fc);
    }

    private double GetEpsMax(double fc)
    {
      //assumes fc is compressive strength of concrete in speckle stress units
      fc = Math.Abs(fc) * StressUnits.GetConversionFactor(conversionFactors.speckleModelUnits.stress, StressUnits.Pascal);
      return LinearInterp(20e6, 100e6, 0.00024, 0.00084, fc);
    }

    private double GetSteelStrain(double fy)
    {
      //assumes fy is yield strength of steel in speckle stress units
      fy = Math.Abs(fy) * StressUnits.GetConversionFactor(conversionFactors.speckleModelUnits.stress, StressUnits.Pascal);
      return LinearInterp(200e6, 450e6, 0.001, 0.00225, fy);
    }
    #endregion

    #region Properties
    private GsaSection GsaSectionExample(GSAProperty1D speckleProperty)
    {
      return new GsaSection()
      {
        Index = speckleProperty.GetIndex<GsaSection>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        Colour = Colour.NO_RGB,
        Type = Section1dType.Generic,
        //PoolIndex = 0,
        ReferencePoint = ReferencePoint.Centroid,
        RefY = 0,
        RefZ = 0,
        Mass = 0,
        Fraction = 1,
        Cost = 0,
        Left = 0,
        Right = 0,
        Slab = 0,
        Components = new List<GsaSectionComponentBase>()
        {
          new SectionComp()
          {
            Name = "",
            //MatAnalIndex = 0,
            MaterialType = Section1dMaterialType.STEEL,
            MaterialIndex = 1,
            OffsetY = 0,
            OffsetZ = 0,
            Rotation = 0,
            Reflect = ComponentReflection.NONE,
            //Pool = 0,
            TaperType = Section1dTaperType.NONE,
            //TaperPos = 0
            ProfileGroup = Section1dProfileGroup.Catalogue,
            ProfileDetails = new ProfileDetailsCatalogue()
            {
              Group = Section1dProfileGroup.Catalogue,
              Profile = "CAT A-UB 610UB125 19981201"
            }
          },
          new SectionSteel()
          {
            //GradeIndex = 0,
            PlasElas = 1,
            NetGross = 1,
            Exposed = 1,
            Beta = 0.4,
            Type = SectionSteelSectionType.HotRolled,
            Plate = SectionSteelPlateType.Undefined,
            Locked = false
          }
        },
        Environ = false
      };
    }

    private double MassModifierUnitConversion(double speckleMassModifier)
    {
      if (speckleMassModifier > 0)
      {
        return speckleMassModifier * conversionFactors.mass;
      }
      else
      {
        return speckleMassModifier; //percentage
      }
    }
    #endregion

    #region Constraint
    private Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>> GetRigidConstraint(Dictionary<AxisDirection6, List<AxisDirection6>> speckleConstraint)
    {
      if (speckleConstraint == null) return null;

      var gsaConstraint = new Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>>();
      foreach (var key in speckleConstraint.Keys)
      {
        var speckleKey = key.ToNative();
        gsaConstraint[speckleKey] = new List<GwaAxisDirection6>();
        foreach (var val in speckleConstraint[key])
        {
          gsaConstraint[speckleKey].Add(val.ToNative());
        }
      }
      return gsaConstraint;
    }

    private Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>> GetRigidConstraint(GSAConstraintCondition speckleConstraint)
    {
      if (speckleConstraint == null) return null;

      var gsaConstraint = new Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>>();
      var dynamicMembers = speckleConstraint.GetMembers();
      foreach (var memb in dynamicMembers)
      {
        GwaAxisDirection6 speckleKey;
        var parse = Enum.TryParse<GwaAxisDirection6>(memb.Key, out speckleKey);
        if (parse)
        {
          var values = (List<string>)memb.Value;
          if (values != null)
          {
            var dirs = new List<GwaAxisDirection6>();
            foreach (var val in values)
            {
              GwaAxisDirection6 dir;
              parse = Enum.TryParse<GwaAxisDirection6>(val.ToUpper(), out dir);
              dirs.Add(dir);
            }
            gsaConstraint[speckleKey] = dirs;
          }
        }
      }
      return gsaConstraint;
    }

    #endregion

    #region Other
    private List<int> IndexByConversionOrLookup<N>(List<Base> speckleObjects, ref List<GsaRecord> extra)
    {
      if (speckleObjects == null) return null;
      var gsaIndices = new List<int>();
      foreach (var o in speckleObjects)
      {
        var index = IndexByConversionOrLookup<N>(o, ref extra);
        if (index.HasValue) gsaIndices.Add(index.Value);
      }
      return (gsaIndices.Count() > 0) ? gsaIndices : null;
    }

    private double LinearInterp(double x1, double x2, double y1, double y2, double x) => (y2 - y1) / (x2 - x1) * (x - x1) + y1;
    #endregion

    #endregion
    #endregion
  }
}
