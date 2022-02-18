using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;

namespace Objects.Structural.Geometry
{
    public class Restraint : Base
    {
        public string code { get; set; } //a string to describe the restraint type for each degree of freedom - ex. FFFRRR (pin) / FFFFFF (fix)

        public Restraint() { }

        [SchemaInfo("Restraint (by code)", "Creates a Speckle restraint object", "Structural", "Geometry")]
        public Restraint([SchemaParamInfo("A 6-character string to describe the restraint condition (F = Fixed, R = Released) for each degree of freedom - the first 3 characters represent translational degrees of freedom in the X, Y, and Z axes and the last 3 characters represent rotational degrees of freedom about the X, Y, and Z axes (ex. FFFRRR denotes a pinned condition, FFFFFF denotes a fixed condition)")] string code)
        {
            this.code = code.ToUpper();
        }

        [SchemaInfo("Restraint (by enum)", "Creates a Speckle restraint object (for pinned condition or fixed condition)", "Structural", "Geometry")]
        public Restraint(RestraintType restraintType)
        {
            if (restraintType == RestraintType.Free)
                this.code = "RRRRRR";
            if (restraintType == RestraintType.Pinned)
                this.code = "FFFRRR";
            if (restraintType == RestraintType.Fixed)
                this.code = "FFFFFF";
            if (restraintType == RestraintType.Roller)
                this.code = "RRFRRR";
        }
    }
}

