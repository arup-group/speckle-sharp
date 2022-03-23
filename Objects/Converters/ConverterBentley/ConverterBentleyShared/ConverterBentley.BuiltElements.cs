﻿using Bentley.DgnPlatformNET;
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
using Parameter = Objects.BuiltElements.Revit.Parameter;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polyline = Objects.Geometry.Polyline;
using RevitBeam = Objects.BuiltElements.Revit.RevitBeam;
using RevitColumn = Objects.BuiltElements.Revit.RevitColumn;
using RevitFloor = Objects.BuiltElements.Revit.RevitFloor;
using RevitWall = Objects.BuiltElements.Revit.RevitWall;
using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;

namespace Objects.Converter.Bentley
{
  public partial class ConverterBentley
  {
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

      Line baseLine = LineToSpeckle(start, end, false);
      Level level = new Level();
      level.units = u;

      RevitBeam beam = new RevitBeam(family, part, baseLine, level, parameters);
      beam.elementId = elementId.ToString();
      //beam.displayMesh
      beam.units = u;

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

      Line baseLine = LineToSpeckle(start, end, false);

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

      return column;
    }

    public Level CreateLevel(double elevation, string units = null)
    {
      var u = units ?? ModelUnits;

      double accuracy = 1000.0;
      elevation = Math.Round(elevation * accuracy) / accuracy;

      Level level = new Level("Level " + elevation + u, elevation);
      level.units = u;
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
          Parameter structuralBendDirAngle = new Parameter("Cross-Section Rotation", 180 * rotation / Math.PI);
          return new List<Parameter>() { structuralBendDirAngle };

        default:
          throw new SpeckleException("Parameter for property not implemented.");
      }
    }

    public Polycurve CreatePolyCurve(List<ICurve> segments, string units = null)
    {
      var u = units ?? ModelUnits;
      Polycurve polyCurve = new Polycurve(u);

      double eps = 0.001;

      // sort segments
      if (segments.Count > 0)
      {
        Line firstSegment = segments[0] as Line;
        Point currentEnd = firstSegment.end;
        List<ICurve> sortedSegments = new List<ICurve>();
        sortedSegments.Add(firstSegment);
        segments.Remove(firstSegment);
        int i = 0;
        while (segments.Count > 0)
        {
          if (i > segments.Count)
          {
            break;
          }
          ICurve nextSegment = segments[i];
          i++;
          Point nextStart = ((Line)nextSegment).start;
          Point nextEnd = ((Line)nextSegment).end;

          double dx = Math.Abs(nextStart.x - currentEnd.x);
          double dy = Math.Abs(nextStart.y - currentEnd.y);
          double dz = Math.Abs(nextStart.z - currentEnd.z);

          if (dx < eps && dy < eps && dz < eps)
          {
            sortedSegments.Add(nextSegment);
            segments.Remove(nextSegment);

            currentEnd = ((Line)nextSegment).end;
            i = 0;
          }
        }
        polyCurve.segments = sortedSegments;
      }
      //polyCurve.domain
      polyCurve.closed = true;
      //polyCurve.bbox
      //polyCurve.area
      //polyCurve.length

      return polyCurve;
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

      Point start = new Point(edge1.start.x + dx1 / 2, edge1.start.y + dy1 / 2, edge1.start.z + dz1 / 2, u);
      Point end = new Point(edge2.start.x + dx2 / 2, edge2.start.y + dy2 / 2, edge2.start.z + dz2 / 2, u);

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
      return familyInstance;
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
        basePoint = Point3dToSpeckle(start, false);
      }
      else
      {
        basePoint = Point3dToSpeckle(end, false);
      }
      string type = part;

      Level level = CreateLevel(basePoint.z, u);
      basePoint.z = 0.0;

      bool facingFlipped = false;
      bool handFlipped = false;
      FamilyInstance familyInstance = new FamilyInstance(basePoint, family, type, level, rotationZ, facingFlipped, handFlipped, new List<Parameter>());
      familyInstance.category = "Structural Foundations";
      familyInstance.elementId = elementId.ToString();
      return familyInstance;
    }

    public RevitFloor SlabToSpeckle(Dictionary<string, object> properties, List<ICurve> segments, string units = null)
    {
      RevitFloor floor = new RevitFloor();
      var u = units ?? ModelUnits;

      string part = (string)GetProperty(properties, "PART");
      string family = "Floor";
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");

      Dictionary<int, List<ICurve>> elevationMap = new Dictionary<int, List<ICurve>>();
      int maxElevation = int.MinValue;

      // this should take the used units into account
      double epsilon = 0.001;

      foreach (ICurve segment in segments)
      {
        Line line = (Line)segment;
        Point start = line.start;
        Point end = line.end;

        double dx = Math.Abs(start.x - end.x);
        double dy = Math.Abs(start.y - end.y);
        double dz = Math.Abs(start.z - end.z);

        // drop vertical segments
        if (dx < epsilon && dy < epsilon)
        {
          continue;
        }

        if (dz > epsilon)
        {
          throw new SpeckleException("Inclined slabs not supported!");
        }

        int elevation = (int)Math.Round(start.z / epsilon);
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
          List<ICurve> lines = new List<ICurve>() { line };
          elevationMap.Add(elevation, lines);

        }
      }

      if (elevationMap.Count != 2)
      {
        throw new SpeckleException("Slab geometry has more than two different elevations!");
      }

      Polycurve outline = CreatePolyCurve(elevationMap[maxElevation], u);
      floor.outline = outline;
      //floor.voids
      //floor.elements
      floor.units = u;
      floor.type = part;
      floor.family = family;
      floor.elementId = elementId.ToString();
      //floor.level = new Level();
      //floor.level.units = u;
      floor.structural = true;
      //floor.slope
      //floor.slopeDirection

      return floor;
    }

    public RevitWall WallToSpeckle(Dictionary<string, object> properties, List<ICurve> segments, string units = null)
    {
      RevitWall wall = new RevitWall();

      var u = units ?? ModelUnits;
      string part = (string)GetProperty(properties, "PART");
      string family = (string)GetProperty(properties, "FAMILY");
      // for some reason the ElementID is a long
      int elementId = (int)(double)GetProperty(properties, "ElementID");

      Dictionary<int, List<ICurve>> elevationMap = new Dictionary<int, List<ICurve>>();

      // this should take the used units into account
      double epsilon = 0.001;

      // only simple walls supported so far
      if (segments.Count != 12)
      {
        throw new SpeckleException("Wall geoemtry not supported!");
      }

      // sort segments by segment.length
      List<ICurve> sortedSegments = segments.OrderBy(segment => segment.length).ToList();

      // drop long edges
      sortedSegments.RemoveRange(4, 8);

      foreach (ICurve segment in sortedSegments)
      {
        Line line = (Line)segment;
        Point start = line.start;
        Point end = line.end;

        double dx = Math.Abs(start.x - end.x);
        double dy = Math.Abs(start.y - end.y);
        double dz = Math.Abs(start.z - end.z);

        // drop vertical edges
        if (dx < epsilon && dy < epsilon)
        {
          // there should be none
          continue;
        }

        if (dz > epsilon)
        {
          throw new SpeckleException("Wall geoemtry not supported!");
        }
        else
        {
          int currentElevation = (int)Math.Round(start.z / epsilon);
          if (elevationMap.ContainsKey(currentElevation))
          {
            elevationMap[currentElevation].Add(line);
          }
          else
          {
            List<ICurve> lines = new List<ICurve>() { line };
            elevationMap.Add(currentElevation, lines);
          }
        }
      }

      if (elevationMap.Count != 2)
      {
        throw new SpeckleException("Inclined walls not supported!");
      }

      // sort by elevations
      List<int> sortedElevations = elevationMap.Keys.OrderBy(lines => lines).ToList();

      Line baseLine = CreateWallBaseLine(elevationMap[sortedElevations[0]], u);

      double elevation = sortedElevations[0] * epsilon;
      double topElevation = sortedElevations[1] * epsilon;
      double height = topElevation - elevation;

      Level level = CreateLevel(elevation, u);
      Level topLevel = CreateLevel(topElevation, u);

      wall.height = height;
      //wall.elements = 
      wall.baseLine = baseLine;
      wall.units = u;
      wall.family = family;
      wall.type = part;
      wall.baseOffset = 0;
      wall.topOffset = 0;
      wall.flipped = false;
      wall.structural = true;
      wall.level = level;
      wall.topLevel = topLevel;
      wall.elementId = elementId.ToString();

      return wall;
    }

    enum Category
    {
      Beams,
      CappingBeam,
      Columns,
      FoundationSlab,
      None,
      Piles,
      Slabs,
      Walls
    }
  }
}
