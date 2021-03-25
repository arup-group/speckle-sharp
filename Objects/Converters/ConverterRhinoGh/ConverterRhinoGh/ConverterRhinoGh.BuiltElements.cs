using Grasshopper.Kernel.Types;
using Objects.Geometry;
using Objects.Primitive;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using System;
using System.Collections.Generic;
using System.Linq;
using Column = Objects.BuiltElements.Column;
using Beam = Objects.BuiltElements.Beam;
using Wall = Objects.BuiltElements.Wall;
using Floor = Objects.BuiltElements.Floor;
using Ceiling = Objects.BuiltElements.Ceiling;
using Roof = Objects.BuiltElements.Roof;
using Opening = Objects.BuiltElements.Opening;
using RH = Rhino.Geometry;
using RV = Objects.BuiltElements.Revit;

namespace Objects.Converter.RhinoGh
{
  public partial class ConverterRhinoGh
  {

    public Column CurveToSpeckleColumn(RH.Curve curve)
    {
      return new Column((ICurve)ConvertToSpeckle(curve)) { units = ModelUnits };
    }

    public Beam CurveToSpeckleBeam(RH.Curve curve)
    {
      return new Beam((ICurve)ConvertToSpeckle(curve)) { units = ModelUnits };
    }

    public Opening CurveToSpeckleOpening(RH.Curve curve)
    {
      return new Opening((ICurve)ConvertToSpeckle(curve)) { units = ModelUnits };
    }

    public Floor CurveToSpeckleFloor(RH.Curve curve)
    {
      return new Floor((ICurve)ConvertToSpeckle(curve)) { units = ModelUnits };
    }

    public Wall BrepToSpeckleWall(RH.Brep brep)
    {
      Wall wall = null;
      BoundingBox brepBox = brep.GetBoundingBox(false);
      double height = brepBox.Max.Z - brepBox.Min.Z; // extract height
      var bottomCurves = GetSurfaceBrepEdges(brep, getBottom: true); // extract baseline
      var intCurves = GetSurfaceBrepEdges(brep, getInterior: true); // extract openings
      List<Base> openings = new List<Base>();
      if (intCurves != null)
        foreach (ICurve crv in intCurves)
          openings.Add(new Opening(crv));
      if (bottomCurves != null && height > 0)
        wall = new Wall(height, bottomCurves[0], openings) { units = ModelUnits };
      return wall;
    }

    public Floor BrepToSpeckleFloor(RH.Brep brep)
    {
      Floor floor = null;
      var extCurves = GetSurfaceBrepEdges(brep, getExterior: true); // extract outline
      var intCurves = GetSurfaceBrepEdges(brep, getInterior: true); // extract voids
      if (extCurves != null)
        floor = new Floor(extCurves[0], intCurves) { units = ModelUnits };
      return floor;
    }

    public Roof BrepToSpeckleRoof(RH.Brep brep)
    {
      Roof roof = null;
      var extCurves = GetSurfaceBrepEdges(brep, getExterior: true); // extract outline
      var intCurves = GetSurfaceBrepEdges(brep, getInterior: true); // extract voids
      if (extCurves != null)
        roof = new Roof(extCurves[0], intCurves) { units = ModelUnits };
      return roof;
    }

    public RV.RevitFaceWall BrepToFaceWall(RH.Brep brep, string[] args)
    {
      if (brep.Faces.Count > 1)
        return null;
      
      string family = "Default";
      string type = "Default";
      try { family = args[0]; type = args[1]; } catch { }
      return new RV.RevitFaceWall(family, type, BrepToSpeckle(brep), null) { units = ModelUnits };
    }

    public RV.DirectShape BrepToDirectShape(RH.Brep brep, string[] args)
    {
      if (args.Length == 0)
        return null;
      if (!Enum.TryParse($"{args[0]}s", out RV.RevitCategory category))
        return null;
      string name = "DirectShape";
      try { name = args[1]; } catch { }
      return new RV.DirectShape(name, category, new List<Base>() { ConvertToSpeckle(brep) }) { units = ModelUnits };
    }

    public RV.DirectShape ExtrusionToDirectShape(RH.Extrusion extrusion, string[] args)
    {
      if (args.Length == 0)
        return null;
      if (!Enum.TryParse($"{args[0]}s", out RV.RevitCategory category))
        return null;
      string name = "DirectShape";
      try { name = args[1]; } catch { }
      return new RV.DirectShape(name, category, new List<Base>() { ConvertToSpeckle(extrusion) }) { units = ModelUnits };
    }

    public RV.DirectShape MeshToDirectShape(RH.Mesh mesh, string[] args)
    {
      if (args.Length == 0)
        return null;
      if (!Enum.TryParse($"{args[0]}s", out RV.RevitCategory category))
        return null;
      string name = "DirectShape";
      try { name = args[1]; } catch { }
      return new RV.DirectShape(name, category, new List<Base>() { ConvertToSpeckle(mesh) }) { units = ModelUnits };
    }

    // edge curve convenience method
    private List<ICurve> GetSurfaceBrepEdges(RH.Brep brep, bool getExterior = true, bool getInterior = false, bool getBottom = false)
    {
      double tol = Doc.ModelAbsoluteTolerance * 1;

      RH.Curve[] brpCurves = null;
      if (getInterior)
        brpCurves = brep.DuplicateNakedEdgeCurves(false, true);
      else
        brpCurves = brep.DuplicateNakedEdgeCurves(true, false);
      if (getBottom)
      {
        var bottomCrv = brpCurves.
          Where(o => o.IsLinear())?.
          Where(o => new Vector3d(o.PointAtEnd.X - o.PointAtStart.X, o.PointAtEnd.Y - o.PointAtStart.Y, o.PointAtEnd.Z - o.PointAtStart.Z).IsPerpendicularTo(Vector3d.ZAxis))?.
          Aggregate((curMin, o) => curMin == null || o.PointAtStart.Z < curMin.PointAtStart.Z ? o : curMin);
        if (bottomCrv != null)
          brpCurves = new RH.Curve[] { bottomCrv };
      }

      List<ICurve> outCurves = null ;
      if (brpCurves != null && brpCurves.Count() > 0)
        outCurves = (brpCurves.Count() == 1) ? new List<ICurve>() { (ICurve)ConvertToSpeckle(brpCurves[0]) } : RH.Curve.JoinCurves(brpCurves, tol).Select(o => (ICurve)ConvertToSpeckle(o)).ToList();
      return outCurves;
    }
  }
}