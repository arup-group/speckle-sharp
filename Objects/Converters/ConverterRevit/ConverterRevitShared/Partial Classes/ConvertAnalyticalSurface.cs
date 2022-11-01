using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements.Revit;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Materials;
using Objects.Structural.Properties;
using Speckle.Core.Models;
using System.Collections.Generic;
using System.Linq;
using DB = Autodesk.Revit.DB;


namespace Objects.Converter.Revit
{
    public partial class ConverterRevit
    {
        public Objects.Geometry.Line GetBottomLine(List<Node> nodes)
        {
            Objects.Geometry.Line baseLine = new Objects.Geometry.Line();
            double lowest_elv = nodes.Min(nodes => nodes.basePoint.z);
            List<Node> nodes1 = nodes.FindAll(node => node.basePoint.z.Equals(lowest_elv));
            if (nodes1.Count == 2)
            {
                var point1 = nodes1[0].basePoint;
                var point2 = nodes1[1].basePoint;
                baseLine = new Geometry.Line(point1, point2, point1.units);
                return baseLine;
            }
            return null;
        }

        public Objects.Geometry.Polycurve PolycurveFromTopology(List<Node> nodes)
        {
            Polycurve polycurve = new Polycurve();
            foreach (int index in Enumerable.Range(0, nodes.Count))
            {
                if (index == nodes.Count - 1)
                {
                    var point1 = nodes[index].basePoint;
                    var point2 = nodes[0].basePoint;
                    Geometry.Line segment = new Geometry.Line(point1, point2, point1.units);
                    polycurve.segments.Add(segment);
                }
                else
                {
                    var point1 = nodes[index].basePoint;
                    var point2 = nodes[index + 1].basePoint;
                    Geometry.Line segment = new Geometry.Line(point1, point2, point1.units);
                    polycurve.segments.Add(segment);
                }
            }
            return polycurve;
        }

        public ApplicationObject AnalyticalSurfaceToNative(Element2D speckleElement)
        {
      switch (speckleElement.memberType)
            {
                case MemberType.Wall:
                    Geometry.Line baseline = GetBottomLine(speckleElement.topology);
                    double lowestElvevation = speckleElement.topology.Min(node => node.basePoint.z);
                    double topElevation = speckleElement.topology.Max(node => node.basePoint.z);
                    Node bottomNode = speckleElement.topology.Find(node => node.basePoint.z == lowestElvevation);
                    Node topNode = speckleElement.topology.Find(node => node.basePoint.z == topElevation);
                    var bottemLevel = LevelFromPoint(PointToNative(bottomNode.basePoint));
                    var topLevel = LevelFromPoint(PointToNative(topNode.basePoint));
                    RevitWall revitWall = new RevitWall("", speckleElement.property.name, baseline, bottemLevel, topLevel);
                    revitWall.structural = true;
                    return WallToNative(revitWall);
                default:
                    var polycurve = PolycurveFromTopology(speckleElement.topology);
                    var level = LevelFromPoint(PointToNative(speckleElement.topology[0].basePoint));
                    RevitFloor revitFloor = new RevitFloor(speckleElement.property.name, speckleElement.property.name, polycurve, level, true);
                    revitFloor.structural = true;
                    return FloorToNative(revitFloor);
            }
        }

#if !REVIT2023
        private Element2D AnalyticalSurfaceToSpeckle(AnalyticalModelSurface revitSurface)
        {
            if (!revitSurface.IsEnabled())
                return new Element2D();

            var speckleElement2D = new Element2D();
            var structuralElement = revitSurface.Document.GetElement(revitSurface.GetElementId());
            var mark = GetParamValue<string>(structuralElement, BuiltInParameter.ALL_MODEL_MARK);
            speckleElement2D.name = mark;

            var edgeNodes = new List<Node> { };
            var loops = revitSurface.GetLoops(AnalyticalLoopType.External);

            var displayLine = new Polycurve();
            foreach (var loop in loops)
            {
                var coor = new List<double>();
                foreach (var curve in loop)
                {
                    var points = curve.Tessellate();

                    foreach (var p in points.Skip(1))
                    {
                        var vertex = PointToSpeckle(p);
                        var edgeNode = new Node(vertex, null, null, null);
                        edgeNodes.Add(edgeNode);
                    }

                    displayLine.segments.Add(CurveToSpeckle(curve));
                }
            }

            speckleElement2D.topology = edgeNodes;
            speckleElement2D["displayValue"] = displayLine;

            var outline = new List<ICurve> { };
            outline.Add(displayLine);

            var voidNodes = new List<List<Node>> { };
            var voidLoops = revitSurface.GetLoops(AnalyticalLoopType.Void);
            foreach (var loop in voidLoops)
            {
                var loopNodes = new List<Node>();
                var voidDisplayLine = new Polycurve();
                foreach (var curve in loop)
                {
                    var points = curve.Tessellate();

                    foreach (var p in points.Skip(1))
                    {
                        var vertex = PointToSpeckle(p);
                        var voidNode = new Node(vertex, null, null, null);
                        loopNodes.Add(voidNode);
                    }
                    voidDisplayLine.segments.Add(CurveToSpeckle(curve));
                }
                voidNodes.Add(loopNodes);
                outline.Add(voidDisplayLine);
            }

            speckleElement2D.voids = voidNodes;
            speckleElement2D.outline = outline;

            //var mesh = new Geometry.Mesh();
            //var solidGeom = GetElementSolids(structuralElement);
            //(mesh.faces, mesh.vertices) = GetFaceVertexArrFromSolids(solidGeom);
            //speckleElement2D.baseMesh = mesh;	  

            //speckleElement2D.displayMesh = GetElementDisplayMesh(Doc.GetElement(revitSurface.GetElementId()),
            // new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false });

            var prop = new Property2D();

            // Material
            DB.Material structMaterial = null;
            double thickness = 0;
            var memberType = MemberType.Generic2D;

            if (structuralElement is DB.Floor)
            {
                var floor = structuralElement as DB.Floor;
                structMaterial = floor.Document.GetElement(floor.FloorType.StructuralMaterialId) as DB.Material;
                // Revit returns value correctly in mm without needing to scale with this call
                thickness = GetParamValue<double>(structuralElement, BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS);
                memberType = MemberType.Slab;
            }
            else if (structuralElement is DB.Wall)
            {
                var wall = structuralElement as DB.Wall;
                structMaterial = wall.Document.GetElement(wall.WallType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId()) as DB.Material;
                thickness = ScaleToSpeckle(wall.WallType.Width);
                memberType = MemberType.Wall;
            }

            var structAsset = (PropertySetElement)Doc.GetElement(structMaterial.StructuralAssetId);

            // If material has no physical properties in revit, assign null
            var materialAsset = structAsset != null ? structAsset.GetStructuralAsset() : null;
            var materialType = structMaterial.MaterialClass;

            Structural.Materials.StructuralMaterial speckleMaterial = null;
            switch (materialType)
	  {
		case "Concrete":
		  var concreteMaterial = new Concrete
		  {
			name = structMaterial.Name,
			materialType = Structural.MaterialType.Concrete,
			grade = null,
			designCode = null,
			codeYear = null,
			elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0,
			compressiveStrength = materialAsset != null ? materialAsset.ConcreteCompression : 0,
			tensileStrength = 0,
			flexuralStrength = 0,
			maxCompressiveStrain = 0,
			maxTensileStrain = 0,
			maxAggregateSize = 0,
			lightweight = materialAsset != null ? materialAsset.Lightweight : false,
			poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
			shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0,
			density = materialAsset != null ? materialAsset.Density : 0,
			thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0,
			dampingRatio = 0
		  };
		  speckleMaterial = concreteMaterial;
		  break;
		case "Steel":
		  var steelMaterial = new Steel
		  {
			name = structMaterial.Name,
			materialType = Structural.MaterialType.Steel,
			grade = materialAsset != null ? materialAsset.Name : null,
			designCode = null,
			codeYear = null,
			elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0, // Newtons per foot meter 
			yieldStrength = materialAsset != null ? materialAsset.MinimumYieldStress : 0, // Newtons per foot meter
			ultimateStrength = materialAsset != null ? materialAsset.MinimumTensileStrength : 0, // Newtons per foot meter
			maxStrain = 0,
			poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
			shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0, // Newtons per foot meter
			density = materialAsset != null ? materialAsset.Density : 0, // kilograms per cubed feet 
			thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0, // inverse Kelvin
			dampingRatio = 0
		  };
		  speckleMaterial = steelMaterial;
		  break;
		case "Wood":
		  var timberMaterial = new Timber
		  {
			name = structMaterial.Name,
			materialType = Structural.MaterialType.Timber,
			grade = materialAsset != null ? materialAsset.WoodGrade : null,
			designCode = null,
			codeYear = null,
			elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0, // Newtons per foot meter 
			poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
			shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0, // Newtons per foot meter
			density = materialAsset != null ? materialAsset.Density : 0, // kilograms per cubed feet 
			thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0, // inverse Kelvin
			species = materialAsset != null ? materialAsset.WoodSpecies : null,
			dampingRatio = 0
		  };
		  timberMaterial["bendingStrength"] = materialAsset != null ? materialAsset.WoodBendingStrength : 0;
		  timberMaterial["parallelCompressionStrength"] = materialAsset != null ? materialAsset.WoodParallelCompressionStrength : 0;
		  timberMaterial["parallelShearStrength"] = materialAsset != null ? materialAsset.WoodParallelShearStrength : 0;
		  timberMaterial["perpendicularCompressionStrength"] = materialAsset != null ? materialAsset.WoodPerpendicularCompressionStrength : 0;
		  timberMaterial["perpendicularShearStrength"] = materialAsset != null ? materialAsset.WoodPerpendicularShearStrength : 0;
		  speckleMaterial = timberMaterial;
		  break;
		default:
		  var defaultMaterial = new Structural.Materials.StructuralMaterial
		  {
			name = structMaterial.Name
          };
		  speckleMaterial = defaultMaterial;
                    break;
            }
            speckleMaterial.applicationId = $"{materialType}:{structMaterial.UniqueId}";
            prop.material = speckleMaterial;
            prop.name = structuralElement.Name;
            prop.applicationId = $"{structuralElement.Name}:{structMaterial.UniqueId}";
            prop.type = Structural.PropertyType2D.Shell;
            prop.thickness = thickness;
            prop.units = ModelUnits;

            speckleElement2D.memberType = memberType;
            speckleElement2D.property = prop;

            GetAllRevitParamsAndIds(speckleElement2D, revitSurface);

            return speckleElement2D;
        }
#else
    private Element2D AnalyticalSurfaceToSpeckle(AnalyticalPanel revitSurface)
    {
      var speckleElement2D = new Element2D();

      var structuralElement = revitSurface;

      var mark = GetParamValue<string>(structuralElement, BuiltInParameter.ALL_MODEL_MARK);
      speckleElement2D.name = mark;

      var edgeNodes = new List<Node> { };
      var loops = revitSurface.GetOuterContour();

      var displayLine = new Polycurve();
      foreach (var loop in loops)
      {
        var coor = new List<double>();

        var points = loop.Tessellate();

        foreach (var p in points.Skip(1))
        {
          var vertex = PointToSpeckle(p);
          var edgeNode = new Node(vertex, null, null, null);
          edgeNodes.Add(edgeNode);
        }

        displayLine.segments.Add(CurveToSpeckle(loop));
      }

      speckleElement2D.topology = edgeNodes;
      var analyticalToPhysicalManager = AnalyticalToPhysicalAssociationManager.GetAnalyticalToPhysicalAssociationManager(Doc);
      if (analyticalToPhysicalManager.HasAssociation(revitSurface.Id))
      {
        var physicalElementId = analyticalToPhysicalManager.GetAssociatedElementId(revitSurface.Id);
        var physicalElement = Doc.GetElement(physicalElementId);
        speckleElement2D.displayValue = GetElementDisplayMesh(physicalElement, new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false });
      }
      //speckleElement2D["displayValue"] = displayLine;

      //speckleElement2D.voids = voidNodes;

      //var mesh = new Geometry.Mesh();
      //var solidGeom = GetElementSolids(structuralElement);
      //(mesh.faces, mesh.vertices) = GetFaceVertexArrFromSolids(solidGeom);
      //speckleElement2D.baseMesh = mesh;	  

      var prop = new Property2D();

      // Material
      DB.Material structMaterial = null;
      double thickness = 0;
      var memberType = MemberType2D.Generic2D;

      if (structuralElement.StructuralRole is AnalyticalStructuralRole.StructuralRoleFloor)
      {
        structMaterial = structuralElement.Document.GetElement(structuralElement.MaterialId) as DB.Material;
        thickness = structuralElement.Thickness;
        memberType = MemberType2D.Slab;
      }
      else if (structuralElement.StructuralRole is AnalyticalStructuralRole.StructuralRoleWall)
      {

        structMaterial = structuralElement.Document.GetElement(structuralElement.MaterialId) as DB.Material;
        thickness = structuralElement.Thickness;
        memberType = MemberType2D.Wall;
      }

      var materialAsset = ((PropertySetElement)structMaterial.Document.GetElement(structMaterial.StructuralAssetId)).GetStructuralAsset();
      var materialType = structMaterial.MaterialClass;

      Structural.Materials.StructuralMaterial speckleMaterial = null;
      switch (materialType)
      {
        case "Concrete":
          var concreteMaterial = new Concrete
          {
            name = structMaterial.Document.GetElement(structMaterial.StructuralAssetId).Name,
            materialType = Structural.MaterialType.Concrete,
            grade = null,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset.YoungModulus.X,
            compressiveStrength = materialAsset.ConcreteCompression,
            tensileStrength = 0,
            flexuralStrength = 0,
            maxCompressiveStrain = 0,
            maxTensileStrain = 0,
            maxAggregateSize = 0,
            lightweight = materialAsset.Lightweight,
            poissonsRatio = materialAsset.PoissonRatio.X,
            shearModulus = materialAsset.ShearModulus.X,
            density = materialAsset.Density,
            thermalExpansivity = materialAsset.ThermalExpansionCoefficient.X,
            dampingRatio = 0
          };
          speckleMaterial = concreteMaterial;
          break;
        case "Steel":
          var steelMaterial = new Steel
          {
            name = structMaterial.Document.GetElement(structMaterial.StructuralAssetId).Name,
            materialType = Structural.MaterialType.Steel,
            grade = materialAsset.Name,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset.YoungModulus.X, // Newtons per foot meter 
            yieldStrength = materialAsset.MinimumYieldStress, // Newtons per foot meter
            ultimateStrength = materialAsset.MinimumTensileStrength, // Newtons per foot meter
            maxStrain = 0,
            poissonsRatio = materialAsset.PoissonRatio.X,
            shearModulus = materialAsset.ShearModulus.X, // Newtons per foot meter
            density = materialAsset.Density, // kilograms per cubed feet 
            thermalExpansivity = materialAsset.ThermalExpansionCoefficient.X, // inverse Kelvin
            dampingRatio = 0
          };
          speckleMaterial = steelMaterial;
          break;
        case "Wood":
          var timberMaterial = new Timber
          {
            name = structMaterial.Document.GetElement(structMaterial.StructuralAssetId).Name,
            materialType = Structural.MaterialType.Timber,
            grade = materialAsset.WoodGrade,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset.YoungModulus.X, // Newtons per foot meter 
            poissonsRatio = materialAsset.PoissonRatio.X,
            shearModulus = materialAsset.ShearModulus.X, // Newtons per foot meter
            density = materialAsset.Density, // kilograms per cubed feet 
            thermalExpansivity = materialAsset.ThermalExpansionCoefficient.X, // inverse Kelvin
            species = materialAsset.WoodSpecies,
            dampingRatio = 0
          };
          timberMaterial["bendingStrength"] = materialAsset.WoodBendingStrength;
          timberMaterial["parallelCompressionStrength"] = materialAsset.WoodParallelCompressionStrength;
          timberMaterial["parallelShearStrength"] = materialAsset.WoodParallelShearStrength;
          timberMaterial["perpendicularCompressionStrength"] = materialAsset.WoodPerpendicularCompressionStrength;
          timberMaterial["perpendicularShearStrength"] = materialAsset.WoodPerpendicularShearStrength;
          speckleMaterial = timberMaterial;
          break;
        default:
          var defaultMaterial = new Structural.Materials.StructuralMaterial
          {
            name = structMaterial.Document.GetElement(structMaterial.StructuralAssetId).Name
          };
          speckleMaterial = defaultMaterial;
          break;
      }

      prop.material = speckleMaterial;
      prop.name = structuralElement.Name;
      //prop.type = memberType;
      //prop.analysisType = Structural.AnalysisType2D.Shell;
      prop.thickness = thickness;

      speckleElement2D.property = prop;

      GetAllRevitParamsAndIds(speckleElement2D, revitSurface);

      //speckleElement2D.displayMesh = GetElementDisplayMesh(Doc.GetElement(revitSurface.GetElementId()),
      // new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false });

      return speckleElement2D;
    }
#endif
    }

}