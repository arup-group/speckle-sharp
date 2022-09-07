using Speckle.Core.Kits;
using Speckle.Core.Models;
using Objects.Structural.Properties;
using Objects.Structural.Materials;
using Objects.Structural.Geometry;

namespace Objects.Structural.GSA.Properties
{
  public class GSAProperty2D : Property2D
  {
    public int? nativeId { get; set; }
    public double? modifierInPlane { get; set; } // negative number is a percentage, positive number is a value
    public double? modifierBending { get; set; } // negative number is a percentage, positive number is a value
    public double? modifierShear { get; set; } // negative number is a percentage, positive number is a value
    public double? modifierVolume { get; set; } // negative number is a percentage, positive number is a value

    [DetachProperty]
    public StructuralMaterial designMaterial { get; set; }
    public double cost { get; set; }
    public double additionalMass { get; set; }
    public string concreteSlabProp { get; set; }
    public string colour { get; set; }
    public SupportType support { get; set; }
    public int? referenceEdge { get; set; }
    public GSAProperty2D() { }

    [SchemaInfo("GSAProperty2D", "Creates a Speckle structural 2D element property for GSA", "GSA", "Properties")]
    public GSAProperty2D(string name, StructuralMaterial material, double thickness, PropertyType2D type, ReferenceSurface referenceSurface = ReferenceSurface.Middle, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.material = material;
      this.thickness = thickness;
      this.type = type;
      this.refSurface = referenceSurface;
    }

    [SchemaInfo("GSAProperty2D (with modifiers)", "Creates a Speckle structural 2D element property for GSA", "GSA", "Properties")]
    public GSAProperty2D(string name, StructuralMaterial material, PropertyType2D type, double thickness,
      [SchemaParamInfo("Either an absolute value to use in place of the calculated stiffness or a percentage modifier to apply to the calculated stiffness (specify which using the modifierTypeInPlane parameter); ex. for percentage modifier, an input of 0.5 corresponds to a 50% modifier")] double? modifierValueInPlane = null,
      [SchemaParamInfo("Whether the in-plane stiffness modifier is an absolute value modifier (to replace the calculated value) or a perecent modifier (stiffness is modified to a percentage of the calculated value)")] ModifierType modifierTypeInPlane = ModifierType.ByPercentage,
      [SchemaParamInfo("Either an absolute value to use in place of the calculated stiffness or a percentage modifier to apply to the calculated stiffness (specify which using the modifierTypeBending parameter); ex. for percentage modifier, an input of 0.5 corresponds to a 50% modifier")] double? modifierValueBending = null,
      [SchemaParamInfo("Whether the bending stiffness modifier is an absolute value modifier (to replace the calculated value) or a perecent modifier (stiffness is modified to a percentage of the calculated value)")] ModifierType modifierTypeBending = ModifierType.ByPercentage,
      [SchemaParamInfo("Either an absolute value to use in place of the calculated stiffness or a percentage modifier to apply to the calculated stiffness (specify which using the modifierTypeShear parameter); ex. for percentage modifier, an input of 0.5 corresponds to a 50% modifier")] double? modifierValueShear = null,
      [SchemaParamInfo("Whether the shear stiffness modifier is an absolute value modifier (to replace the calculated value) or a perecent modifier (stiffness is modified to a percentage of the calculated value)")] ModifierType modifierTypeShear = ModifierType.ByPercentage,
      [SchemaParamInfo("Either an absolute value to use in place of the calculated stiffness or a percentage modifier to apply to the calculated stiffness (specify which using the modifierTypeVolume parameter); ex. for percentage modifier, an input of 0.5 corresponds to a 50% modifier")] double? modifierValueVolume = null,
      [SchemaParamInfo("Whether the volume modifier is an absolute value modifier (to replace the calculated value) or a perecent modifier (stiffness is modified to a percentage of the calculated value)")] ModifierType modifierTypeVolume = ModifierType.ByPercentage,
      double zOffset = 0, ReferenceSurface referenceSurface = ReferenceSurface.Middle, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.material = material;
      this.type = type;
      this.thickness = thickness;
      this.refSurface = referenceSurface;
      this.zOffset = zOffset;

      this.modifierInPlane = modifierTypeInPlane == ModifierType.ToAbsoluteValue ? modifierValueInPlane : -modifierValueInPlane;
      this.modifierBending = modifierTypeBending == ModifierType.ToAbsoluteValue ? modifierValueBending : -modifierValueBending;
      this.modifierShear = modifierTypeShear == ModifierType.ToAbsoluteValue ? modifierValueShear : -modifierValueShear;
      this.modifierVolume = modifierTypeVolume == ModifierType.ToAbsoluteValue ? modifierValueVolume : -modifierValueVolume;
    }
  }

  public enum ModifierType
  {
    ToAbsoluteValue,
    ByPercentage
  }

  public enum SupportType
  {
    Auto,
    AllSides,
    ThreeSides,
    TwoSides,
    TwoAdjacentSides,
    OneSide,
    Cantilever //cantilever
  }
}
