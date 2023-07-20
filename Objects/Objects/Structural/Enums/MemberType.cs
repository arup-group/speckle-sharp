using System.Collections.Generic;
using Objects.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Newtonsoft.Json;

namespace Objects.Structural.Geometry;

public enum MemberType
{
  NotSet = 0,
  Beam,
  Column,
  Generic1D,
  Slab,
  Wall,
  Generic2D,
  VoidCutter1D,
  VoidCutter2D //Brace
}

public enum MemberType2D
{
  Generic2D,
  Slab,
  Wall,
  VoidCutter2D
}

