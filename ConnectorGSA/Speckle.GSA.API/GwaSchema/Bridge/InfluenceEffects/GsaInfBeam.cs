﻿namespace Speckle.GSA.API.GwaSchema
{
  public class GsaInfBeam : GsaRecord_
  {
    public string Name { get => name; set { name = value; } }
    public int? Action;
    public int? Element;
    public double? Position;
    public double? Factor;
    public InfType Type;
    public AxisDirection6 Direction;

    public GsaInfBeam() : base()
    {
      //Defaults
      Version = 2;
    }
  }
}
