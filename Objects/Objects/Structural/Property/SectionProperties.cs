﻿using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Materials;

namespace Objects.Structural.Properties
{
    public class SectionProperties : Base
    {
        public double area { get; set; }
        public double Iy { get; set; } // seccond moment of area about y-axis
        public double Iz { get; set; } // seccond moment of area about z-axis
        public double J { get; set; } // st. venant torsional constant 
        public double Sy { get; set; } // elastic section modulus about y-axis
        public double Sz { get; set; } // elastic section modulus about z-axis
        public SectionProperties() { }

        [SchemaInfo("SectionProperties", "Creates Speckle structural section properties", "Structural", "Section Properties")]
        public SectionProperties(double area, double Iy, double Iz, double J, double Sz, double Sy)
        {
            this.area = area;
            this.Iy = Iy;
            this.Iz = Iz;
            this.J = J;
            this.Sy = Sy;
            this.Sz = Sz;
        }
    }

    public class SteelSectionProperties : SectionProperties
    {
        public double Sply { get; set; } // plastic section modulus about y-axis
        public double Splz { get; set; } // plastic section modulus about z-axis
        public double C { get; set; } // warping constant 
        public double ry { get; set; } // radius of gyration about y-axis
        public double rz { get; set; } // radius of gyration about z-axis
        public double y0 { get; set; } // distance from centroid to shear center in y-axis direction
        public double z0 { get; set; } // distance from centroid to shear center in z-axis direction
        public SteelSectionProperties() { }

        [SchemaInfo("SteelSectionProperties", "Creates Speckle structural steel section properties", "Structural", "Section Properties")]
        public SteelSectionProperties(double area, double Iy, double Iz, double J, double Sely, double Selz, double Sply, double Splz, double C, double ry, double rz, double y0 = 0.0, double z0 = 0.0) : base(area, Iy, Iz, J, Sely, Selz)
        {
            this.C = C;
            this.Sply = Sply;
            this.Splz = Splz;
            this.ry = ry;
            this.rz = rz;
            this.y0 = y0;
            this.z0 = z0;
        }
    }
}
