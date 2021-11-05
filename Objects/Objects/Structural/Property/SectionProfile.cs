using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Geometry;
using Objects.Structural.Materials;

namespace Objects.Structural.Properties.Profiles
{
    public class SectionProfile : Base
    {
        public string name { get; set; }
        public ShapeType shapeType { get; set; }
        public double weight { get; set; } // section weight, ex. kg/m
        public string units { get; set; }
        // optional nested section profiles
        public List<SectionProfile> children { get; set; } = new List<SectionProfile>();
        // optional derived section properties
        public SectionProperties sectionProperties { get; set; }
        // optional concrete parameters
        public List<ReinforcementBar> longitudinalBars { get; set; } = new List<ReinforcementBar>(); // the longitudinal reinforcement bars of the cross-section
        public List<ReinforcementLink> links { get; set; } = new List<ReinforcementLink>(); // the shear or torsion links of the cross-section
        public double cover { get; set; } // the concrete cover

        // deprecated 
        public double area { get; set; }
        public double Iyy { get; set; }
        public double Izz { get; set; }
        public double J { get; set; }
        public double Ky { get; set; }
        public double Kz { get; set; }

        public SectionProfile() { }

        // deprecated
        public SectionProfile(string name, ShapeType shapeType, double area, double Iyy, double Izz, double J, double Ky, double Kz, double weight)
        {
            this.name = name;
            this.shapeType = shapeType;
            this.area = area;
            this.Iyy = Izz;
            this.Izz = Izz;
            this.J = J;
            this.Ky = Ky;
            this.Kz = Kz;
            this.weight = weight;
        }

        public SectionProfile(string name)
        {
            this.name = name;
        }
    }

    public class Rectangular : SectionProfile
    {
        public double depth { get; set; }
        public double width { get; set; }
        public double webThickness { get; set; } // tw 
        public double flangeThickness { get; set; } // tf
        public Rectangular() { }

        [SchemaInfo("Rectangular", "Creates a Speckle structural rectangular section profile", "Structural", "Section Profile")]
        public Rectangular(string name, double depth, double width, double webThickness = 0, double flangeThickness = 0) : base(name)
        {
            this.depth = depth;
            this.width = width;
            this.webThickness = webThickness;
            this.flangeThickness = flangeThickness;
            this.shapeType = ShapeType.Rectangular;
        }
    }

    public class RectangularConcrete : Rectangular
    {
        public double topCover { get; set; } // the top concrete cover
        public double bottomCover { get; set; } // the bottom concrete cover
        public double leftCover { get; set; } // the left concrete cover
        public double rightCover { get; set; } // the right concrete cover

        public RectangularConcrete() { }

        [SchemaInfo("Rectangular", "Creates a Speckle structural rectangular concrete section profile with uniform cover", "Structural", "Section Profile")]
        public RectangularConcrete(string name, double depth, double width, double uniformCover) : base(name, depth, width, 0, 0)
        {
            this.topCover = uniformCover;
            this.bottomCover = uniformCover;
            this.leftCover = uniformCover;
            this.rightCover = uniformCover;
        }

        [SchemaInfo("Rectangular", "Creates a Speckle structural rectangular concrete section profile", "Structural", "Section Profile")]
        public RectangularConcrete(string name, double depth, double width, double topCover, double bottomCover, double leftCover, double rightCover) : base(name, depth, width, 0, 0)
        {
            this.topCover = topCover;
            this.bottomCover = bottomCover;
            this.leftCover = leftCover;
            this.rightCover = rightCover;
        }
    }

    public class Circular : SectionProfile
    {

        public double radius { get; set; }
        public double wallThickness { get; set; }

        public Circular() { }

        [SchemaInfo("Circular", "Creates a Speckle structural circular section profile", "Structural", "Section Profile")]
        public Circular(string name, double radius, double wallThickness = 0) : base(name)
        {
            this.radius = radius;
            this.wallThickness = wallThickness;
            this.shapeType = ShapeType.Circular;
        }
    }

    public class CircularConcrete : Circular
    {
        public double cover { get; set; } // the concrete cover

        public CircularConcrete()
        { }

        [SchemaInfo("Circular", "Creates a Speckle structural circular concrete section profile", "Structural", "Section Profile")]
        public CircularConcrete(string name, double radius, double cover) : base(name, radius, 0)
        {
            this.cover = cover;
        }
    }

    public class ISection : SectionProfile
    {
        public double depth { get; set; }
        public double width { get; set; }
        public double webThickness { get; set; }
        public double flangeThickness { get; set; }
        public double rootRadius { get; set; }

        public ISection() { }

        [SchemaInfo("ISection", "Creates a Speckle structural I section profile", "Structural", "Section Profile")]
        public ISection(string name, double depth, double width, double webThickness, double flangeThickness) : base(name)
        {
            this.depth = depth;
            this.width = width;
            this.webThickness = webThickness;
            this.flangeThickness = flangeThickness;
            this.shapeType = ShapeType.I;
        }
    }

    public class Tee : SectionProfile
    {
        public double depth { get; set; }
        public double width { get; set; }
        public double webThickness { get; set; }
        public double flangeThickness { get; set; }

        public Tee() { }

        [SchemaInfo("Tee", "Creates a Speckle structural Tee section profile", "Structural", "Section Profile")]
        public Tee(string name, double depth, double width, double webThickness, double flangeThickness) : base(name)
        {
            this.depth = depth;
            this.width = width;
            this.webThickness = webThickness;
            this.flangeThickness = flangeThickness;
            this.shapeType = ShapeType.Tee;
        }
    }

    public class Angle : SectionProfile
    {
        public double depth { get; set; }
        public double width { get; set; }
        public double webThickness { get; set; }
        public double flangeThickness { get; set; }

        public Angle() { }

        [SchemaInfo("Angle", "Creates a Speckle structural angle section profile", "Structural", "Section Profile")]
        public Angle(string name, double depth, double width, double webThickness, double flangeThickness) : base(name)
        {
            this.depth = depth;
            this.width = width;
            this.webThickness = webThickness;
            this.flangeThickness = flangeThickness;
            this.shapeType = ShapeType.Angle;
        }
    }

    public class Channel : SectionProfile
    {
        public double depth { get; set; }
        public double width { get; set; }
        public double webThickness { get; set; }
        public double flangeThickness { get; set; }

        public Channel() { }

        [SchemaInfo("Channel", "Creates a Speckle structural channel section profile", "Structural", "Section Profile")]
        public Channel(string name, double depth, double width, double webThickness, double flangeThickness) : base(name)
        {
            this.depth = depth;
            this.width = width;
            this.webThickness = webThickness;
            this.flangeThickness = flangeThickness;
            this.shapeType = ShapeType.Channel;
        }
    }

    public class Perimeter : SectionProfile
    {
        public ICurve outline { get; set; }
        public List<ICurve> voids { get; set; } = new List<ICurve>();

        public Perimeter() { }

        [SchemaInfo("Perimeter", "Creates a Speckle structural section profile defined by a perimeter curve and, if applicable, a list of void curves", "Structural", "Section Profile")]
        public Perimeter(string name, ICurve outline, List<ICurve> voids = null) : base(name)
        {
            this.outline = outline;
            this.voids = voids;
            this.shapeType = ShapeType.Perimeter;
        }
    }

    public class PerimeterConcrete : Perimeter
    {
        public List<double> covers { get; set; } = new List<double>();
        public PerimeterConcrete() { }

        [SchemaInfo("Perimeter", "Creates a Speckle structural concrete section profile defined by a perimeter curve and, if applicable, a list of void curves", "Structural", "Section Profile")]
        public PerimeterConcrete(string name, ICurve outline, List<double> covers, List<ICurve> voids = null) : base(name, outline, voids)
        {
            this.covers = covers;
        }
    }

    public class Catalogue : SectionProfile
    {
        public string description { get; set; } // a description string for a catalogue section, per a to be defined convention for industry-typical, commonly manufactured sections - SAF Formcodes, Oasys profiles?
        public string catalogueName { get; set; } // ex. AISC, could be enum value
        public string sectionType { get; set; } // ex. W shapes, could be enum value
        public string sectionName { get; set; } // ex. W44x335, could be enum value

        public Catalogue() { }

        [SchemaInfo("Catalogue (by description)", "Creates a Speckle structural section profile based on a catalogue section description", "Structural", "Section Profile")]
        public Catalogue(string description)
        {
            this.description = description;
            this.shapeType = ShapeType.Catalogue;
        }

        [SchemaInfo("Catalogue", "Creates a Speckle structural section profile", "Structural", "Section Profile")]
        public Catalogue(string name, string catalogueName, string sectionType, string sectionName) : base(name)
        {
            this.catalogueName = catalogueName;
            this.sectionType = sectionType;
            this.sectionName = sectionName;
            this.shapeType = ShapeType.Catalogue;
        }
    }

    // deprecated
    public class Explicit : SectionProfile
    {
        public Explicit() { }

        [SchemaInfo("Explicit", "Creates a Speckle structural section profile based on explicitly defining geometric properties", "Structural", "Section Profile")]
        public Explicit(string name, double area, double Iyy, double Izz, double J, double Ky, double Kz)
        {
            this.name = name;
            this.area = area;
            this.Iyy = Iyy;
            this.Izz = Izz;
            this.J = J;
            this.Ky = Ky;
            this.Kz = Kz;
            this.shapeType = ShapeType.Explicit;
        }
    }
}