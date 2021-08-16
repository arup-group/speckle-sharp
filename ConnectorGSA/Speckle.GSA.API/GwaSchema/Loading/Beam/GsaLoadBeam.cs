﻿using System.Collections.Generic;

namespace Speckle.GSA.API.GwaSchema
{
  public abstract class GsaLoadBeam : GsaRecord_
  {
    public string Name { get => name; set { name = value; } }
    public List<int> Entities = new List<int>();
    public int? LoadCaseIndex;
    public LoadBeamAxisRefType AxisRefType;
    public int? AxisIndex;
    public bool Projected;
    public AxisDirection6 LoadDirection;

    protected GwaKeyword childKeyword;
  }

  public class GsaLoadBeamPoint : GsaLoadBeam
  {
    public double Position;
    public double? Load;

    public GsaLoadBeamPoint() : base()
    {
      childKeyword = GwaKeyword.LOAD_BEAM_POINT;
      Version = 2;
    }
  }

  public class GsaLoadBeamUdl : GsaLoadBeam
  {
    public double? Load;

    public GsaLoadBeamUdl() : base()
    {
      childKeyword = GwaKeyword.LOAD_BEAM_UDL;
      Version = 2;
    }
  }

  public class GsaLoadBeamLine : GsaLoadBeam
  {
    public double? Load1;
    public double? Load2;

    public GsaLoadBeamLine() : base()
    {
      childKeyword = GwaKeyword.LOAD_BEAM_LINE;
      Version = 2;
    }
  }

  //This class is here simply to save on code as the code is the same except for the keyword.  If the syntax/schema for a future ever changes then
  //this will need to be refactored into separate classes again
  public abstract class GsaLoadBeamPatchTrilin : GsaLoadBeam
  {
    public double Position1;
    public double? Load1;
    public double Position2Percent;
    public double? Load2;
  }

  public class GsaLoadBeamPatch : GsaLoadBeamPatchTrilin
  {
    public GsaLoadBeamPatch() : base()
    {
      childKeyword = GwaKeyword.LOAD_BEAM_PATCH;
      Version = 2;
    }
  }

  public class GsaLoadBeamTrilin : GsaLoadBeamPatchTrilin
  {
    public GsaLoadBeamTrilin() : base()
    {
      childKeyword = GwaKeyword.LOAD_BEAM_TRILIN;
      Version = 2;
    }
  }
}
