using Grasshopper.Kernel.Types;
using Objects.Geometry;
using Objects.Primitive;
using Rhino.Geometry;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry.Collections;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using System;
using System.Collections.Generic;
using System.Linq;
using Alignment = Objects.BuiltElements.Alignment;
using Column = Objects.BuiltElements.Column;
using Beam = Objects.BuiltElements.Beam;
using Wall = Objects.BuiltElements.Wall;
using Floor = Objects.BuiltElements.Floor;
using Ceiling = Objects.BuiltElements.Ceiling;
using Roof = Objects.BuiltElements.Roof;
using Opening = Objects.BuiltElements.Opening;
using Point = Objects.Geometry.Point;
using View3D = Objects.BuiltElements.View3D;
using RH = Rhino.Geometry;
using RV = Objects.BuiltElements.Revit;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;

namespace Objects.Converter.RhinoGh
{
  public partial class ConverterRhinoGh
  {
    RH.Curve Element1DToNative(Element1D element1d)
    {
      return CurveToNative(element1d.baseLine);
    }

    RH.Polyline Element2DToNative(Element2D element2d)
    {
      //if (element2d.outline != null && element2d.outline.Count() > 0)
      //{
      //  var curve = element2d.outline.Select(p => CurveToNative(p)).ToList();
      //  var polyine = new Polycurve() { segments = element2d.outline };
      //}
      //return PolylineToNative(element2d.outline as Objects.Geometry.Polyline);
      return new RH.Polyline();
    }

    RH.Point NodeToNative(Node node)
    {
      return PointToNative(node.basePoint);
    }

    RH.Curve GSAMember1DToNative(GSAMember1D member1d)
    {
      return CurveToNative(member1d.baseLine);
    }
  }
}
