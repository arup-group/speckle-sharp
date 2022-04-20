#if (OPENBUILDINGS)
using Bentley.Building.Api;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects;
using Bentley.ECObjects.Instance;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.XML;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Objects.Geometry;
using Objects.Primitive;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Arc = Objects.Geometry.Arc;
using BIM = Bentley.Interop.MicroStationDGN;
using BMIU = Bentley.MstnPlatformNET.InteropServices.Utilities;
using Box = Objects.Geometry.Box;
using Circle = Objects.Geometry.Circle;
using Curve = Objects.Geometry.Curve;
using Ellipse = Objects.Geometry.Ellipse;
using FamilyInstance = Objects.BuiltElements.Revit.FamilyInstance;
using Interval = Objects.Primitive.Interval;
using Level = Objects.BuiltElements.Level;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using Opening = Objects.BuiltElements.Opening;
using Parameter = Objects.BuiltElements.Revit.Parameter;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polyline = Objects.Geometry.Polyline;
using RevitBeam = Objects.BuiltElements.Revit.RevitBeam;
using RevitColumn = Objects.BuiltElements.Revit.RevitColumn;
using RevitFloor = Objects.BuiltElements.Revit.RevitFloor;
using RevitWall = Objects.BuiltElements.Revit.RevitWall;
using RevitWallOpening = Objects.BuiltElements.Revit.RevitWallOpening;
using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;

namespace Objects.Converter.Bentley
{
  public partial class ConverterBentley
  {
    private static int Decimals = 3;

    // this should take the used units into account
    private static double Epsilon = 0.001;

    private Dictionary<int, Level> Levels = new Dictionary<int, Level>();

    private List<RevitWall> Walls = new List<RevitWall>();

    public RevitBeam BeamToSpeckle(Dictionary<string, object> properties, string units = null)
    {
      var u = units ?? ModelUnits;

      string part = (string)GetProperty(properties, "PART");
      string family = (string)GetProperty(properties, "FAMILY");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");
      DPoint3d start = (DPoint3d)GetProperty(properties, "PTS_0");
      DPoint3d end = (DPoint3d)GetProperty(properties, "PTS_1");

      List<Parameter> parameters = new List<Parameter>();
      // justification
      parameters.AddRange(CreateParameter(properties, "PLACEMENT_POINT", u));
      // rotation
      parameters.AddRange(CreateParameter(properties, "ROTATION", u));

      Line baseLine = LineToSpeckle(ToInternalCoordinates(start), ToInternalCoordinates(end));
      Level level = new Level();
      level.units = u;

      RevitBeam beam = new RevitBeam(family, part, baseLine, level, parameters);
      beam.elementId = elementId.ToString();
      //beam.displayMesh
      beam.units = u;
      beam["containerName"] = "Structural Framing";

      return beam;
    }

    public RevitColumn ColumnToSpeckle(Dictionary<string, object> properties, string units = null)
    {
      var u = units ?? ModelUnits;

      string part = (string)GetProperty(properties, "PART");
      string family = (string)GetProperty(properties, "FAMILY");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");
      DPoint3d start = (DPoint3d)GetProperty(properties, "PTS_0");
      DPoint3d end = (DPoint3d)GetProperty(properties, "PTS_1");
      double rotation = (double)GetProperty(properties, "ROTATION");
      double rotationZ = (double)GetProperty(properties, "RotationZ");

      Line baseLine = LineToSpeckle(ToInternalCoordinates(start), ToInternalCoordinates(end));

      double bottomElevation, topElevation;
      if (start.Z > end.Z)
      {
        bottomElevation = end.Z;
        topElevation = start.Z;
      }
      else
      {
        bottomElevation = start.Z;
        topElevation = end.Z;
      }

      Level level = CreateLevel(bottomElevation, u);
      Level topLevel = CreateLevel(topElevation, u);
      double baseOffset = 0;
      double topOffset = 0;
      bool structural = true;

      List<Parameter> parameters = new List<Parameter>();
      // justification
      parameters.AddRange(CreateParameter(properties, "PLACEMENT_POINT", u));

      RevitColumn column = new RevitColumn(family, part, baseLine, level, topLevel, baseOffset, topOffset, structural, rotationZ, parameters);
      column.elementId = elementId.ToString();
      //column.displayMesh
      column.units = u;
      column["containerName"] = "Structural Columns";

      return column;
    }

    public Level CreateLevel(double elevation, string units = null)
    {
      var u = units ?? ModelUnits;
      elevation = Math.Round(elevation, Decimals);

      int levelKey = (int)(elevation * Math.Pow(10, Decimals));
      Levels.TryGetValue(levelKey, out Level level);

      if (level == null)
      {
        level = new Level("Level " + elevation + u, elevation);
        level.units = u;
        Levels.Add(levelKey, level);
      }
      return level;
    }

    public List<Parameter> CreateParameter(Dictionary<string, object> properties, string propertyName, string units = null)
    {
      var u = units ?? ModelUnits;

      switch (propertyName)
      {
        // justification
        case ("PLACEMENT_POINT"):
          int placementPoint = (int)GetProperty(properties, "PLACEMENT_POINT");

          Parameter zJustification = new Parameter("z Justification", 0, u);
          zJustification.applicationInternalName = "Z_JUSTIFICATION";
          Parameter yJustification = new Parameter("y Justification", 0, u);
          yJustification.applicationInternalName = "Y_JUSTIFICATION";

          // Revit ZJustification
          // Top = 0
          // Center = 1
          // Origin = 2
          // Bottom = 3

          // Revit YJustification
          // Left = 0
          // Center = 1
          // Origin = 2
          // Right = 3

          switch (placementPoint)
          {
            case (1):
              // bottom left
              zJustification.value = 3;
              yJustification.value = 0;
              break;

            case (2):
              // bottom center
              zJustification.value = 3;
              yJustification.value = 1;
              break;

            case (3):
              // bottom right
              zJustification.value = 3;
              yJustification.value = 3;
              break;

            case (4):
              // center left
              zJustification.value = 1;
              yJustification.value = 0;
              break;

            case (10):
              // origin origin
              zJustification.value = 2;
              yJustification.value = 2;
              break;

            case (5):
              // center center
              zJustification.value = 1;
              yJustification.value = 1;
              break;

            case (6):
              // center right
              zJustification.value = 1;
              yJustification.value = 3;
              break;

            case (7):
              // top left
              zJustification.value = 0;
              yJustification.value = 0;
              break;

            case (8):
              // top center
              zJustification.value = 0;
              yJustification.value = 1;
              break;

            case (9):
              // top right
              zJustification.value = 0;
              yJustification.value = 3;
              break;

            default:
              zJustification.value = 0;
              yJustification.value = 0;
              break;
          }

          return new List<Parameter>() { zJustification, yJustification };

        case ("ROTATION"):
          double rotation = (double)GetProperty(properties, "ROTATION");
          Parameter structuralBendDirAngle = new Parameter("Cross-Section Rotation", -Math.PI * rotation / 180.0);
          return new List<Parameter>() { structuralBendDirAngle };

        default:
          throw new SpeckleException("Parameter for property not implemented.");
      }
    }

    public Polycurve CreateClosedPolyCurve(List<ICurve> lines, string units = null)
    {
      var u = units ?? ModelUnits;
      Polycurve polyCurve = new Polycurve(u);

      // sort lines
      List<ICurve> segments = Sort(lines);

      if (segments.Count < 3)
        throw new SpeckleException("Curve outline for opening must be a polyline with at least three unique points.");

      polyCurve.segments = segments;

      //polyCurve.domain
      polyCurve.closed = true;
      //polyCurve.bbox
      //polyCurve.area
      //polyCurve.length

      return polyCurve;
    }

    private List<ICurve> Sort(List<ICurve> lines)
    {
      double eps = 0.001;

      List<ICurve> sortedLines = new List<ICurve>();
      if (lines.Count > 0)
      {
        Line firstSegment = lines[0] as Line;
        Point currentEnd = firstSegment.end;
        sortedLines.Add(firstSegment);
        lines.Remove(firstSegment);
        int i = 0;
        while (lines.Count > 0)
        {
          if (i == lines.Count)
          {
            break;
          }
          ICurve nextSegment = lines[i];
          i++;
          Point nextStart = ((Line)nextSegment).start;
          Point nextEnd = ((Line)nextSegment).end;

          double dx = Math.Abs(nextStart.x - currentEnd.x);
          double dy = Math.Abs(nextStart.y - currentEnd.y);
          double dz = Math.Abs(nextStart.z - currentEnd.z);

          if (dx < eps && dy < eps && dz < eps)
          {
            sortedLines.Add(nextSegment);
            lines.Remove(nextSegment);

            currentEnd = ((Line)nextSegment).end;
            i = 0;
          }
        }
      }
      return sortedLines;
    }

    public Line CreateWallBaseLine(List<ICurve> shortEdges, string units = null)
    {
      var u = units ?? ModelUnits;

      Line edge1 = (Line)shortEdges[0];
      Line edge2 = (Line)shortEdges[1];

      double dx1 = edge1.end.x - edge1.start.x;
      double dy1 = edge1.end.y - edge1.start.y;
      double dz1 = edge1.end.z - edge1.start.z;

      double dx2 = edge2.end.x - edge2.start.x;
      double dy2 = edge2.end.y - edge2.start.y;
      double dz2 = edge2.end.z - edge2.start.z;

      // z-coordinates need to be rounded to avoid problems in Revit regarding floating point errors or small deviations
      double x1 = edge1.start.x + dx1 / 2;
      double y1 = edge1.start.y + dy1 / 2;
      double z1 = Math.Round(edge1.start.z + dz1 / 2, Decimals);

      double x2 = edge2.start.x + dx2 / 2;
      double y2 = edge2.start.y + dy2 / 2;
      double z2 = Math.Round(edge2.start.z + dz2 / 2, Decimals);

      Point start = new Point(x1, y1, z1, u);
      Point end = new Point(x2, y2, z2, u);

      Line baseLine = new Line(start, end, u);

      return baseLine;
    }

    private FamilyInstance CappingBeamToSpeckle(Dictionary<string, object> properties, string units = null)
    {
      var u = units ?? ModelUnits;
      string part = (string)GetProperty(properties, "PART");
      string family = (string)GetProperty(properties, "FAMILY");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");
      DPoint3d start = (DPoint3d)GetProperty(properties, "PTS_0");
      DPoint3d end = (DPoint3d)GetProperty(properties, "PTS_1");
      double rotation = (double)GetProperty(properties, "ROTATION");
      double rotationZ = (double)GetProperty(properties, "RotationZ");

      Point basePoint = Point3dToSpeckle(start);
      string type = part;

      Level level = CreateLevel(basePoint.z, u);

      bool facingFlipped = false;
      bool handFlipped = false;
      FamilyInstance familyInstance = new FamilyInstance(basePoint, family, type, level, rotationZ, facingFlipped, handFlipped, new List<Parameter>());
      familyInstance.category = "Structural Foundations";
      familyInstance.elementId = elementId.ToString();
      familyInstance["containerName"] = "Structural Foundations";

      return familyInstance;
    }

    public RevitWallOpening OpeningToSpeckle(Dictionary<string, object> properties, List<ICurve> segments, string units = null)
    {
      var u = units ?? ModelUnits;

      double angle = (double)GetProperty(properties, "RotationZ");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");
      //DPoint3d rangeLow = (DPoint3d)GetProperty(properties, "RangeLow");
      //DPoint3d rangeHigh = (DPoint3d)GetProperty(properties, "RangeHigh");
      double length = (double)GetProperty(properties, "Penetrations__x002F____x0040__Length"); // height
      double width = (double)GetProperty(properties, "Penetrations__x002F____x0040__Width");
      //double radius = (double)GetProperty(properties, "Penetrations__x002F____x0040__Radius");

      DPoint3d origin = (DPoint3d)GetProperty(properties, "Origin");

      // 4 -- 3
      // |    |
      // 1 -- 2
      Point p1 = Point3dToSpeckle(origin, u);

      // rotate opening into xz plane
      p1 = RotateZ(p1, -angle);

      Point p2 = new Point(p1.x + width, p1.y, p1.z);
      Point p3 = new Point(p1.x + width, p1.y, p1.z + length);
      Point p4 = new Point(p1.x, p1.y, p1.z + length);

      // find host
      RevitWall host = null;
      foreach (RevitWall wall in Walls)
      {
        double wallAngle = (double)wall["angle"];
        double wallWidth = (double)wall["width"];

        if (Math.Abs(angle - wallAngle) > Epsilon)
        {
          continue;
        }

        Line baseLine = (Line)wall.baseLine;
        Point start = RotateZ(baseLine.start, -angle);
        Point end = RotateZ(baseLine.end, -angle);

        double minX = Math.Min(start.x, end.x);
        double maxX = Math.Max(start.x, end.x);

        // we don´t know on which side of the wall we are
        double minY = Math.Min(start.y, end.y) - width;
        double maxY = Math.Max(start.y, end.y) + width;

        if (p1.x < minX + Epsilon || p1.x > maxX - Epsilon)
          continue;
        if (p1.y < minY + Epsilon || p1.y > maxY - Epsilon)
          continue;

        double deltaX = maxX - minX;
        double deltaY = maxY - minY;

        // normalize
        double p1x = (p1.x - minX) / deltaX;
        double p1y = (p1.y - minY) / deltaY;

        if (p1x < 0 || p1x > 1 || p1y < 0 || p1y > 1)
          continue;
        if (Math.Abs(deltaX / deltaY - p1y / p1x) < Epsilon)
          continue;

        double p2x = (p2.x - minX) / deltaX;
        double p2y = (p2.y - minY) / deltaY;

        if (p2x < 0 || p2x > 1 || p2y < 0 || p2y > 1)
          continue;
        if (Math.Abs(deltaX / deltaY - p2y / p2x) < Epsilon)
          continue;

        host = wall;
      }

      // rotate back
      p1 = RotateZ(p1, angle);
      p2 = RotateZ(p2, angle);
      p3 = RotateZ(p3, angle);
      p4 = RotateZ(p4, angle);

      Polyline outline = new Polyline(new List<double>() { p1.x, p1.y, p1.z, p2.x, p2.y, p2.z, p3.x, p3.y, p3.z, p4.x, p4.y, p4.z }, u)
      {
        closed = true
      };

      if (host == null)
        throw new SpeckleException("Host element could not be found for opening " + elementId);

      RevitWallOpening opening = new RevitWallOpening
      {
        outline = outline,
        units = u,
        host = host,
        elementId = elementId.ToString()
      };
      opening["containerName"] = "Openings";

      // add opening to hosting wall
      host.elements.Add(opening);

      return opening;
    }

    public FamilyInstance PileToSpeckle(Dictionary<string, object> properties, string units = null)
    {
      var u = units ?? ModelUnits;
      string part = (string)GetProperty(properties, "PART");
      string family = (string)GetProperty(properties, "FAMILY");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");
      DPoint3d start = (DPoint3d)GetProperty(properties, "PTS_0");
      DPoint3d end = (DPoint3d)GetProperty(properties, "PTS_1");
      double rotation = (double)GetProperty(properties, "ROTATION");
      double rotationZ = (double)GetProperty(properties, "RotationZ");

      Point basePoint;
      if (start.Z > end.Z)
      {
        basePoint = Point3dToSpeckle(ToInternalCoordinates(start));
      }
      else
      {
        basePoint = Point3dToSpeckle(ToInternalCoordinates(end));
      }
      string type = part;

      Level level = CreateLevel(basePoint.z, u);
      basePoint.z = 0.0;

      bool facingFlipped = false;
      bool handFlipped = false;
      FamilyInstance familyInstance = new FamilyInstance(basePoint, family, type, level, rotationZ, facingFlipped, handFlipped, new List<Parameter>())
      {
        category = "Structural Foundations",
        elementId = elementId.ToString()
      };
      familyInstance["containerName"] = "Structural Foundations";

      return familyInstance;
    }

    public object RevitBeamToNative(RevitBeam beam)
    {
      if (beam.baseLine is Line baseLine)
      {
        DPoint3d start = Point3dToNative(baseLine.start);
        DPoint3d end = Point3dToNative(baseLine.end);

        string family = beam.family;
        string type = beam.type;

        TFCatalogList datagroup = new TFCatalogList();
        datagroup.Init("");
        ITFCatalog catalog = datagroup as ITFCatalog;

        catalog.GetCatalogItemByNames(family, type, 0, out ITFCatalogItemList catalogItemList);

        // if no catalog item is found, use Concrete Beam..
        if (catalogItemList == null)
          catalog.GetCatalogItemsByTypeName("Concrete Beam", 0, out catalogItemList);

        // ..otherwise a random one
        if (catalogItemList == null)
        {
          ITFCatalogList tfCatalog = new TFCatalogList();
          tfCatalog.AsTFCatalog.GetAllCatalogTypesList(0, out ITFCatalogTypeList catalogTypes);

          while (catalogTypes != null)
          {
            catalogTypes.AsTFCatalogType.ImplementsInterface("StructuralFramingCommon", 0, out bool isStructuralCatalog);
            if (isStructuralCatalog)
            {
              catalogTypes.AsTFCatalogType.GetName(0, out string catalogTypeName);
              tfCatalog.AsTFCatalog.GetCatalogItemsByTypeName(catalogTypeName, 0, out catalogItemList);

              //while (catalogItemList != null)
              //{
              //catalogItemList.GetNext("", out catalogItemList);
              //}
            }
            catalogTypes.GetNext("", out catalogTypes);
          }
        }


        // sections
        ISTFSectionList sectionList = new STFSectionList();
        sectionList.InitFromTFSecMgrByType(STFdSectionType.tfdSectionType_WideFlange, "");
        ISTFSectionList nextSection;
        sectionList.GetNext("", out nextSection);
        if (nextSection != null)
          sectionList = nextSection;




        ITFCatalogItem catalogItem = catalogItemList.AsTFCatalogItem;
        ISTFSection section = sectionList.AsSTFSection;




        STFLinearMemberList linearMemberList = new STFLinearMemberList();
        ISTFLinearMember linearMember = linearMemberList.AsSTFLinearMember;
        linearMember.InitFromCatalogItem(catalogItem, 0);

        string sectionName;
        section.GetName(0, out sectionName);
        linearMember.SetSectionName(sectionName, 0);

        start.ScaleInPlace(1.0 / UoR);
        end.ScaleInPlace(1.0 / UoR);

        linearMember.SetPQPoints(ref start, ref end, 0);



        linearMember.AddToDgnExt(Model, false, 0);



        return linearMember;
      }
      else
      {
        throw new SpeckleException("Only simple lines as base lines supported.");
      }
    }

    public Element RevitColumnToNative(RevitColumn column)
    {
      if (column.baseLine is Line baseLine)
      {
        DPoint3d start = Point3dToNative(baseLine.start);
        DPoint3d end = Point3dToNative(baseLine.end);


        TFCatalogList datagroup = new TFCatalogList();
        datagroup.Init("");
        ITFCatalog catalog = datagroup as ITFCatalog;

        ITFCatalogTypeList typeList;
        catalog.GetAllCatalogTypesList(0, out typeList);

        string family = column.family;
        string type = column.type;

        ITFCatalogItemList itemList;
        catalog.GetCatalogItemByNames(family, type, 0, out itemList);

        // if no catalog item is found, use a random one
        if (itemList == null)
          catalog.GetCatalogItemsByTypeName("Wall", 0, out itemList);







        //string type = revitColumn.type;

        //LineElement element = LineToNative(baseLine);


        Element element = new CellHeaderElement(Model, "cell", new DPoint3d(), DMatrix3d.Identity, new List<Element>() { });

        DPoint3d baseOrigin = new DPoint3d(0, 0, 0);
        DPoint3d topOrigin = new DPoint3d(0, 0, 2);

        DVector3d vectorX = new DVector3d(1, 1, 0);
        DVector3d vectorY = new DVector3d(1, 1, 0);

        double baseX = 5;
        double baseY = 6;
        double topX = 5;
        double topY = 6;
        DgnBoxDetail odata = new DgnBoxDetail(baseOrigin, topOrigin, vectorX, vectorY, baseX, baseY, topX, topY, true);
        SolidPrimitive sample = SolidPrimitive.CreateDgnBox(odata);

        element = DraftingElementSchema.ToElement(Model, sample, null);
        //element.AddToModel();


        return element;
      }
      else
      {
        throw new SpeckleException("Only lines as base lines supported.");
      }
    }

    public Element RevitFloorToNative(RevitFloor floor)
    {
      DisplayableElement outline = CurveToNative(floor.outline);

      TFPolyList polyList = new TFPolyList();
      // tolerance is the maximum distance in UORs between the actual curve and the approximating vectors for curved elements
      int foo = polyList.InitFromElement(outline, Tolerance * UoR, "0");
      //foo = polyList.InitFromDescr(outline, Tolerance, "0");

      // doesn´t seem to work, since area is 0
      polyList.GetArea(0, out double area);

      //foo = polyList.InitFromElement2(outline, 0, "0");
      //polyList.GetArea(0, out area);

      //Array points;
      //polyList.InitFromPoints(points, "0");


      Level level = floor.level;


      TFCatalogList datagroup = new TFCatalogList();
      datagroup.Init("");
      ITFCatalog catalog = datagroup as ITFCatalog;

      catalog.GetAllCatalogTypesList(0, out ITFCatalogTypeList typeList);

      string family = floor.family;
      string type = floor.type;

      catalog.GetCatalogItemByNames(family, type, 0, out ITFCatalogItemList itemList);

      // if no catalog item is found, use a random one
      if (itemList == null)
        catalog.GetCatalogItemsByTypeName("Slab", 0, out itemList);
      if (itemList == null)
        catalog.GetCatalogItemsByTypeName("Floor", 0, out itemList);

      TFLoadableFloorList form = new TFLoadableFloorList();

      form.InitFromCatalogItem(itemList, 0);
      form.SetFloorType(TFdLoadableFloorType.TFdLoadableFloorType_SimpleFloor, 0); // floor type to verify

      form.SetPoly(polyList, 0);
      form.SetElevation(level.elevation, 0);

      form.GetElementWritten(out Element element, Session.Instance.GetActiveDgnModelRef(), 0);


      // alternative?
      //TFFormRecipeSlabList recipe = new TFFormRecipeSlabList();



      return element;
    }

    public Element RevitWallToNative(RevitWall wall)
    {
      if (wall.baseLine is Line baseLine)
      {
        baseLine.start.z += wall.baseOffset;
        baseLine.end.z += wall.baseOffset;

        DPoint3d start = Point3dToNative(baseLine.start);
        DPoint3d end = Point3dToNative(baseLine.end);

        double height = wall.height + wall.topOffset;
        //double thickness = height / 10.0;

        TFCatalogList datagroup = new TFCatalogList();
        datagroup.Init("");
        ITFCatalog catalog = datagroup as ITFCatalog;

        catalog.GetAllCatalogTypesList(0, out ITFCatalogTypeList typeList);

        string family = wall.family;
        string type = wall.type;

        catalog.GetCatalogItemByNames(family, type, 0, out ITFCatalogItemList itemList);

        // if no catalog item is found, use a random one
        if (itemList == null)
          catalog.GetCatalogItemsByTypeName("Wall", 0, out itemList);

        TFLoadableWallList form = new TFLoadableWallList();
        form.InitFromCatalogItem(itemList, 0);
        form.SetWallType(TFdLoadableWallType.TFdLoadableWallType_Line, 0);
        start.ScaleInPlace(1.0 / UoR);
        end.ScaleInPlace(1.0 / UoR);
        form.SetEndPoints(ref start, ref end, 0);
        form.SetHeight(height, 0);
        //form.SetThickness(thickness, 0);

        // todo: horizontal offset
        // revit parameter: WALL_KEY_REF_PARAM  "Location Line"
        // 0. wall centerline
        // 1. core centerline
        // 2. finish face: exterior
        // 3. finish face: interior
        // 4. core face: exterior
        // 5. core face: interior
        // 
        // todo: determine interior/exterior face?
        // form.SetOffsetType(TFdFormRecipeOffsetType.tfdFormRecipeOffsetTypeCenter, 0);

        form.GetElementWritten(out Element element, Session.Instance.GetActiveDgnModelRef(), 0);
        return element;
      }
      else
      {
        throw new SpeckleException("Only simple lines as base lines supported.");
      }
    }

    public RevitFloor SlabToSpeckle(Dictionary<string, object> properties, List<ICurve> segments, string units = null)
    {
      var u = units ?? ModelUnits;

      string part = (string)GetProperty(properties, "PART");
      string family = "Floor";
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");

      Dictionary<int, List<ICurve>> elevationMap = new Dictionary<int, List<ICurve>>();
      int maxElevation = int.MinValue;

      foreach (ICurve segment in segments)
      {
        Line line = (Line)segment;
        Point start = line.start;
        Point end = line.end;

        double dx = Math.Abs(start.x - end.x);
        double dy = Math.Abs(start.y - end.y);
        double dz = Math.Abs(start.z - end.z);

        // drop vertical segments
        if (dx < Epsilon && dy < Epsilon)
        {
          continue;
        }

        if (dz > Epsilon)
        {
          throw new SpeckleException("Inclined slabs not supported!");
        }

        int elevation = (int)Math.Round(start.z / Epsilon);
        if (elevation > maxElevation)
        {
          maxElevation = elevation;
        }
        if (elevationMap.ContainsKey(elevation))
        {
          elevationMap[elevation].Add(line);
        }
        else
        {
          elevationMap.Add(elevation, new List<ICurve>() { line });
        }
      }

      if (elevationMap.Count != 2)
      {
        throw new SpeckleException("Slab geometry has more than two different elevations!");
      }

      Level level = CreateLevel(maxElevation, u);

      List<ICurve> lines = elevationMap[maxElevation];

      // todo: create bbox and sort by size
      // for now assuming that outline comes before the openings
      Polycurve outline = CreateClosedPolyCurve(lines, u);

      // all lines that are not part of the outline must be part of a void
      List<ICurve> voids = new List<ICurve>();
      while (lines.Count > 0)
      {
        Polycurve opening = CreateClosedPolyCurve(lines);
        voids.Add(opening);
      }

      RevitFloor floor = new RevitFloor
      {
        outline = outline,
        voids = voids,
        //floor.elements
        units = u,
        type = part,
        family = family,
        elementId = elementId.ToString(),
        level = level,
        structural = true,
        slope = 0
        //floor.slopeDirection
      };
      floor["containerName"] = "Floors";

      return floor;
    }

    private List<ICurve> RotateZ(List<ICurve> segments, double angle)
    {
      List<ICurve> rotatedSegments = new List<ICurve>();
      foreach (ICurve segment in segments)
      {
        Line line = (Line)segment;
        Line rotatedLine = new Line
        {
          start = RotateZ(line.start, angle),
          end = RotateZ(line.end, angle)
        };
        rotatedSegments.Add(rotatedLine);
      }
      return rotatedSegments;
    }

    public RevitWall WallToSpeckle(Dictionary<string, object> properties, List<ICurve> segments, string units = null)
    {
      var u = units ?? ModelUnits;
      string part = (string)GetProperty(properties, "PART");
      string family = "Basic Wall";
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");

      double height = (double)GetProperty(properties, "Wall_Application__x002F____x0040__Height");
      double width = (double)GetProperty(properties, "Wall_Application__x002F____x0040__Width");

      // rotate wall into xz plane
      double angle = (double)GetProperty(properties, "RotationZ");
      List<ICurve> rotatedSegments = RotateZ(segments, -angle);

      Dictionary<int, List<ICurve>> yMap = new Dictionary<int, List<ICurve>>();
      int maxY = int.MinValue;

      // collect short edges and sort the rest of the edges by inner/outer surface
      List<ICurve> shortEdges = new List<ICurve>();
      foreach (ICurve segment in rotatedSegments)
      {
        Line line = (Line)segment;
        Point start = line.start;
        Point end = line.end;

        double dx = Math.Abs(start.x - end.x);
        double dy = Math.Abs(start.y - end.y);
        double dz = Math.Abs(start.z - end.z);

        // collect segments in y-direction (wall thickness)
        // segments are not neccessary perfectly in y-direction!
        if (dz < Epsilon && dx < 2 * Math.Abs(dy))
        {
          shortEdges.Add(line);
          continue;
        }

        if (dy > Epsilon)
        {
          throw new SpeckleException("Wall geometry not supported!");
        }

        int y = (int)Math.Round(start.y / Epsilon);
        if (y > maxY)
        {
          maxY = y;
        }
        if (yMap.ContainsKey(y))
        {
          yMap[y].Add(line);
        }
        else
        {
          yMap.Add(y, new List<ICurve>() { line });
        }
      }
      if (yMap.Count != 2)
      {
        throw new SpeckleException("Wall geometry not supported!");
      }


      // sort short segments by z coordinate
      List<ICurve> sortedShortEdges = shortEdges.OrderBy(segment => ((Line)segment).start.z).ToList();

      double elevation = ((Line)sortedShortEdges[0]).start.z;
      double topElevation = ((Line)sortedShortEdges[sortedShortEdges.Count - 1]).start.z;

      sortedShortEdges = shortEdges.OrderBy(segment => ((Line)segment).start.x).ToList();

      double minX1 = ((Line)sortedShortEdges[0]).start.x;
      double maxX1 = ((Line)sortedShortEdges[sortedShortEdges.Count - 1]).start.x;
      double minX2 = ((Line)sortedShortEdges[0]).end.x;
      double maxX2 = ((Line)sortedShortEdges[sortedShortEdges.Count - 1]).end.x;
      double minX = Math.Max(minX1, minX2);
      double maxX = Math.Min(maxX1, maxX2);

      List<ICurve> foo = sortedShortEdges.Where(segment => ((Line)segment).start.x == minX).ToList();

      ICurve edge1 = sortedShortEdges.Where(segment => Math.Abs(((Line)segment).start.x - minX1) < Epsilon).OrderBy(segment => ((Line)segment).start.z).ToList()[0];
      ICurve edge2 = sortedShortEdges.Where(segment => Math.Abs(((Line)segment).start.x - maxX1) < Epsilon).OrderBy(segment => ((Line)segment).start.z).ToList()[0];

      // rotate edges back and create base line
      Line baseLine = CreateWallBaseLine(RotateZ(new List<ICurve>() { edge1, edge2 }, angle), u);

      // openings
      // identify lines that are not part of the wall outline
      List<ICurve> linesNotOutline = new List<ICurve>();
      foreach (ICurve line in yMap[maxY])
      {
        Point start = ((Line)line).start;
        Point end = ((Line)line).end;

        if (start.x - minX < Epsilon || maxX - start.x < Epsilon)
        {
          if (end.x - minX < Epsilon || maxX - end.x < Epsilon)
            continue;
        }
        linesNotOutline.Add(line);
      }

      //lines = RotateZ(lines, angle);


      // remove orphan lines
      int i = 0;
      List<ICurve> lines = new List<ICurve>();
      foreach (Line line1 in linesNotOutline)
      {
        bool orphanStart = true;
        bool orphanEnd = true;

        for (int j = 0; j < linesNotOutline.Count; j++)
        {
          if (j == i)
          {
            continue;
          }
          else
          {
            Line line2 = (Line)linesNotOutline[j];

            double dx1 = Math.Abs(((Line)line1).end.x - ((Line)line2).start.x);
            double dy1 = Math.Abs(((Line)line1).end.y - ((Line)line2).start.y);
            double dz1 = Math.Abs(((Line)line1).end.z - ((Line)line2).start.z);
            if (dx1 < Epsilon && dy1 < Epsilon && dz1 < Epsilon)
            {
              orphanEnd = false;
              continue;
            }

            double dx2 = Math.Abs(((Line)line2).end.x - ((Line)line1).start.x);
            double dy2 = Math.Abs(((Line)line2).end.y - ((Line)line1).start.y);
            double dz2 = Math.Abs(((Line)line2).end.z - ((Line)line1).start.z);
            if (dx2 < Epsilon && dy2 < Epsilon && dz2 < Epsilon)
            {
              orphanStart = false;
              continue;
            }
          }
        }
        if (orphanStart && orphanEnd)
          lines.Add(line1);
        i++;
      }

      List<ICurve> voids = new List<ICurve>();
      //while (lines.Count > 0)
      //{
      //  Polycurve opening = CreateClosedPolyCurve(linesNotOutline);
      //  if (opening != null)
      //    voids.Add(opening);
      //}

      // store opening as RevitWallOpening
      // alternative: FamilyInstance with type = Opening
      //wall.elements = new List<Base>();
      //foreach (ICurve voidOutline in voids)
      //{
      //  RevitWallOpening opening = new RevitWallOpening();
      //  opening.outline = voidOutline;
      //  opening.units = u;
      //  opening.host = wall;

      //  wall.elements.Add(opening);
      //}



      //Dictionary<int, List<ICurve>> elevationMap = new Dictionary<int, List<ICurve>>();

      // sort segments by segment.length
      //List<ICurve> sortedSegments = segments.OrderBy(segment => segment.length).ToList();

      //// drop long edges
      //sortedSegments.RemoveRange(4, 8);

      //foreach (ICurve segment in sortedSegments)
      //{
      //  Line line = (Line)segment;
      //  Point start = line.start;
      //  Point end = line.end;

      //  double dx = Math.Abs(start.x - end.x);
      //  double dy = Math.Abs(start.y - end.y);
      //  double dz = Math.Abs(start.z - end.z);

      //  // drop vertical edges
      //  if (dx < epsilon && dy < epsilon)
      //  {
      //    // there should be none
      //    continue;
      //  }

      //  if (dz > epsilon)
      //  {
      //    throw new SpeckleException("Wall geoemtry not supported!");
      //  }
      //  else
      //  {
      //    int currentElevation = (int)Math.Round(start.z / epsilon);
      //    if (elevationMap.ContainsKey(currentElevation))
      //    {
      //      elevationMap[currentElevation].Add(line);
      //    }
      //    else
      //    {
      //      List<ICurve> lines = new List<ICurve>() { line };
      //      elevationMap.Add(currentElevation, lines);
      //    }
      //  }
      //}

      //if (elevationMap.Count != 2)
      //{
      //  throw new SpeckleException("Inclined walls not supported!");
      //}

      // sort by elevations
      //List<int> sortedElevations = elevationMap.Keys.OrderBy(lines => lines).ToList();

      Level level = CreateLevel(elevation, u);
      Level topLevel = CreateLevel(topElevation, u);

      RevitWall wall = new RevitWall
      {
        height = height,
        //wall.elements = 
        baseLine = baseLine,
        units = u,
        family = family,
        type = part,
        baseOffset = 0,
        topOffset = 0,
        flipped = false,
        structural = true,
        level = level,
        topLevel = topLevel,
        elementId = elementId.ToString(),
        elements = new List<Base>()
      };
      wall["angle"] = angle;
      wall["containerName"] = "Walls";
      wall["width"] = width;

      Walls.Add(wall);

      return wall;
    }

    private DPoint3d ToInternalCoordinates(DPoint3d point)
    {
      point.ScaleInPlace(UoR);
      return point;
    }

    enum Category
    {
      Beams,
      CappingBeams,
      Columns,
      FoundationSlabs,
      None,
      Opening,
      Piles,
      Slabs,
      Walls
    }
  }
}
#endif
