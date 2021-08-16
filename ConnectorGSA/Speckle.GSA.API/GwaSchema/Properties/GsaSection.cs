﻿using System.Collections.Generic;

namespace Speckle.GSA.API.GwaSchema
{
  public class GsaSection : GsaRecord_
  {
    public string Name { get => name; set { name = value; } }
    public Colour Colour = Colour.NO_RGB;
    public Section1dType Type;
    public int? PoolIndex;
    public ReferencePoint ReferencePoint;
    public double? RefY;
    public double? RefZ;
    public double? Mass;
    public double? Fraction;
    public double? Cost;
    public double? Left;
    public double? Right;
    public double? Slab;
    public List<GsaSectionComponentBase> Components;
    //This would be populated by the final value before environment variables, which is either ENVIRON or NO_ENVIRON
    //- this isn't implemented yet in the FromGwa case
    public bool Environ = false;

    public GsaSection() : base()
    {
      //Defaults
      Version = 7;
    }

    //Notes about the documentation:
    //- it leaves out the last 3 parameters
    //- mistakenly leaves out the pipe between 'slab' and 'num'
    //- the 'num' doesn't seem to represent the number of components at all (e.g. it is 1 even with 3 components), just whether there's at least one
    //- there is no mention of the last 3 arguments 0 | 0 | NO_ENVIRON/ENVIRON

    //SECTION.7 | ref | colour | name | memb | pool | point | refY | refZ | mass | fraction | cost | left | right | slab | num { < comp > } | 0 | 0 | NO_ENVIRON
    // where <comp> could be one or more of:
    //SECTION_COMP | ref | name | matAnal | matType | matRef | desc | offset_y | offset_z | rotn | reflect | pool
    //SECTION_CONC | ref | grade | agg
    //SECTION_STEEL | ref | grade | plasElas | netGross | exposed | beta | type | plate | lock
    //SECTION_LINK (not in documentation)
    //SECTION_COVER | ref | type:UNIFORM | cover | outer 
    //    or SECTION_COVER | ref | type:VARIABLE | top | bot | left | right | outer
    //    or SECTION_COVER | ref | type:FACE | num | face[] | outer
    //SECTION_TMPL (not in documentation except for a mention that is deprecated, despite being included in GWA generated by GSA 10.1
  }
}
