﻿using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Properties;
using Objects.Structural.Properties.Profiles;
using Objects.Structural.Materials;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.Loading;

//Place holder for classes prior to being added to the structural schema

namespace Objects.Structural.GSA.Other
{
  public abstract class GSAGridLoad : Load
  {
    public int nativeId { get; set; }
    public GSAGridSurface gridSurface { get; set; }
    public Axis loadAxis { get; set; }
    public LoadDirection2D direction { get; set; }
  }

  public class GSAGridPointLoad : GSAGridLoad
  {
    public Point position { get; set; }
    public double value { get; set; }
    public GSAGridPointLoad() { }
  }

  public class GSAGridLineLoad : GSAGridLoad
  {
    public Polyline polyline { get; set; }
    public bool isProjected { get; set; }
    public List<double> values { get; set; }
    public GSAGridLineLoad() { }
  }

  public class GSAGridAreaLoad : GSAGridLoad
  {
    public Polyline polyline { get; set; }
    public bool isProjected { get; set; }
    public double value { get; set; }
    public GSAGridAreaLoad() { }
  }

  public class GSAThermal2dLoad : Load
  {
    public int nativeId { get; set; }
    public List<Element2D> elements { get; set; }
    public Thermal2dLoadType type { get; set; }
    public List<double> values { get; set; }
    public GSAThermal2dLoad() { }
  }

  public enum Thermal2dLoadType
  {
    NotSet = 0,
    Uniform,
    Gradient,
    General
  }

  public class GSAPolyline : Polyline
  {
    public string name { get; set; }
    public int nativeId { get; set; }
    public string colour { get; set; }
    public GSAGridPlane gridPlane { get; set; }
    public GSAPolyline() { }
  }

  public class GSARigid : Base
  {
    public string name { get; set; }
    public int nativeId { get; set; }
    public Node primaryNode { get; set; }
    public List<Node> constrainedNodes { get; set; }
    public Base parentMember { get; set; }
    public List<GSAStage> stages { get; set; }
    public RigidConstraint type { get; set; }
    public Dictionary<AxisDirection6,List<AxisDirection6>> link { get; set; }
    public GSARigid() { }
  }

  public enum AxisDirection6
  {
    NotSet = 0,
    X,
    Y,
    Z,
    XX,
    YY,
    ZZ
  }

  public enum RigidConstraint
  {
    NotSet = 0,
    ALL,
    XY_PLANE,
    YZ_PLANE,
    ZX_PLANE,
    XY_PLATE,
    YZ_PLATE,
    ZX_PLATE,
    PIN,
    XY_PLANE_PIN,
    YZ_PLANE_PIN,
    ZX_PLANE_PIN,
    XY_PLATE_PIN,
    YZ_PLATE_PIN,
    ZX_PLATE_PIN,
    Custom
  }

  public class GSAGenRest : Base
  {
    public int nativeId { get; set; }
    public string name { get; set; }
    public Restraint restraint { get; set; }
    public List<Node> nodes { get; set; }
    public List<GSAStage> stages { get; set; }
    public GSAGenRest() { }
  }

  public class GSAStage : Base
  {
    public int nativeId { get; set; }
    public string name { get; set; }
    public string colour { get; set; }
    public List<Base> elements { get; set; }
    public double creepFactor { get; set; }
    public int stageTime { get; set; } //number of days
    public List<Base> lockedElements { get; set; } //elements not part of the current analysis stage
    public GSAStage() { }
  }

}
