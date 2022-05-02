namespace Speckle.SNAP.API.s8iSchema
{
  public class Section : ISnapRecordNamed
  {
    public string Name { get; set; }
    public SectionType SectionType { get; set; } = SectionType.HSection;
    public SectionRelevance SectionRelevance { get; set; } = SectionRelevance.EntireLength;
    public string CustomCatalogueField1 { get => CustomCatalogueFields[0]; set => CustomCatalogueFields[0] = value; }
    public string CustomCatalogueField2 { get => CustomCatalogueFields[1]; set => CustomCatalogueFields[1] = value; }
    public string CatalogueItemName { get; set; } = "";
    public double StandardDimensionWidth { get => StandardDimensions[0]; set => StandardDimensions[0] = value; }
    public double StandardDimensionDepth { get => StandardDimensions[1]; set => StandardDimensions[1] = value; }
    public double StandardDimensionWeb { get => StandardDimensions[2]; set => StandardDimensions[2] = value; }
    public double StandardDimensionFlange { get => StandardDimensions[3]; set => StandardDimensions[3] = value; }

    public string[] CustomCatalogueFields = new string[2] { "0", "0" };
    public string Material { get; set; } = "SS400";
    public double[] StandardDimensions = new double[4] { 0, 0, 0, 0 };
  }
}
