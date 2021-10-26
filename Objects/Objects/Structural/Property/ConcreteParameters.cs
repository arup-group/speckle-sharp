using System;
using System.Collections.Generic;
using System.Text;
using Objects.Structural.Materials;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace Objects.Structural.Properties
{
    public class ReinforcementBar
    {
        public double localY { get; set; } // local y-coordinate
        public double localZ { get; set; } // local z-coordinate
        public double diameter { get; set; } // diameter of bar or of single bar in bar bundle
        public string unit { get; set; }
        Material rebarMaterial { get; set; }
        public int countPerBundle { get; set; } // the number of bundled bars

        public ReinforcementBar() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localY">Local y-coordinate</param>
        /// <param name="localZ">Local z-coordinate</param>
        /// <param name="diameter">Diameter of bar</param>
        /// <param name="unit">Bar diameter unit</param>
        /// <param name="rebarMaterial"></param>
        /// 
        [SchemaInfo("Reinforcement Bar", "Creates a Speckle structural reinforcement bar", "Structural", "Section Profile")]
        public ReinforcementBar(double localY, double localZ, double diameter, string unit, Material rebarMaterial)
        {
            this.localY = localY;
            this.localZ = localZ;
            this.diameter = diameter;
            this.unit = unit;
            this.rebarMaterial = rebarMaterial;
            this.countPerBundle = 1;
        }
    }

    public class ReinforcementBundle : ReinforcementBar
    {
        public ReinforcementBundle() { }

        [SchemaInfo("Reinforcement Bundle", "Creates a structural reinforcement bundle", "Structural", "Section Profile")]
        public ReinforcementBundle(double localY, double localZ, double diameter, string unit, Material rebarMaterial, int countPerBundle) : base(localY, localZ, diameter, unit, rebarMaterial)
        {
            this.countPerBundle = countPerBundle;
        }
    }

    // is a differentiation between shear and torsion links necessary?
    public class ReinforcementLink
    {
        double diameter { get; set; } // diameter of bar
        double longitudinalSpacing { get; set; } // the longitudinal spacing of the links
        double transverseSpacing { get; set; } // the transverse spacing of the links
        public BaseReferencePoint referencePoint { get; set; }
        public double offsetY { get; set; } = 0; // offset from reference point
        public double offsetZ { get; set; } = 0; // offset from reference point
        public Material rebarMaterial { get; set; }

        public ReinforcementLink() { }

        [SchemaInfo("Reinforcement Link", "Creates a structural reinforcement link", "Structural", "Section Profile")]
        public ReinforcementLink(double diameter, double longitudinalSpacing)
        {
            this.diameter = diameter;
            this.longitudinalSpacing = longitudinalSpacing;
        }
    }
}
