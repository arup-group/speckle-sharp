using System;
using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Objects.Geometry;
using Objects.Structural.Properties;

namespace Objects.Structural.Geometry
{
  public class Element2D : Base, IDisplayValue<List<Mesh>>
  {
    public string name { get; set; }

    [DetachProperty]
    public List<ICurve> outline { get; set; }

    [DetachProperty]
    public Property2D property { get; set; }

    public MemberType memberType { get; set; } // <<<<<< ex. slab, wall, generic, opening

    public double offset { get; set; } //z direction (normal)
    public double orientationAngle { get; set; } //

    [DetachProperty]
    public Base parent { get; set; } //parent element

    [DetachProperty]
    public List<Node> topology { get; set; }

    [DetachProperty]
    public List<List<Node>> voids { get; set; }

    [DetachProperty]
    public List<Mesh> displayValue { get; set; }

    public string units { get; set; }

    public Element2D() { }

    public Element2D(List<Node> nodes)
    {
      this.topology = nodes;
    }

    [SchemaInfo("Element2D", "Creates a Speckle structural 2D element (based on a list of edge ie. external, geometry defining nodes)", "Structural", "Geometry")]
    public Element2D(List<Node> nodes, Property2D property, MemberType memberType = MemberType.NotSet, List<List<Node>> voids = null, double offset = 0, double orientationAngle = 0)
    {
      this.topology = nodes;
      this.property = property;
      this.memberType = memberType;
      this.voids = voids;
      this.offset = offset;
      this.orientationAngle = orientationAngle;
    }


    [SchemaInfo("Element2D (from polyline)", "Creates a Speckle structural 2D element (based on a list of edge ie. external, geometry defining nodes)", "Structural", "Geometry")]
    public Element2D(Polyline perimeter, Property2D property, MemberType memberType = MemberType.NotSet, List<Polyline> voids = null, double offset = 0, double orientationAngle = 0)
    {
      this.topology = GetNodesFromPolyline(perimeter);
      this.property = property;
      this.memberType = memberType;
      this.offset = offset;
      this.orientationAngle = orientationAngle;

      var outlineLoops = new List<ICurve>() { perimeter };
      if (voids != null)
      {
        var voidLoops = new List<List<Node>>() { };
        foreach (var v in voids)
        {
          voidLoops.Add(GetNodesFromPolyline(v));
          outlineLoops.Add(v);
        }
        this.voids = voidLoops;
      }

      this.outline = outlineLoops;
    }

    public static List<Node> GetNodesFromPolyline(Polyline outline)
    {
      if (outline == null)
        return null;

      var points = outline.GetPoints();
      var nodesPoints = points.First() == points.Last() ? points.Take(points.Count - 1).ToList() : points;
      var nodes = nodesPoints.Select(p => new Node(p, null, null, null, null, null, null)).ToList();
      nodes.ForEach(n => n.units = outline.units);
      return nodes;
    }

    #region Obsolete
    [JsonIgnore, Obsolete("Use " + nameof(displayValue) + " instead")]
    public Mesh displayMesh
    {
      get => displayValue?.FirstOrDefault();
      set => displayValue = new List<Mesh> { value };
    }
    #endregion
  }
}