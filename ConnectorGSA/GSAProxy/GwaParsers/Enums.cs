﻿using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  //These should be without version numbers
  public enum GwaKeyword
  {
    [StringValue("LOAD_NODE")]
    LOAD_NODE,
    [StringValue("NODE")]
    NODE,
    [StringValue("AXIS")]
    AXIS,
    [StringValue("LOAD_TITLE")]
    LOAD_TITLE,
    [StringValue("LOAD_GRID_AREA")]
    LOAD_GRID_AREA,
    [StringValue("GRID_SURFACE")]
    GRID_SURFACE,
    [StringValue("GRID_PLANE")]
    GRID_PLANE,
    [StringValue("POLYLINE")]
    POLYLINE,
    [StringValue("EL")]
    EL,
    [StringValue("MEMB")]
    MEMB,
    [StringValue("LOAD_BEAM")]
    LOAD_BEAM,
    [StringValue("LOAD_BEAM_POINT")]
    LOAD_BEAM_POINT,
    [StringValue("LOAD_BEAM_UDL")]
    LOAD_BEAM_UDL,
    [StringValue("LOAD_BEAM_LINE")]
    LOAD_BEAM_LINE,
    [StringValue("LOAD_BEAM_PATCH")]
    LOAD_BEAM_PATCH,
    [StringValue("LOAD_BEAM_TRILIN")]
    LOAD_BEAM_TRILIN,
    [StringValue("ASSEMBLY")]
    ASSEMBLY,
    [StringValue("SECTION")]
    SECTION,
    [StringValue("MAT_CONCRETE")]
    MAT_CONCRETE,
    [StringValue("MAT_STEEL")]
    MAT_STEEL,
    [StringValue("MAT")]
    MAT,
    [StringValue("MAT_ANAL")]
    MAT_ANAL,
    [StringValue("MAT_CURVE")]
    MAT_CURVE,
    [StringValue("MAT_CURVE_PARAM")]
    MAT_CURVE_PARAM,
    [StringValue("SECTION_COMP")]
    SECTION_COMP,
    [StringValue("SECTION_CONC")]
    SECTION_CONC,
    [StringValue("SECTION_STEEL")]
    SECTION_STEEL,
    [StringValue("SECTION_LINK")]
    SECTION_LINK,
    [StringValue("SECTION_COVER")]
    SECTION_COVER,
    [StringValue("SECTION_TMPL")]
    SECTION_TMPL,
    [StringValue("PROP_SPR")]
    PROP_SPR,
    [StringValue("PROP_2D")]
    PROP_2D,
    [StringValue("PROP_MASS")]
    PROP_MASS,
    [StringValue("ALIGN")]
    ALIGN,
    [StringValue("ANAL")]
    ANAL,
    [StringValue("TASK")]
    TASK,
    [StringValue("ANAL_STAGE")]
    ANAL_STAGE,
    [StringValue("ANAL_STAGE_PROP")]
    ANAL_STAGE_PROP,
    [StringValue("COMBINATION")]
    COMBINATION,
    [StringValue("GEN_REST")]
    GEN_REST,
    [StringValue("GRID_LINE")]
    GRID_LINE,
    [StringValue("INF_BEAM")]
    INF_BEAM,
    [StringValue("INF_NODE")]
    INF_NODE,
    [StringValue("LIST")]
    LIST,
    [StringValue("LOAD_1D_THERMAL")]
    LOAD_1D_THERMAL,
    [StringValue("LOAD_2D_FACE")]
    LOAD_2D_FACE,
    [StringValue("LOAD_2D_THERMAL")]
    LOAD_2D_THERMAL,
    [StringValue("LOAD_GRAVITY")]
    LOAD_GRAVITY,
    [StringValue("LOAD_GRID_LINE")]
    LOAD_GRID_LINE,
    [StringValue("LOAD_GRID_POINT")]
    LOAD_GRID_POINT,
    [StringValue("PATH")]
    PATH,
    [StringValue("PROP_SEC")]
    PROP_SEC,
    [StringValue("RIGID")]
    RIGID,
    [StringValue("USER_VEHICLE")]
    USER_VEHICLE,
    [StringValue("UNIT_DATA")]
    UNIT_DATA,
    [StringValue("TOL")]
    TOL,
    [StringValue("SPEC_STEEL_DESIGN")]
    SPEC_STEEL_DESIGN,
    [StringValue("SPEC_CONC_DESIGN")]
    SPEC_CONC_DESIGN
  }
}
