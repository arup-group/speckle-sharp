﻿namespace Speckle.GSA.API.GwaSchema
{
  //The term "section component" here is a name applied to both the group as a whole as well as one member of the group, 
  //but the latter is shortened to SectionComp to distinguish them here
  [GsaType(GwaKeyword.SECTION_LINK, GwaSetCommandType.Set, false, true, true)]
  public class SectionLink : GsaSectionComponentBase
  {
    public SectionLink() : base()
    {
      Version = 3;
    }
  }
}
