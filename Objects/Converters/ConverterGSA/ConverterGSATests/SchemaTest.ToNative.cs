using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.GSA.Materials;
using Objects.Structural.GSA.Properties;
using Objects.Structural.Geometry;
using Objects.Structural;
using Objects.Geometry;
using ConverterGSA;
using Objects.Structural.GSA.Analysis;
using Objects.Structural.GSA.Bridge;
using Objects.Structural.Loading;
using Objects.Structural.Properties.Profiles;
using Speckle.GSA.API.GwaSchema;
using Restraint = Objects.Structural.Geometry.Restraint;
using MemberType = Objects.Structural.Geometry.MemberType;
using GwaAxisDirection6 = Speckle.GSA.API.GwaSchema.AxisDirection6;
using Speckle.Core.Models;
using PathType = Objects.Structural.GSA.Bridge.PathType;
using Speckle.GSA.API;
using KellermanSoftware.CompareNetObjects;
using Objects.Structural.Analysis;
using Objects.Structural.GSA.Loading;
using Objects.Structural.Properties;
using AutoMapper;
using Objects.Structural.Materials;
using Speckle.Core.Kits;
using Objects.Structural.ApplicationSpecific.GSA.GeneralData;

namespace ConverterGSATests
{
  public partial class SchemaTest : SpeckleConversionFixture
  {
    //Reminder: conversions could create 1:1, 1:n, n:1, n:n structural per native objects

    #region Geometry
    [Fact]
    public void AxisToNative()
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      var gsaAxis = GsaAxisExample("axis 1");
      gsaRecords.Add(gsaAxis);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleObjects = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList());
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedAxis = gsaConvertedRecords.FindAll(r => r is GsaAxis).Select(r => (GsaAxis)r).First();

      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XDirX));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XDirY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XDirZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XYDirX));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XYDirY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAxis x) => x.XYDirZ));
      var result = compareLogic.Compare(gsaAxis, gsaConvertedAxis);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaAxis.XDirX.Value, gsaConvertedAxis.XDirX.Value, 6);
      Assert.Equal(gsaAxis.XDirY.Value, gsaConvertedAxis.XDirY.Value, 6);
      Assert.Equal(gsaAxis.XDirZ.Value, gsaConvertedAxis.XDirZ.Value, 6);
      Assert.Equal(gsaAxis.XYDirX.Value, gsaConvertedAxis.XYDirX.Value, 6);
      Assert.Equal(gsaAxis.XYDirY.Value, gsaConvertedAxis.XYDirY.Value, 6);
      Assert.Equal(gsaAxis.XYDirZ.Value, gsaConvertedAxis.XYDirZ.Value, 6);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void NodeToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      var gsaNodes = GsaNodeExamples(1, "node 1");
      gsaRecords.AddRange(gsaNodes);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.nodes.FindAll(o => o is GSANode).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedNodes = gsaConvertedRecords.FindAll(r => r is GsaNode).Select(r => (GsaNode)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaNode x) => x.X));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaNode x) => x.Y));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaNode x) => x.Z));
      var result = compareLogic.Compare(gsaNodes, gsaConvertedNodes);
      Assert.Empty(result.Differences);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      Assert.Equal(factor * gsaNodes[0].X, gsaConvertedNodes[0].X);
      Assert.Equal(factor * gsaNodes[0].Y, gsaConvertedNodes[0].Y);
      Assert.Equal(factor * gsaNodes[0].Z, gsaConvertedNodes[0].Z);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void Element2dToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      var gsaEls = GsaElement2dExamples(2, "element 1", "element 2");
      gsaRecords.AddRange(gsaEls);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAElement2D).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedEls = gsaConvertedRecords.FindAll(r => r is GsaEl).Select(r => (GsaEl)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.Angle));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.OffsetZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.ParentIndex));
      var result = compareLogic.Compare(gsaEls, gsaConvertedEls);
      Assert.Empty(result.Differences);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      Assert.Null(gsaConvertedEls[0].Angle);
      Assert.Null(gsaConvertedEls[0].OffsetZ);
      Assert.Null(gsaConvertedEls[0].ParentIndex);
      Assert.Equal(gsaEls[1].Angle, gsaConvertedEls[1].Angle); // always degrees (so no conversion)
      Assert.Equal(factor * gsaEls[1].OffsetZ, gsaConvertedEls[1].OffsetZ);
      Assert.Null(gsaConvertedEls[1].ParentIndex);
    }

    [Fact]
    public void Element1dBaseLineToNative()
    {
      var speckleElement = new GSAElement1D()
      {
        type = ElementType1D.Beam,
        baseLine = new Line(new List<double>() { 20, 21, 22, 50, 51, 52 }),
      };
      var gsaConvertedRecords = converter.ConvertToNative(new List<Base>() { speckleElement });
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void Element1dToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      var gsaEls = GsaElement1dExamples(2, "element 1", "element 2");
      gsaRecords.AddRange(gsaEls);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAElement1D).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedEls = gsaConvertedRecords.FindAll(r => r is GsaEl).Select(r => (GsaEl)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.OffsetY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.OffsetZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaEl x) => x.ParentIndex));
      var result = compareLogic.Compare(gsaEls, gsaConvertedEls);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      Assert.Empty(result.Differences);
      Assert.Null(gsaConvertedEls[0].OffsetY);
      Assert.Null(gsaConvertedEls[0].OffsetZ);
      Assert.Null(gsaConvertedEls[0].ParentIndex);
      Assert.Equal(factor * gsaEls[1].OffsetY, gsaConvertedEls[1].OffsetY);
      Assert.Equal(factor * gsaEls[1].OffsetZ, gsaConvertedEls[1].OffsetZ);
      Assert.Null(gsaConvertedEls[1].ParentIndex);
    }

    [Fact]
    public void GSAMember1dBaseLineToNative()
    {
      var speckleMember1d = new GSAMember1D()
      {
        memberType = MemberType.Beam,
        baseLine = new Line(new List<double>() { 20, 21, 22, 50, 51, 52 }),
      };
      var gsaConvertedRecords = converter.ConvertToNative(new List<Base>() { speckleMember1d });
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void GSAMemberToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5"));
      gsaRecords.Add(GsaProp2dExample("prop 2D 1"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      var gsaMembers = GsaMemberExamples(2, "member 1", "member 2");
      gsaRecords.AddRange(gsaMembers);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.Add(speckleDesignModel.elements.FindAll(o => o is GSAMember1D).First());
      speckleObjects.Add(speckleDesignModel.elements.FindAll(o => o is GSAMember2D).First());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedMembers = gsaConvertedRecords.FindAll(r => r is GsaMemb).Select(r => (GsaMemb)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.Angle));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.End1OffsetX));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.End2OffsetX));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.OffsetY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.OffsetZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.Offset2dZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMemb x) => x.MeshSize));
      var result = compareLogic.Compare(gsaMembers, gsaConvertedMembers);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaMembers[0].Angle, gsaConvertedMembers[0].Angle); //angle always in degrees
      Assert.Equal(factor * gsaMembers[0].End1OffsetX, gsaConvertedMembers[0].End1OffsetX);
      Assert.Null(gsaConvertedMembers[0].End2OffsetX);
      Assert.Equal(factor * gsaMembers[0].OffsetY, gsaConvertedMembers[0].OffsetY);
      Assert.Equal(factor * gsaMembers[0].OffsetZ, gsaConvertedMembers[0].OffsetZ);
      Assert.Equal(factor * gsaMembers[0].MeshSize, gsaConvertedMembers[0].MeshSize);
      Assert.Equal(gsaMembers[1].Angle, gsaConvertedMembers[1].Angle); //angle always in degrees
      Assert.Equal(factor * gsaMembers[1].Offset2dZ, gsaConvertedMembers[1].Offset2dZ);
      Assert.Equal(factor * gsaMembers[1].MeshSize, gsaConvertedMembers[1].MeshSize);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void AssemblyToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5"));
      gsaRecords.Add(GsaProp2dExample("section 1"));
      gsaRecords.AddRange(GsaElement2dExamples(2, "element 1", "element 2"));
      var gsaAssemblies = GsaAssemblyExamples(2, "assembly 1", "assembly 2");
      gsaRecords.AddRange(gsaAssemblies);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAAssembly).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedAssemblies = gsaConvertedRecords.FindAll(r => r is GsaAssembly).Select(r => (GsaAssembly)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAssembly x) => x.SizeY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAssembly x) => x.SizeZ));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAssembly x) => x.NumberOfPoints));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAssembly x) => x.Spacing));
      var result = compareLogic.Compare(gsaAssemblies, gsaConvertedAssemblies);
      Assert.Empty(result.Differences);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      for (var i = 0; i < gsaConvertedAssemblies.Count(); i++)
      {
        Assert.Equal(factor * gsaAssemblies[i].SizeY, gsaConvertedAssemblies[i].SizeY);
        Assert.Equal(factor * gsaAssemblies[i].SizeZ, gsaConvertedAssemblies[i].SizeZ);
      }
      Assert.Equal(gsaAssemblies[0].NumberOfPoints, gsaConvertedAssemblies[0].NumberOfPoints);
      Assert.Null(gsaConvertedAssemblies[0].Spacing);
      Assert.Null(gsaConvertedAssemblies[1].NumberOfPoints);
      Assert.Equal(factor * gsaAssemblies[1].Spacing, gsaConvertedAssemblies[1].Spacing);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void GridLineToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      var gsaGridLines = GsaGridLineExamples(2, "grid line 1", "grid line 2");
      gsaRecords.AddRange(gsaGridLines);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      Assert.Empty(converter.Report.ConversionErrors);
      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAGridLine).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedGridLines = gsaConvertedRecords.FindAll(r => r is GsaGridLine).Select(r => (GsaGridLine)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridLine x) => x.Theta1));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridLine x) => x.Theta2));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridLine x) => x.Length));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridLine x) => x.XCoordinate));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridLine x) => x.YCoordinate));
      var result = compareLogic.Compare(gsaGridLines, gsaConvertedGridLines);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      Assert.Empty(result.Differences);

      for (int i = 0; i < gsaConvertedGridLines.Count(); i++)
      {
        Assert.Equal(factor * gsaGridLines[i].Length, gsaConvertedGridLines[i].Length);
        Assert.Equal(factor * gsaGridLines[i].XCoordinate, gsaConvertedGridLines[i].XCoordinate);
        Assert.Equal(factor * gsaGridLines[i].YCoordinate, gsaConvertedGridLines[i].YCoordinate);
        if (gsaGridLines[i].Theta1.HasValue)
        {
          Assert.Equal(Math.Round(gsaGridLines[i].Theta1.Value, 4), Math.Round(gsaConvertedGridLines[i].Theta1.Value, 4));
        }
        if (gsaGridLines[i].Theta2.HasValue)
        {
          Assert.Equal(Math.Round(gsaGridLines[i].Theta2.Value, 4), Math.Round(gsaConvertedGridLines[i].Theta2.Value, 4));
        }
      }
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void GridPlaneToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      var gsaGridPlanes = GsaGridPlaneExamples(2, "grid plane 1", "grid plane 2");
      gsaRecords.AddRange(gsaGridPlanes);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      Assert.Empty(converter.Report.ConversionErrors);
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAGridPlane).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedGridPlanes = gsaConvertedRecords.FindAll(r => r is GsaGridPlane).Select(r => (GsaGridPlane)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridPlane x) => x.Elevation));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridPlane x) => x.StoreyToleranceBelow));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridPlane x) => x.StoreyToleranceAbove));
      var result = compareLogic.Compare(gsaGridPlanes, gsaConvertedGridPlanes);
      Assert.Empty(result.Differences);
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      for (var i = 0; i < gsaConvertedGridPlanes.Count(); i++)
      {
        Assert.Equal(factor * gsaGridPlanes[i].Elevation, gsaConvertedGridPlanes[i].Elevation);
        Assert.Equal(factor * gsaGridPlanes[i].StoreyToleranceBelow, gsaConvertedGridPlanes[i].StoreyToleranceBelow);
        Assert.Equal(factor * gsaGridPlanes[i].StoreyToleranceAbove, gsaConvertedGridPlanes[i].StoreyToleranceAbove);
      }
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void GridSurfaceToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(4, "node 1", "node 2", "node 3", "node 4"));
      gsaRecords.Add(GsaProp2dExample("prop 2D 1"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      var gsaElement2d = GsaElement2dExamples(1, "quad 1").FirstOrDefault();
      gsaElement2d.Index = 2;
      gsaRecords.Add(gsaElement2d);
      var gsaGridSurfaces = GsaGridSurfaceExamples(2, "grid surface 1", "grid surface 2");
      gsaRecords.AddRange(gsaGridSurfaces);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelAndResults;
      var speckleObjects = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList());

      //Get speckle results
      var speckleModelObjects = speckleObjects.FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModelObjects.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModelObjects.Where(so => so.layerDescription == "Analysis").FirstOrDefault();

      #region Design Layer
      var speckleDesignObjects = new List<Base>();
      speckleDesignObjects.AddRange(speckleDesignModel.elements.FindAll(so => so is GSAGridSurface).Select(so => (GSAGridSurface)so).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaDesignRecords = converter.ConvertToNative(speckleDesignObjects);

      //Checks
      var gsaDesignConverted = gsaDesignRecords.FindAll(r => r is GsaGridSurface).Select(r => (GsaGridSurface)r).ToList();
      var compareLogic = new CompareLogic();
      //Ignore Type because this context didn't create any Design Layer members, so the Model object for the design layer doesn't have
      //anything in its elements collection, so the ToNative of the Grid surface can't work out which type it is
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridSurface x) => x.Type));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridSurface x) => x.ElementIndices));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridSurface x) => x.Tolerance));
      var result = compareLogic.Compare(gsaGridSurfaces, gsaDesignConverted);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaGridSurfaces.Count(); i++)
      {
        Assert.Equal(factor * gsaGridSurfaces[i].Tolerance, gsaDesignConverted[i].Tolerance);
      }
      #endregion

      #region Analysis Layer
      var speckleAnalysisObjects = new List<Base>();
      speckleAnalysisObjects.AddRange(speckleAnalysisModel.elements.FindAll(so => so is GSAGridSurface).Select(so => (GSAGridSurface)so).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaAnalysisRecords = converter.ConvertToNative(speckleAnalysisObjects);

      //Checks
      var gsaAnalysisConverted = gsaAnalysisRecords.FindAll(r => r is GsaGridSurface).Select(r => (GsaGridSurface)r).ToList();
      compareLogic = new CompareLogic();  //Get new clear CompareLogic object with a fresh config without any ignores
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaGridSurface x) => x.Tolerance));
      result = compareLogic.Compare(gsaGridSurfaces, gsaAnalysisConverted);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaGridSurfaces.Count(); i++)
      {
        Assert.Equal(factor * gsaGridSurfaces[i].Tolerance, gsaAnalysisConverted[i].Tolerance);
      }
      #endregion
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData("mm", "mm")]
    [InlineData("mm", "m")]
    [InlineData("", "m")]
    [InlineData("m", "")]
    [InlineData(null, null)]
    public void PolylineToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      var factor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      var gsaPolylines = GsaPolylineExamples(2, "polyline 1", "polyline 2");
      gsaRecords.AddRange(gsaPolylines);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAPolyline).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedPolylines = gsaConvertedRecords.FindAll(r => r is GsaPolyline).Select(r => (GsaPolyline)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPolyline x) => x.Values));
      var result = compareLogic.Compare(gsaPolylines, gsaConvertedPolylines);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaPolylines.Count(); i++)
      {
        for (var j = 0; j < gsaPolylines[i].Values.Count(); j++)
        {
          Assert.Equal(factor * gsaPolylines[i].Values[j], gsaConvertedPolylines[i].Values[j]);
        }

      }
    }
    #endregion

    #region Loading
    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadCaseToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      var gsaLoadCases = GsaLoadCaseExamples(2, "load case 1", "load case 2");
      gsaRecords.AddRange(gsaLoadCases);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.loads.FindAll(o => o is GSALoadCase).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadCases = gsaConvertedRecords.FindAll(r => r is GsaLoadCase).Select(r => (GsaLoadCase)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadCase x) => x.Direction)); //Ignore app specific data
      var result = compareLogic.Compare(gsaLoadCases, gsaConvertedLoadCases);
      Assert.Empty(result.Differences);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadCaseToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      var gsaLoadCases = GsaLoadCaseExamples(2, "load case 1", "load case 2");
      gsaRecords.AddRange(gsaLoadCases);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleDesignModel.loads.FindAll(o => o is GSALoadCase).Select(o => (GSALoadCase)o).ToList();
      Map<GSALoadCase, LoadCase>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadCases = gsaConvertedRecords.FindAll(r => r is GsaLoadCase).Select(r => (GsaLoadCase)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadCase x) => x.Direction)); //Ignore app specific data
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadCase x) => x.Include));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadCase x) => x.Bridge));
      var result = compareLogic.Compare(gsaLoadCases, gsaConvertedLoadCases);
      Assert.Empty(result.Differences);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSAAnalysisCaseToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      var gsaAnalysisCases = GsaAnalysisCaseExamples(2, "analysis case 1", "analysis case 2");
      gsaRecords.AddRange(gsaAnalysisCases);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.loads.FindAll(o => o is GSAAnalysisCase).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedAnalysisCases = gsaConvertedRecords.FindAll(r => r is GsaAnal).Select(r => (GsaAnal)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAnal x) => x.TaskIndex)); //TODO - add in when this keyword is supported
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAnal x) => x.Desc));
      var result = compareLogic.Compare(gsaAnalysisCases, gsaConvertedAnalysisCases);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaAnalysisCases[0].Desc.RemoveWhitespace(), gsaConvertedAnalysisCases[0].Desc);
      Assert.Equal(gsaAnalysisCases[1].Desc.RemoveWhitespace(), gsaConvertedAnalysisCases[1].Desc);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSACombinationCaseToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.AddRange(GsaAnalysisCaseExamples(2, "analysis case 1", "analysis case 2"));
      var gsaCombinations = GsaCombinationExamples(2, "combo 1", "combo 2");
      gsaRecords.AddRange(gsaCombinations);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.loads.FindAll(o => o is GSACombinationCase).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedCombinations = gsaConvertedRecords.FindAll(r => r is GsaCombination).Select(r => (GsaCombination)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaCombination x) => x.Desc));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaCombination x) => x.Bridge));
      var result = compareLogic.Compare(gsaCombinations, gsaConvertedCombinations);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaConvertedCombinations[0].Desc.RemoveWhitespace(), gsaConvertedCombinations[0].Desc);
      Assert.False(gsaConvertedCombinations[0].Bridge);
      Assert.Equal(gsaConvertedCombinations[1].Desc.RemoveWhitespace(), gsaConvertedCombinations[1].Desc);
      Assert.False(gsaConvertedCombinations[1].Bridge);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadCombinationToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.AddRange(GsaAnalysisCaseExamples(2, "analysis case 1", "analysis case 2"));
      var gsaCombinations = GsaCombinationExamples(2, "combo 1", "combo 2");
      gsaRecords.AddRange(gsaCombinations);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleDesignModel.loads.FindAll(o => o is GSACombinationCase).Select(o => (GSACombinationCase)o).ToList();
      Map<GSACombinationCase, LoadCombination>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedCombinations = gsaConvertedRecords.FindAll(r => r is GsaCombination).Select(r => (GsaCombination)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaCombination x) => x.Desc));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaCombination x) => x.Note)); //Ignore app specific data
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaCombination x) => x.Bridge));
      var result = compareLogic.Compare(gsaCombinations, gsaConvertedCombinations);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaConvertedCombinations[0].Desc.RemoveWhitespace(), gsaConvertedCombinations[0].Desc);
      Assert.Equal(gsaConvertedCombinations[1].Desc.RemoveWhitespace(), gsaConvertedCombinations[1].Desc);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadFaceToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(6, "node 1", "node 2", "node 3", "node 4", "node 5", "node 6"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      gsaRecords.AddRange(GsaElement2dExamples(3, "element 1", "element 2", "element 3"));
      var gsaLoad2dFace = GsaLoad2dFaceExamples(3, "load 2d face 1", "load 2d face 2", "load 2d face 3");
      gsaRecords.AddRange(gsaLoad2dFace);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadFace).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoad2dFace = gsaConvertedRecords.FindAll(r => r is GsaLoad2dFace).Select(r => (GsaLoad2dFace)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoad2dFace x) => x.Values));
      var result = compareLogic.Compare(gsaLoad2dFace, gsaConvertedLoad2dFace);
      Assert.Empty(result.Differences);
      Assert.Equal(forceFactor / Math.Pow(lengthFactor, 2) * gsaLoad2dFace[0].Values[0], gsaConvertedLoad2dFace[0].Values[0]);
      Assert.Equal(forceFactor * gsaLoad2dFace[1].Values[0], gsaConvertedLoad2dFace[1].Values[0]);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadFaceToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(6, "node 1", "node 2", "node 3", "node 4", "node 5", "node 6"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      gsaRecords.AddRange(GsaElement2dExamples(3, "element 1", "element 2", "element 3"));
      var gsaLoad2dFace = GsaLoad2dFaceExamples(3, "load 2d face 1", "load 2d face 2", "load 2d face 3");
      gsaRecords.AddRange(gsaLoad2dFace);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleAnalysisModel.loads.FindAll(o => o is GSALoadFace).Select(o => (GSALoadFace)o).ToList();
      Map<GSALoadFace, LoadFace>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoad2dFace = gsaConvertedRecords.FindAll(r => r is GsaLoad2dFace).Select(r => (GsaLoad2dFace)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoad2dFace x) => x.Values));
      var result = compareLogic.Compare(gsaLoad2dFace, gsaConvertedLoad2dFace);
      Assert.Empty(result.Differences);
      Assert.Equal(forceFactor / Math.Pow(lengthFactor, 2) * gsaLoad2dFace[0].Values[0], gsaConvertedLoad2dFace[0].Values[0]);
      Assert.Equal(forceFactor * gsaLoad2dFace[1].Values[0], gsaConvertedLoad2dFace[1].Values[0]);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadBeamToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.AddRange(GsaElement1dExamples(2, "element 1", "element 2"));
      var gsaLoadBeams = GsaLoadBeamExamples(3, "load beam 1", "load beam 2", "load beam 3");
      gsaRecords.AddRange(gsaLoadBeams);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadBeam).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      #region GsaLoadBeamPoint
      var gsaPoint = gsaLoadBeams.FindAll(r => r is GsaLoadBeamPoint).Select(r => (GsaLoadBeamPoint)r).ToList();
      var gsaConvertedPoint = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamPoint).Select(r => (GsaLoadBeamPoint)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamPoint x) => x.Load));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamPoint x) => x.Position));
      var result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor * gsaPoint[0].Load, gsaConvertedPoint[0].Load);
      Assert.Equal(lengthFactor * gsaPoint[0].Position, gsaConvertedPoint[0].Position);
      #endregion
      #region GsaLoadBeamUdl
      var gsaUdl = gsaLoadBeams.FindAll(r => r is GsaLoadBeamUdl).Select(r => (GsaLoadBeamUdl)r).ToList();
      var gsaConvertedUdl = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamUdl).Select(r => (GsaLoadBeamUdl)r).ToList();
      compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamUdl x) => x.Load));
      result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor / lengthFactor * gsaUdl[0].Load, gsaConvertedUdl[0].Load);
      #endregion
      #region GsaLoadBeamLine
      var gsaLine = gsaLoadBeams.FindAll(r => r is GsaLoadBeamLine).Select(r => (GsaLoadBeamLine)r).ToList();
      var gsaConvertedLine = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamLine).Select(r => (GsaLoadBeamLine)r).ToList();
      compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamLine x) => x.Load1));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamLine x) => x.Load2));
      result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor / lengthFactor * gsaLine[0].Load1, gsaConvertedLine[0].Load1);
      Assert.Equal(forceFactor / lengthFactor * gsaLine[0].Load2, gsaConvertedLine[0].Load2);
      #endregion
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadBeamToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.AddRange(GsaElement1dExamples(2, "element 1", "element 2"));
      var gsaLoadBeams = GsaLoadBeamExamples(3, "load beam 1", "load beam 2", "load beam 3");
      gsaRecords.AddRange(gsaLoadBeams);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleAnalysisModel.loads.FindAll(o => o is GSALoadBeam).Select(o => (GSALoadBeam)o).ToList();
      Map<GSALoadBeam, LoadBeam>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      #region GsaLoadBeamPoint
      var gsaPoint = gsaLoadBeams.FindAll(r => r is GsaLoadBeamPoint).Select(r => (GsaLoadBeamPoint)r).ToList();
      var gsaConvertedPoint = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamPoint).Select(r => (GsaLoadBeamPoint)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamPoint x) => x.Load));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamPoint x) => x.Position));
      var result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor * gsaPoint[0].Load, gsaConvertedPoint[0].Load);
      Assert.Equal(lengthFactor * gsaPoint[0].Position, gsaConvertedPoint[0].Position);
      #endregion
      #region GsaLoadBeamUdl
      var gsaUdl = gsaLoadBeams.FindAll(r => r is GsaLoadBeamUdl).Select(r => (GsaLoadBeamUdl)r).ToList();
      var gsaConvertedUdl = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamUdl).Select(r => (GsaLoadBeamUdl)r).ToList();
      compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamUdl x) => x.Load));
      result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor / lengthFactor * gsaUdl[0].Load, gsaConvertedUdl[0].Load);
      #endregion
      #region GsaLoadBeamLine
      var gsaLine = gsaLoadBeams.FindAll(r => r is GsaLoadBeamLine).Select(r => (GsaLoadBeamLine)r).ToList();
      var gsaConvertedLine = gsaConvertedRecords.FindAll(r => r is GsaLoadBeamLine).Select(r => (GsaLoadBeamLine)r).ToList();
      compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamLine x) => x.Load1));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadBeamLine x) => x.Load2));
      result = compareLogic.Compare(gsaPoint, gsaConvertedPoint);
      Assert.Equal(forceFactor / lengthFactor * gsaLine[0].Load1, gsaConvertedLine[0].Load1);
      Assert.Equal(forceFactor / lengthFactor * gsaLine[0].Load2, gsaConvertedLine[0].Load2);
      #endregion
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadNodeToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      var gsaLoadNodes = GsaLoadNodeExamples(2, "load node 1", "load node 2");
      gsaRecords.AddRange(gsaLoadNodes);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.loads.FindAll(o => o is GSALoadNode).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadNodes = gsaConvertedRecords.FindAll(r => r is GsaLoadNode).Select(r => (GsaLoadNode)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadNode x) => x.Value));
      var result = compareLogic.Compare(gsaLoadNodes, gsaConvertedLoadNodes);
      Assert.Empty(result.Differences);
      Assert.Equal(forceFactor * gsaLoadNodes[0].Value, gsaConvertedLoadNodes[0].Value);
      Assert.Equal(forceFactor * lengthFactor * gsaLoadNodes[1].Value, gsaConvertedLoadNodes[1].Value);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadNodeToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      var gsaLoadNodes = GsaLoadNodeExamples(2, "load node 1", "load node 2");
      gsaRecords.AddRange(gsaLoadNodes);

      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleDesignModel.loads.FindAll(o => o is GSALoadNode).Select(o => (GSALoadNode)o).ToList();
      Map<GSALoadNode, LoadNode>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadNodes = gsaConvertedRecords.FindAll(r => r is GsaLoadNode).Select(r => (GsaLoadNode)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadNode x) => x.Value));
      var result = compareLogic.Compare(gsaLoadNodes, gsaConvertedLoadNodes);
      Assert.Empty(result.Differences);
      Assert.Equal(forceFactor * gsaLoadNodes[0].Value, gsaConvertedLoadNodes[0].Value);
      Assert.Equal(forceFactor * lengthFactor * gsaLoadNodes[1].Value, gsaConvertedLoadNodes[1].Value);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadGravityToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      gsaRecords.AddRange(GsaElement2dExamples(2, "element 1", "element 2"));
      var gsaLoadGravity = GsaLoadGravityExample("load gravity 1");
      gsaRecords.Add(gsaLoadGravity);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadGravity).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadGravity = gsaConvertedRecords.FindAll(r => r is GsaLoadGravity).Select(r => (GsaLoadGravity)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGravity x) => x.X));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGravity x) => x.Y));
      var result = compareLogic.Compare(gsaLoadGravity, gsaConvertedLoadGravity);
      Assert.Empty(result.Differences);
      Assert.Null(gsaConvertedLoadGravity.X);
      Assert.Null(gsaConvertedLoadGravity.Y);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void LoadGravityToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaLoadCaseExamples(1, "load case 1").First());
      gsaRecords.AddRange(GsaNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      gsaRecords.AddRange(GsaElement2dExamples(2, "element 1", "element 2"));
      var gsaLoadGravity = GsaLoadGravityExample("load gravity 1");
      gsaRecords.Add(gsaLoadGravity);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleAnalysisModel.loads.FindAll(o => o is GSALoadGravity).Select(o => (GSALoadGravity)o).ToList();
      Map<GSALoadGravity, LoadGravity>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadGravity = gsaConvertedRecords.FindAll(r => r is GsaLoadGravity).Select(r => (GsaLoadGravity)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGravity x) => x.X));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGravity x) => x.Y));
      //Ignore app specific data - currently no app specific members
      var result = compareLogic.Compare(gsaLoadGravity, gsaConvertedLoadGravity);
      Assert.Empty(result.Differences);
      Assert.Null(gsaConvertedLoadGravity.X);
      Assert.Null(gsaConvertedLoadGravity.Y);
    }

    [Theory]
    [InlineData("oC", "K")]
    [InlineData(null, null)]
    public void GSALoadThermal2dToNative(string gsaTemperatureUnit, string speckleTemperatureUnit)
    {
      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Temperature, Name = gsaTemperatureUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaProp2dExample("property 2D 1"));
      gsaRecords.AddRange(GsaNodeExamples(4, "node 1", "node 2", "node 3", "node 4"));
      gsaRecords.Add(GsaElement2dExamples(1, "element 1").FirstOrDefault());
      var gsaLoadThermals = GsaLoad2dThermalExamples(2, "load thermal 1", "load thermal 2");
      gsaRecords.AddRange(gsaLoadThermals);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadThermal2d).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.temperature = speckleTemperatureUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadThermals = gsaConvertedRecords.FindAll(r => r is GsaLoad2dThermal).Select(r => (GsaLoad2dThermal)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoad2dThermal x) => x.Values));
      var result = compareLogic.Compare(gsaLoadThermals, gsaConvertedLoadThermals);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaLoadThermals.Count(); i++)
      {
        for (var j = 0; j < gsaLoadThermals[i].Values.Count(); j++)
        {
          Assert.Equal(TemperatureUnits.Convert(gsaLoadThermals[i].Values[j], speckleTemperatureUnit, gsaTemperatureUnit), gsaConvertedLoadThermals[i].Values[j]);
        }
      }

    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadGridPointToNaitve(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      gsaRecords.Add(GsaGridSurfaceExamples(1, "grid surface 1").FirstOrDefault());
      var gsaLoadGridPoints = GsaLoadGridPointExamples(2, "load grid point 1", "load grid point 2");
      gsaRecords.AddRange(gsaLoadGridPoints);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadGridPoint).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadGrids = gsaConvertedRecords.FindAll(r => r is GsaLoadGridPoint).Select(r => (GsaLoadGridPoint)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridPoint x) => x.X));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridPoint x) => x.Y));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridPoint x) => x.Value));
      var result = compareLogic.Compare(gsaLoadGridPoints, gsaConvertedLoadGrids);
      Assert.Empty(result.Differences);
      Assert.Null(gsaConvertedLoadGrids[0].X);
      Assert.Null(gsaConvertedLoadGrids[0].Y);
      Assert.Equal(forceFactor * gsaLoadGridPoints[0].Value, gsaConvertedLoadGrids[0].Value);
      Assert.Null(gsaConvertedLoadGrids[1].X);
      Assert.Null(gsaConvertedLoadGrids[1].Y);
      Assert.Equal(forceFactor * gsaLoadGridPoints[1].Value, gsaConvertedLoadGrids[1].Value);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadGridLineToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaPolylineExamples(1, "polyline 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      gsaRecords.Add(GsaGridSurfaceExamples(1, "grid surface 1").FirstOrDefault());
      var gsaLoadGridLines = GsaLoadGridLineExamples(2, "load grid line 1", "load grid line 2");
      gsaRecords.AddRange(gsaLoadGridLines);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadGridLine).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadGrids = gsaConvertedRecords.FindAll(r => r is GsaLoadGridLine).Select(r => (GsaLoadGridLine)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridLine x) => x.Value1));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridLine x) => x.Value2));
      var result = compareLogic.Compare(gsaLoadGridLines, gsaConvertedLoadGrids);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaLoadGridLines.Count(); i++)
      {
        Assert.Equal(forceFactor / lengthFactor * gsaLoadGridLines[i].Value1, gsaConvertedLoadGrids[i].Value1);
        Assert.Equal(forceFactor / lengthFactor * gsaLoadGridLines[i].Value2, gsaConvertedLoadGrids[i].Value2);
      }
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData(null, null, null, null)]
    public void GSALoadGridAreaToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      gsaRecords.AddRange(GsaLoadCaseExamples(2, "load case 1", "load case 2"));
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaPolylineExamples(1, "polyline 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      gsaRecords.Add(GsaGridSurfaceExamples(1, "grid surface 1").FirstOrDefault());
      var gsaLoadGridAreas = GsaLoadGridAreaExamples(2, "load grid area 1", "load grid area 2");
      gsaRecords.AddRange(gsaLoadGridAreas);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.loads.FindAll(o => o is GSALoadGridArea).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedLoadGrids = gsaConvertedRecords.FindAll(r => r is GsaLoadGridArea).Select(r => (GsaLoadGridArea)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaLoadGridArea x) => x.Value));
      var result = compareLogic.Compare(gsaLoadGridAreas, gsaConvertedLoadGrids);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaLoadGridAreas.Count(); i++)
      {
        Assert.Equal(forceFactor / Math.Pow(lengthFactor, 2) * gsaLoadGridAreas[i].Value, gsaConvertedLoadGrids[i].Value);
      }
    }

    //TODO: add app agnostic test methods
    #endregion

    #region Materials
    [Theory]
    [InlineData("m", "kg", "oC", "Pa", "ε", "mm", "t", "oF", "MPa", "%ε")]
    [InlineData("m", null, null, null, null, "mm", null, null, null, null)]
    [InlineData(null, null, null, null, null, null, null, null, null, null)]
    public void GSASteelToNative(string gsaLengthUnit, string gsaMassUnit, string gsaTemperatureUnit, string gsaStressUnit, string gsaStrainUnit,
      string speckleLengthUnit, string speckleMassUnit, string speckleTemperatureUnit, string speckleStressUnit, string speckleStrainUnit)
    {
      # region unit conversion factors
      double lengthFactor = 1, massFactor = 1, thermalFactor = 1, stressFactor = 1, strainFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnit) && !string.IsNullOrEmpty(gsaLengthUnit))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      }
      if (!string.IsNullOrEmpty(speckleMassUnit) && !string.IsNullOrEmpty(gsaMassUnit))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnit, gsaMassUnit);
      }
      var densityFactor = massFactor / Math.Pow(lengthFactor, 3);
      if (!string.IsNullOrEmpty(speckleTemperatureUnit) && !string.IsNullOrEmpty(gsaTemperatureUnit))
      {
        thermalFactor = 1 / TemperatureUnits.GetTemperatureChangeConversionFactor(speckleTemperatureUnit, gsaTemperatureUnit);
      }
      if (!string.IsNullOrEmpty(speckleStressUnit) && !string.IsNullOrEmpty(gsaStressUnit))
      {
        stressFactor = StressUnits.GetConversionFactor(speckleStressUnit, gsaStressUnit);
      }
      if (!string.IsNullOrEmpty(speckleStrainUnit) && !string.IsNullOrEmpty(gsaStrainUnit))
      {
        strainFactor = StrainUnits.GetConversionFactor(speckleStrainUnit, gsaStrainUnit);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Temperature, Name = gsaTemperatureUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Stress, Name = gsaStressUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Strain, Name = gsaStrainUnit });
      var gsaMatSteel = GsaMatSteelExample("steel 1");
      gsaRecords.Add(gsaMatSteel);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.materials.FindAll(o => o is GSASteel).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.temperature = speckleTemperatureUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.stress = speckleStressUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.strain = speckleStrainUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedSteel = gsaConvertedRecords.FindAll(r => r is GsaMatSteel).Select(r => (GsaMatSteel)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.E));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.F));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.G));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Rho));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Alpha));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Eps));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.E));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.G));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.Rho));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.Alpha));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainElasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainElasticTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainPlasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainPlasticTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatSteel x) => x.Fy));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatSteel x) => x.Fu));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatSteel x) => x.Eh));
      var result = compareLogic.Compare(gsaMatSteel, gsaConvertedSteel);
      Assert.Empty(result.Differences);
      Assert.Equal(stressFactor * gsaMatSteel.Mat.E, gsaConvertedSteel.Mat.E);
      Assert.Equal(stressFactor * gsaMatSteel.Mat.F, gsaConvertedSteel.Mat.F);
      Assert.Equal(stressFactor * gsaMatSteel.Mat.G, gsaConvertedSteel.Mat.G);
      Assert.Equal(densityFactor * gsaMatSteel.Mat.Rho, gsaConvertedSteel.Mat.Rho);
      Assert.Equal(thermalFactor * gsaMatSteel.Mat.Alpha, gsaConvertedSteel.Mat.Alpha);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Eps, gsaConvertedSteel.Mat.Eps);
      Assert.Equal(stressFactor * gsaMatSteel.Mat.Prop.E, gsaConvertedSteel.Mat.Prop.E);
      Assert.Equal(stressFactor * gsaMatSteel.Mat.Prop.G, gsaConvertedSteel.Mat.Prop.G);
      Assert.Equal(densityFactor * gsaMatSteel.Mat.Prop.Rho, gsaConvertedSteel.Mat.Prop.Rho);
      Assert.Equal(thermalFactor * gsaMatSteel.Mat.Prop.Alpha, gsaConvertedSteel.Mat.Prop.Alpha);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainElasticCompression, gsaConvertedSteel.Mat.Uls.StrainElasticCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainElasticTension, gsaConvertedSteel.Mat.Uls.StrainElasticTension);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainPlasticCompression, gsaConvertedSteel.Mat.Uls.StrainPlasticCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainPlasticTension, gsaConvertedSteel.Mat.Uls.StrainPlasticTension);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainFailureCompression, gsaConvertedSteel.Mat.Uls.StrainFailureCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Uls.StrainFailureTension, gsaConvertedSteel.Mat.Uls.StrainFailureTension);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainElasticCompression, gsaConvertedSteel.Mat.Sls.StrainElasticCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainElasticTension, gsaConvertedSteel.Mat.Sls.StrainElasticTension);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainPlasticCompression, gsaConvertedSteel.Mat.Sls.StrainPlasticCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainPlasticTension, gsaConvertedSteel.Mat.Sls.StrainPlasticTension);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainFailureCompression, gsaConvertedSteel.Mat.Sls.StrainFailureCompression);
      Assert.Equal(strainFactor * gsaMatSteel.Mat.Sls.StrainFailureTension, gsaConvertedSteel.Mat.Sls.StrainFailureTension);
      Assert.Equal(stressFactor * gsaMatSteel.Fy, gsaConvertedSteel.Fy);
      Assert.Equal(stressFactor * gsaMatSteel.Fu, gsaConvertedSteel.Fu);
      Assert.Equal(stressFactor * gsaMatSteel.Eh, gsaConvertedSteel.Eh);
    }

    [Theory]
    [InlineData("m", "kg", "oC", "Pa", "ε", "mm", "t", "oF", "MPa", "%ε")]
    [InlineData("m", null, null, null, null, "mm", null, null, null, null)]
    [InlineData(null, null, null, null, null, null, null, null, null, null)]
    public void SteelToNative(string gsaLengthUnit, string gsaMassUnit, string gsaTemperatureUnit, string gsaStressUnit, string gsaStrainUnit,
      string speckleLengthUnit, string speckleMassUnit, string speckleTemperatureUnit, string speckleStressUnit, string speckleStrainUnit)
    {
      # region unit conversion factors
      double lengthFactor = 1, massFactor = 1, thermalFactor = 1, stressFactor = 1, strainFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnit) && !string.IsNullOrEmpty(gsaLengthUnit))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      }
      if (!string.IsNullOrEmpty(speckleMassUnit) && !string.IsNullOrEmpty(gsaMassUnit))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnit, gsaMassUnit);
      }
      var densityFactor = massFactor / Math.Pow(lengthFactor, 3);
      if (!string.IsNullOrEmpty(speckleTemperatureUnit) && !string.IsNullOrEmpty(gsaTemperatureUnit))
      {
        thermalFactor = 1 / TemperatureUnits.GetTemperatureChangeConversionFactor(speckleTemperatureUnit, gsaTemperatureUnit);
      }
      if (!string.IsNullOrEmpty(speckleStressUnit) && !string.IsNullOrEmpty(gsaStressUnit))
      {
        stressFactor = StressUnits.GetConversionFactor(speckleStressUnit, gsaStressUnit);
      }
      if (!string.IsNullOrEmpty(speckleStrainUnit) && !string.IsNullOrEmpty(gsaStrainUnit))
      {
        strainFactor = StrainUnits.GetConversionFactor(speckleStrainUnit, gsaStrainUnit);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Temperature, Name = gsaTemperatureUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Stress, Name = gsaStressUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Strain, Name = gsaStrainUnit });
      var gsaMatSteel = GsaMatSteelExample("steel 1");
      gsaRecords.Add(gsaMatSteel);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleDesignModel.materials.Select(o => (GSASteel)o).ToList();
      speckleAppSpecificObjects[0].yieldStrength /= stressFactor;
      speckleAppSpecificObjects[0].ultimateStrength /= stressFactor;
      speckleAppSpecificObjects[0].density /= densityFactor;
      speckleAppSpecificObjects[0].elasticModulus /= stressFactor;
      speckleAppSpecificObjects[0].shearModulus /= stressFactor;
      speckleAppSpecificObjects[0].thermalExpansivity /= thermalFactor;
      speckleAppSpecificObjects[0].strainHardeningModulus /= stressFactor;
      speckleAppSpecificObjects[0].maxStrain /= strainFactor;
      Map<GSASteel, Steel>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.temperature = speckleTemperatureUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.stress = speckleStressUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.strain = speckleStrainUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedSteel = gsaConvertedRecords.FindAll(r => r is GsaMatSteel).Select(r => (GsaMatSteel)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Name)); //Ignore app specific data
      var result = compareLogic.Compare(gsaMatSteel, gsaConvertedSteel);
      Assert.Empty(result.Differences);
    }

    [Theory]
    [InlineData("m", "kg", "oC", "Pa", "ε", "mm", "t", "oF", "MPa", "%ε")]
    [InlineData("m", null, null, null, null, "mm", null, null, null, null)]
    [InlineData(null, null, null, null, null, null, null, null, null, null)]
    public void GSAConcreteToNative(string gsaLengthUnit, string gsaMassUnit, string gsaTemperatureUnit, string gsaStressUnit, string gsaStrainUnit,
      string speckleLengthUnit, string speckleMassUnit, string speckleTemperatureUnit, string speckleStressUnit, string speckleStrainUnit)
    {
      # region unit conversion factors
      double lengthFactor = 1, massFactor = 1, thermalFactor = 1, stressFactor = 1, strainFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnit) && !string.IsNullOrEmpty(gsaLengthUnit))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      }
      if (!string.IsNullOrEmpty(speckleMassUnit) && !string.IsNullOrEmpty(gsaMassUnit))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnit, gsaMassUnit);
      }
      var densityFactor = massFactor / Math.Pow(lengthFactor, 3);
      if (!string.IsNullOrEmpty(speckleTemperatureUnit) && !string.IsNullOrEmpty(gsaTemperatureUnit))
      {
        thermalFactor = 1 / TemperatureUnits.GetTemperatureChangeConversionFactor(speckleTemperatureUnit, gsaTemperatureUnit);
      }
      if (!string.IsNullOrEmpty(speckleStressUnit) && !string.IsNullOrEmpty(gsaStressUnit))
      {
        stressFactor = StressUnits.GetConversionFactor(speckleStressUnit, gsaStressUnit);
      }
      if (!string.IsNullOrEmpty(speckleStrainUnit) && !string.IsNullOrEmpty(gsaStrainUnit))
      {
        strainFactor = StrainUnits.GetConversionFactor(speckleStrainUnit, gsaStrainUnit);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Temperature, Name = gsaTemperatureUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Stress, Name = gsaStressUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Strain, Name = gsaStrainUnit });
      var gsaMatConcrete = GsaMatConcreteExample("concrete 1");
      gsaRecords.Add(gsaMatConcrete);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.materials.FindAll(o => o is GSAConcrete).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.temperature = speckleTemperatureUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.stress = speckleStressUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.strain = speckleStrainUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedConcrete = gsaConvertedRecords.FindAll(r => r is GsaMatConcrete).Select(r => (GsaMatConcrete)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.E));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.F));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.G));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Rho));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Alpha));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Eps));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.E));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.G));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.Rho));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatAnal x) => x.Alpha));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainElasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainElasticTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainPlasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainPlasticTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureTension));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fc));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcdt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsU));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcd));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcdc));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcfib));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EmEs));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Emod));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Eps));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsPeak));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsMax));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsAx));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsTran));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsAxs));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Shrink));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Confine));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcc));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsPlasC));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.EpsUC));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Agg));
      var result = compareLogic.Compare(gsaMatConcrete, gsaConvertedConcrete);
      Assert.Empty(result.Differences);
      Assert.Equal(stressFactor * gsaMatConcrete.Mat.E, gsaConvertedConcrete.Mat.E);
      Assert.Equal(stressFactor * gsaMatConcrete.Mat.F, gsaConvertedConcrete.Mat.F);
      Assert.Equal(stressFactor * gsaMatConcrete.Mat.G, gsaConvertedConcrete.Mat.G);
      Assert.Equal(densityFactor * gsaMatConcrete.Mat.Rho, gsaConvertedConcrete.Mat.Rho);
      Assert.Equal(thermalFactor * gsaMatConcrete.Mat.Alpha, gsaConvertedConcrete.Mat.Alpha);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Eps, gsaConvertedConcrete.Mat.Eps);
      Assert.Equal(stressFactor * gsaMatConcrete.Mat.Prop.E, gsaConvertedConcrete.Mat.Prop.E);
      Assert.Equal(stressFactor * gsaMatConcrete.Mat.Prop.G, gsaConvertedConcrete.Mat.Prop.G);
      Assert.Equal(densityFactor * gsaMatConcrete.Mat.Prop.Rho, gsaConvertedConcrete.Mat.Prop.Rho);
      Assert.Equal(thermalFactor * gsaMatConcrete.Mat.Prop.Alpha, gsaConvertedConcrete.Mat.Prop.Alpha);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainElasticCompression, gsaConvertedConcrete.Mat.Uls.StrainElasticCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainElasticTension, gsaConvertedConcrete.Mat.Uls.StrainElasticTension);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainPlasticCompression, gsaConvertedConcrete.Mat.Uls.StrainPlasticCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainPlasticTension, gsaConvertedConcrete.Mat.Uls.StrainPlasticTension);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainFailureCompression, gsaConvertedConcrete.Mat.Uls.StrainFailureCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Uls.StrainFailureTension, gsaConvertedConcrete.Mat.Uls.StrainFailureTension);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainElasticCompression, gsaConvertedConcrete.Mat.Sls.StrainElasticCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainElasticTension, gsaConvertedConcrete.Mat.Sls.StrainElasticTension);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainPlasticCompression, gsaConvertedConcrete.Mat.Sls.StrainPlasticCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainPlasticTension, gsaConvertedConcrete.Mat.Sls.StrainPlasticTension);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainFailureCompression, gsaConvertedConcrete.Mat.Sls.StrainFailureCompression);
      Assert.Equal(strainFactor * gsaMatConcrete.Mat.Sls.StrainFailureTension, gsaConvertedConcrete.Mat.Sls.StrainFailureTension);
      Assert.Equal(stressFactor * gsaMatConcrete.Fc, gsaConvertedConcrete.Fc);
      Assert.Equal(stressFactor * gsaMatConcrete.Fcdt, gsaConvertedConcrete.Fcdt);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsU, gsaConvertedConcrete.EpsU);
      Assert.Equal(stressFactor * gsaMatConcrete.Fcd, gsaConvertedConcrete.Fcd);
      Assert.Equal(stressFactor * gsaMatConcrete.Fcdc, gsaConvertedConcrete.Fcdc);
      Assert.Equal(stressFactor * gsaMatConcrete.Fcfib, gsaConvertedConcrete.Fcfib);
      Assert.Equal(stressFactor * gsaMatConcrete.EmEs, gsaConvertedConcrete.EmEs);
      Assert.Equal(stressFactor * gsaMatConcrete.Emod, gsaConvertedConcrete.Emod);
      Assert.Equal(strainFactor * gsaMatConcrete.Eps, gsaConvertedConcrete.Eps);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsPeak, gsaConvertedConcrete.EpsPeak);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsMax, gsaConvertedConcrete.EpsMax);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsAx, gsaConvertedConcrete.EpsAx);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsTran, gsaConvertedConcrete.EpsTran);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsAxs, gsaConvertedConcrete.EpsAxs);
      Assert.Equal(strainFactor * gsaMatConcrete.Shrink, gsaConvertedConcrete.Shrink);
      Assert.Equal(stressFactor * gsaMatConcrete.Confine, gsaConvertedConcrete.Confine);
      Assert.Equal(stressFactor * gsaMatConcrete.Fcc, gsaConvertedConcrete.Fcc);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsPlasC, gsaConvertedConcrete.EpsPlasC);
      Assert.Equal(strainFactor * gsaMatConcrete.EpsUC, gsaConvertedConcrete.EpsUC);
      Assert.Equal(lengthFactor * gsaMatConcrete.Agg, gsaConvertedConcrete.Agg);
    }

    [Theory]
    [InlineData("m", "kg", "oC", "Pa", "ε", "mm", "t", "oF", "MPa", "%ε")]
    [InlineData("m", null, null, null, null, "mm", null, null, null, null)]
    [InlineData(null, null, null, null, null, null, null, null, null, null)]
    public void ConcreteToNative(string gsaLengthUnit, string gsaMassUnit, string gsaTemperatureUnit, string gsaStressUnit, string gsaStrainUnit,
      string speckleLengthUnit, string speckleMassUnit, string speckleTemperatureUnit, string speckleStressUnit, string speckleStrainUnit)
    {
      # region unit conversion factors
      double lengthFactor = 1, massFactor = 1, thermalFactor = 1, stressFactor = 1, strainFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnit) && !string.IsNullOrEmpty(gsaLengthUnit))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      }
      if (!string.IsNullOrEmpty(speckleMassUnit) && !string.IsNullOrEmpty(gsaMassUnit))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnit, gsaMassUnit);
      }
      var densityFactor = massFactor / Math.Pow(lengthFactor, 3);
      if (!string.IsNullOrEmpty(speckleTemperatureUnit) && !string.IsNullOrEmpty(gsaTemperatureUnit))
      {
        thermalFactor = 1 / TemperatureUnits.GetTemperatureChangeConversionFactor(speckleTemperatureUnit, gsaTemperatureUnit);
      }
      if (!string.IsNullOrEmpty(speckleStressUnit) && !string.IsNullOrEmpty(gsaStressUnit))
      {
        stressFactor = StressUnits.GetConversionFactor(speckleStressUnit, gsaStressUnit);
      }
      if (!string.IsNullOrEmpty(speckleStrainUnit) && !string.IsNullOrEmpty(gsaStrainUnit))
      {
        strainFactor = StrainUnits.GetConversionFactor(speckleStrainUnit, gsaStrainUnit);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Temperature, Name = gsaTemperatureUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Stress, Name = gsaStressUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Strain, Name = gsaStrainUnit });
      var gsaMatConcrete = GsaMatConcreteExample("concrete 1");
      gsaRecords.Add(gsaMatConcrete);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAppSpecificObjects = speckleDesignModel.materials.Select(o => (GSAConcrete)o).ToList();
      speckleAppSpecificObjects[0].compressiveStrength /= stressFactor;
      speckleAppSpecificObjects[0].tensileStrength /= stressFactor;
      speckleAppSpecificObjects[0].density /= densityFactor;
      speckleAppSpecificObjects[0].elasticModulus /= stressFactor;
      speckleAppSpecificObjects[0].shearModulus /= stressFactor;
      speckleAppSpecificObjects[0].thermalExpansivity /= thermalFactor;
      speckleAppSpecificObjects[0].maxCompressiveStrain /= strainFactor;
      speckleAppSpecificObjects[0].maxTensileStrain /= strainFactor;
      speckleAppSpecificObjects[0].maxAggregateSize /= lengthFactor;
      Map<GSAConcrete, Concrete>(speckleAppSpecificObjects, out var speckleAppAgnosticObjects);
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAppAgnosticObjects);
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.temperature = speckleTemperatureUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.stress = speckleStressUnit; //change the units
      speckleDesignModel.specs.settings.modelUnits.strain = speckleStrainUnit; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      Instance.GsaModel.Cache.Upsert(gsaRecords);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedConcrete = gsaConvertedRecords.FindAll(r => r is GsaMatConcrete).Select(r => (GsaMatConcrete)r).First();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMat x) => x.Name)); //Ignore app specific data
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatConcrete x) => x.Fcfib));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainElasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainPlasticCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureCompression));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaMatCurveParam x) => x.StrainFailureTension));
      var result = compareLogic.Compare(gsaMatConcrete, gsaConvertedConcrete);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaMatConcrete.Fcfib.Value, gsaConvertedConcrete.Fcfib.Value, 3);
      Assert.Equal(gsaMatConcrete.Mat.Uls.StrainElasticCompression.Value, gsaConvertedConcrete.Mat.Uls.StrainElasticCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Uls.StrainPlasticCompression.Value, gsaConvertedConcrete.Mat.Uls.StrainPlasticCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Uls.StrainFailureCompression.Value, gsaConvertedConcrete.Mat.Uls.StrainFailureCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Uls.StrainFailureTension.Value, gsaConvertedConcrete.Mat.Uls.StrainFailureTension.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Sls.StrainElasticCompression.Value, gsaConvertedConcrete.Mat.Sls.StrainElasticCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Sls.StrainPlasticCompression.Value, gsaConvertedConcrete.Mat.Sls.StrainPlasticCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Sls.StrainFailureCompression.Value, gsaConvertedConcrete.Mat.Sls.StrainFailureCompression.Value, 9);
      Assert.Equal(gsaMatConcrete.Mat.Sls.StrainFailureTension.Value, gsaConvertedConcrete.Mat.Sls.StrainFailureTension.Value, 9);
    }
    #endregion

    #region Properties
    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void Property1dToNative2(string gsaDisplacementUnits, string speckleDisplacementUnits)
    {
      # region unit conversion factors
      double displacementFactor = 1;
      if (!string.IsNullOrEmpty(speckleDisplacementUnits) && !string.IsNullOrEmpty(gsaDisplacementUnits))
      {
        displacementFactor = Units.GetConversionFactor(speckleDisplacementUnits, gsaDisplacementUnits);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Displacements, Name = gsaDisplacementUnits });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      var gsaSections = new List<GsaSection>
      {
        GsaCatalogueSectionExample("section 1"),
        GsaExplicitSectionExample("section 2"),
        GsaPerimeterSectionExample("section 3"),
        GsaRectangularSectionExample("section 4"),
        GsaRectangularHollowSectionExample("section 5"),
        GsaCircularSectionExample("section 6"),
        GsaCircularHollowSectionExample("section 7"),
        GsaISectionSectionExample("section 8"),
        GsaTSectionSectionExample("section 9"),
        GsaAngleSectionExample("section 10"),
        GsaChannelSectionExample("section 11")
      };
      gsaRecords.AddRange(gsaSections);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      Assert.Empty(converter.Report.ConversionErrors);
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(r => (object)r).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.properties.FindAll(o => o is GSAProperty1D).ToList());
      speckleDesignModel.specs.settings.modelUnits.displacements = speckleDisplacementUnits; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedSections = gsaConvertedRecords.FindAll(r => r is GsaSection).Select(r => (GsaSection)r).ToList();
      var compareLogic = new CompareLogic();
      #region members to ignore
      compareLogic.Config.MembersToIgnore.Add("SectionSteel.Type");
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsExplicit x) => x.Area));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsExplicit x) => x.Iyy));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsExplicit x) => x.Izz));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsExplicit x) => x.J));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsPerimeter x) => x.Y));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsPerimeter x) => x.Z));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectangular x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectangular x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTwoThickness x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTwoThickness x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTwoThickness x) => x.tw));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTwoThickness x) => x.tf));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCircular x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCircularHollow x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCircularHollow x) => x.t));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaper x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaper x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaper x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsEllipse x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsEllipse x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsEllipse x) => x.k));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.tw));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.tfb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsGeneralI x) => x.tft));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperTAngle x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperTAngle x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperTAngle x) => x.twt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperTAngle x) => x.twb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperTAngle x) => x.tf));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectoEllipse x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectoEllipse x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectoEllipse x) => x.df));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectoEllipse x) => x.bf));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsRectoEllipse x) => x.k));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.twt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.twb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.tft));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsTaperI x) => x.tfb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSecant x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSecant x) => x.c));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSecant x) => x.n));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsOval x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsOval x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsOval x) => x.t));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.dt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.db));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsZ x) => x.t));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsC x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsC x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsC x) => x.dt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsC x) => x.t));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.tw));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.tf));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.ds));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsCastellatedCellular x) => x.p));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.dt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.twt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.tft));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.db));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.twb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.tfb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.ds));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsAsymmetricCellular x) => x.p));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.d));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.b));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.bt));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.bb));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.tf));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((ProfileDetailsSheetPile x) => x.tw));
      #endregion
      var result = compareLogic.Compare(gsaSections, gsaConvertedSections);
      Assert.Empty(result.Differences);

      var i = 0;
      #region catalogue
      //No scaling required, therefore no additional checks required.
      i++;
      #endregion
      #region explicit
      var oldExplicit = (ProfileDetailsExplicit)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newExplicit = (ProfileDetailsExplicit)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldExplicit.Area * Math.Pow(displacementFactor, 2), newExplicit.Area);
      Assert.Equal(oldExplicit.Iyy * Math.Pow(displacementFactor, 4), newExplicit.Iyy);
      Assert.Equal(oldExplicit.Izz * Math.Pow(displacementFactor, 4), newExplicit.Izz);
      Assert.Equal(oldExplicit.J * Math.Pow(displacementFactor, 4), newExplicit.J);
      i++;
      #endregion
      #region perimeter
      var oldPerimeter = (ProfileDetailsPerimeter)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newPerimeter = (ProfileDetailsPerimeter)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldPerimeter.Y.Select(v => v * displacementFactor).ToList(), newPerimeter.Y);
      Assert.Equal(oldPerimeter.Z.Select(v => v * displacementFactor).ToList(), newPerimeter.Z);
      i++;
      #endregion
      #region rectangle
      var oldRect = (ProfileDetailsRectangular)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newRect = (ProfileDetailsRectangular)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldRect.b * displacementFactor, newRect.b);
      Assert.Equal(oldRect.d * displacementFactor, newRect.d);
      i++;
      #endregion
      #region rhs
      var oldRhs = (ProfileDetailsTwoThickness)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newRhs = (ProfileDetailsTwoThickness)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldRhs.b * displacementFactor, newRhs.b);
      Assert.Equal(oldRhs.d * displacementFactor, newRhs.d);
      Assert.Equal(oldRhs.tf * displacementFactor, newRhs.tf);
      Assert.Equal(oldRhs.tw * displacementFactor, newRhs.tw);
      i++;
      #endregion
      #region circular
      var oldCirc = (ProfileDetailsCircular)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newCirc = (ProfileDetailsCircular)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldCirc.d * displacementFactor, newCirc.d);
      i++;
      #endregion
      #region chs
      var oldChs = (ProfileDetailsCircularHollow)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newChs = (ProfileDetailsCircularHollow)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldChs.d * displacementFactor, newChs.d);
      Assert.Equal(oldChs.t * displacementFactor, newChs.t);
      i++;
      #endregion
      #region I
      var oldI = (ProfileDetailsTwoThickness)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newI = (ProfileDetailsTwoThickness)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldI.b * displacementFactor, newI.b);
      Assert.Equal(oldI.d * displacementFactor, newI.d);
      Assert.Equal(oldI.tf * displacementFactor, newI.tf);
      Assert.Equal(oldI.tw * displacementFactor, newI.tw);
      i++;
      #endregion
      #region T
      var oldT = (ProfileDetailsTwoThickness)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newT = (ProfileDetailsTwoThickness)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldT.b * displacementFactor, newT.b);
      Assert.Equal(oldT.d * displacementFactor, newT.d);
      Assert.Equal(oldT.tf * displacementFactor, newT.tf);
      Assert.Equal(oldT.tw * displacementFactor, newT.tw);
      i++;
      #endregion
      #region angle
      var oldAng = (ProfileDetailsTwoThickness)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newAng = (ProfileDetailsTwoThickness)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldAng.b * displacementFactor, newAng.b);
      Assert.Equal(oldAng.d * displacementFactor, newAng.d);
      Assert.Equal(oldAng.tf * displacementFactor, newAng.tf);
      Assert.Equal(oldAng.tw * displacementFactor, newAng.tw);
      i++;
      #endregion
      #region channel
      var oldChl = (ProfileDetailsTwoThickness)((SectionComp)gsaSections[i].Components[0]).ProfileDetails;
      var newChl = (ProfileDetailsTwoThickness)((SectionComp)gsaConvertedSections[i].Components[0]).ProfileDetails;
      Assert.Equal(oldChl.b * displacementFactor, newChl.b);
      Assert.Equal(oldChl.d * displacementFactor, newChl.d);
      Assert.Equal(oldChl.tf * displacementFactor, newChl.tf);
      Assert.Equal(oldChl.tw * displacementFactor, newChl.tw);
      i++;
      #endregion
    }

    [Theory]
    [InlineData("m", "m", "m", "kg", "mm", "cm", "m", "g")]
    [InlineData("m", null, null, null, "mm", null, null, null)]
    [InlineData(null, null, null, null, null, null, null, null)]
    public void Property2dToNative(string gsaLengthUnits, string gsaDisplacementUnits, string gsaSectionUnits, string gsaMassUnits,
      string speckleLengthUnits, string speckleDisplacementUnits, string speckleSectionUnits, string speckleMassUnits)
    {
      # region unit conversion factors
      double lengthFactor = 1, displacementFactor = 1, sectionFactor = 1, massFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnits) && !string.IsNullOrEmpty(gsaLengthUnits))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnits, gsaLengthUnits);
      }
      if (!string.IsNullOrEmpty(speckleDisplacementUnits) && !string.IsNullOrEmpty(gsaDisplacementUnits))
      {
        displacementFactor = Units.GetConversionFactor(speckleDisplacementUnits, gsaDisplacementUnits);
      }
      if (!string.IsNullOrEmpty(speckleSectionUnits) && !string.IsNullOrEmpty(gsaSectionUnits))
      {
        sectionFactor = Units.GetConversionFactor(speckleSectionUnits, gsaSectionUnits);
      }
      if (!string.IsNullOrEmpty(speckleMassUnits) && !string.IsNullOrEmpty(gsaMassUnits))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnits, gsaMassUnits);
      }
      var thicknessFactor = displacementFactor;
      var inPlaneFactor = Math.Pow(displacementFactor, 2) / lengthFactor;
      var bendingFactor = Math.Pow(sectionFactor, 4) / lengthFactor;
      var shearFactor = Math.Pow(displacementFactor, 2) / lengthFactor;
      var volumeFactor = Math.Pow(sectionFactor, 3) / Math.Pow(lengthFactor, 2);
      massFactor /= Math.Pow(lengthFactor, 2);
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnits });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Displacements, Name = gsaDisplacementUnits });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Sections, Name = gsaSectionUnits });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnits });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      var gsaProp2d = GsaProp2dExample("property 2D 1");
      gsaRecords.Add(gsaProp2d);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      Assert.Empty(converter.Report.ConversionErrors);
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.properties.FindAll(o => o is GSAProperty2D).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnits; //change the units
      speckleDesignModel.specs.settings.modelUnits.displacements = speckleDisplacementUnits; //change the units
      speckleDesignModel.specs.settings.modelUnits.sections = speckleSectionUnits; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnits; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedProp2d = (GsaProp2d)gsaConvertedRecords.FirstOrDefault(n => n is GsaProp2d);
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.Thickness));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.Mass));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.InPlane));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.Bending));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.Shear));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.Volume));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.InPlaneStiffnessPercentage));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.BendingStiffnessPercentage));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.ShearStiffnessPercentage));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaProp2d x) => x.VolumePercentage));
      var result = compareLogic.Compare(gsaProp2d, gsaConvertedProp2d);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaProp2d.Thickness * thicknessFactor, gsaConvertedProp2d.Thickness);
      Assert.Equal(gsaProp2d.Mass * massFactor, gsaConvertedProp2d.Mass);
      Assert.Equal(gsaProp2d.InPlane * inPlaneFactor, gsaConvertedProp2d.InPlane);
      Assert.Equal(gsaProp2d.Bending * bendingFactor, gsaConvertedProp2d.Bending);
      Assert.Equal(gsaProp2d.Shear * shearFactor, gsaConvertedProp2d.Shear);
      Assert.Equal(gsaProp2d.Volume * volumeFactor, gsaConvertedProp2d.Volume);
      Assert.Equal(gsaProp2d.InPlaneStiffnessPercentage, gsaConvertedProp2d.InPlaneStiffnessPercentage);
      Assert.Equal(gsaProp2d.BendingStiffnessPercentage, gsaConvertedProp2d.BendingStiffnessPercentage);
      Assert.Equal(gsaProp2d.ShearStiffnessPercentage, gsaConvertedProp2d.ShearStiffnessPercentage);
      Assert.Equal(gsaProp2d.VolumePercentage, gsaConvertedProp2d.VolumePercentage);
    }

    [Theory]
    [InlineData("m", "kg", "mm", "g")]
    [InlineData("m", null, "mm", null)]
    [InlineData(null, null, null, null)]
    public void PropertyMassToNative(string gsaLengthUnits, string gsaMassUnits, string speckleLengthUnits, string speckleMassUnits)
    {
      # region unit conversion factors
      double lengthFactor = 1, massFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnits) && !string.IsNullOrEmpty(gsaLengthUnits))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnits, gsaLengthUnits);
      }
      if (!string.IsNullOrEmpty(speckleMassUnits) && !string.IsNullOrEmpty(gsaMassUnits))
      {
        massFactor = MassUnits.GetConversionFactor(speckleMassUnits, gsaMassUnits);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnits });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Mass, Name = gsaMassUnits });
      var gsaPropMass = GsaPropMassExample("property mass 1");
      gsaRecords.Add(gsaPropMass);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      Assert.Empty(converter.Report.ConversionErrors);
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.properties.FindAll(o => o is PropertyMass).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnits; //change the units
      speckleDesignModel.specs.settings.modelUnits.mass = speckleMassUnits; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedPropMass = (GsaPropMass)gsaConvertedRecords.FirstOrDefault(n => n is GsaPropMass);
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Mass));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Ixx));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Iyy));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Izz));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Ixy));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Iyz));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.Izx));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.ModX));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.ModY));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropMass x) => x.ModZ));
      var result = compareLogic.Compare(gsaPropMass, gsaConvertedPropMass);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaPropMass.Mass * massFactor, gsaConvertedPropMass.Mass);
      Assert.Equal(gsaPropMass.Ixx * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Ixx);
      Assert.Equal(gsaPropMass.Iyy * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Iyy);
      Assert.Equal(gsaPropMass.Izz * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Izz);
      Assert.Equal(gsaPropMass.Ixy * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Ixy);
      Assert.Equal(gsaPropMass.Iyz * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Iyz);
      Assert.Equal(gsaPropMass.Izx * massFactor * Math.Pow(lengthFactor, 2), gsaConvertedPropMass.Izx);
      if (gsaPropMass.ModX > 0) Assert.Equal(gsaPropMass.ModX * massFactor, gsaConvertedPropMass.ModX);
      else Assert.Equal(gsaPropMass.ModX, gsaConvertedPropMass.ModX);
      if (gsaPropMass.ModY > 0) Assert.Equal(gsaPropMass.ModY * massFactor, gsaConvertedPropMass.ModY);
      else Assert.Equal(gsaPropMass.ModY, gsaConvertedPropMass.ModY);
      if (gsaPropMass.ModZ > 0) Assert.Equal(gsaPropMass.ModZ * massFactor, gsaConvertedPropMass.ModZ);
      else Assert.Equal(gsaPropMass.ModZ, gsaConvertedPropMass.ModZ);
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData("m", null, "mm", null)]
    [InlineData(null, null, null, null)]
    public void PropertySpringToNative(string gsaLengthUnits, string gsaForceUnits, string speckleLengthUnits, string speckleForceUnits)
    {
      # region unit conversion factors
      double lengthFactor = 1, forceFactor = 1;
      if (!string.IsNullOrEmpty(speckleLengthUnits) && !string.IsNullOrEmpty(gsaLengthUnits))
      {
        lengthFactor = Units.GetConversionFactor(speckleLengthUnits, gsaLengthUnits);
      }
      if (!string.IsNullOrEmpty(speckleForceUnits) && !string.IsNullOrEmpty(gsaForceUnits))
      {
        forceFactor = ForceUnits.GetConversionFactor(speckleForceUnits, gsaForceUnits);
      }
      #endregion

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnits });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnits });
      var gsaProSpr = GsaPropSprExample("property spring 1");
      gsaRecords.Add(gsaProSpr);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      Assert.Empty(converter.Report.ConversionErrors);
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleDesignModel.properties.FindAll(o => o is PropertySpring).ToList());
      speckleDesignModel.specs.settings.modelUnits.length = speckleLengthUnits; //change the units
      speckleDesignModel.specs.settings.modelUnits.force = speckleForceUnits; //change the units
      speckleObjects.Add(speckleDesignModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedProSpr = (GsaPropSpr)gsaConvertedRecords.FirstOrDefault(n => n is GsaPropSpr);
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPropSpr x) => x.Stiffnesses));
      var result = compareLogic.Compare(gsaProSpr, gsaConvertedProSpr);
      Assert.Empty(result.Differences);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.X] * forceFactor / lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.X]);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.Y] * forceFactor / lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.Y]);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.Z] * forceFactor / lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.Z]);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.XX] * forceFactor * lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.XX]);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.YY] * forceFactor * lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.YY]);
      Assert.Equal(gsaProSpr.Stiffnesses[GwaAxisDirection6.ZZ] * forceFactor * lengthFactor, gsaConvertedProSpr.Stiffnesses[GwaAxisDirection6.ZZ]);
    }

    #endregion

    #region Constraints
    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSARigidConstraintToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.AddRange(GsaElement1dExamples(2, "element 1", "element 2"));
      gsaRecords.Add(GsaAnalStageExamples(1, "stage 1").FirstOrDefault());
      var gsaRigids = GsaRigidExamples(2, "rigid 1", "rigid 2");
      gsaRecords.AddRange(gsaRigids);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSARigidConstraint).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedRigids = gsaConvertedRecords.FindAll(r => r is GsaRigid).Select(r => (GsaRigid)r).ToList();
      var compareLogic = new CompareLogic();
      var result = compareLogic.Compare(gsaRigids, gsaConvertedRigids);
      Assert.Empty(result.Differences);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAGeneralisedRestraintToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.AddRange(GsaElement1dExamples(2, "element 1", "element 2"));
      gsaRecords.AddRange(GsaAnalStageExamples(2, "stage 1", "stage 2"));
      var gsaGenRests = GsaGenRestExamples(2, "gen rest 1", "gen rest 2");
      gsaRecords.AddRange(gsaGenRests);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAGeneralisedRestraint).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedGenRests = gsaConvertedRecords.FindAll(r => r is GsaGenRest).Select(r => (GsaGenRest)r).ToList();
      var compareLogic = new CompareLogic();
      var result = compareLogic.Compare(gsaGenRests, gsaConvertedGenRests);
      Assert.Empty(result.Differences);
    }
    #endregion

    #region Analysis Stages
    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAStageToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(3, "node 1", "node 2", "node 3"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.AddRange(GsaElement1dExamples(2, "element 1", "element 2"));
      var gsaStage = GsaAnalStageExamples(1, "stage 1").First();
      gsaRecords.Add(gsaStage);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(o => o is Model).Select(o => (Model)o).ToList();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAStage).ToList());
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedStage = gsaConvertedRecords.FindAll(r => r is GsaAnalStage).Select(r => (GsaAnalStage)r).First();
      var compareLogic = new CompareLogic();
      var result = compareLogic.Compare(gsaStage, gsaConvertedStage);
      Assert.Empty(result.Differences);
    }
    #endregion

    #region Bridges
    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAAlignmentToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      gsaRecords.Add(GsaGridSurfaceExamples(1, "grid surface 1").FirstOrDefault());
      var gsaAligns = GsaAlignExamples(2, "align 1", "align 2");
      gsaRecords.AddRange(gsaAligns);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAAlignment));
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedAligns = gsaConvertedRecords.FindAll(r => r is GsaAlign).Select(r => (GsaAlign)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAlign x) => x.Chain));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaAlign x) => x.Curv));
      var result = compareLogic.Compare(gsaAligns, gsaConvertedAligns);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaAligns.Count(); i++)
      {
        Assert.Equal(gsaAligns[i].Chain.Select(v => lengthFactor * v).ToList(), gsaConvertedAligns[i].Chain);
        Assert.Equal(gsaAligns[i].Curv.Select(v => (1 / lengthFactor) * v).ToList(), gsaConvertedAligns[i].Curv);
      }
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAInfluenceBeamToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaElement1dExamples(2, "element 1").FirstOrDefault());
      var gsaInfBeams = GsaInfBeamExamples(2, "inf beam 1", "inf beam 2");
      gsaRecords.AddRange(gsaInfBeams);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAInfluenceBeam));
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedInfBeams = gsaConvertedRecords.FindAll(r => r is GsaInfBeam).Select(r => (GsaInfBeam)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaInfBeam x) => x.Position));
      var result = compareLogic.Compare(gsaInfBeams, gsaConvertedInfBeams);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaInfBeams.Count(); i++)
      {
        Assert.Equal(lengthFactor * gsaInfBeams[i].Position, gsaConvertedInfBeams[i].Position);
      }
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAInfluenceNodeToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.Add(GsaNodeExamples(1, "node 1").First());
      var gsaInfNodes = GsaInfNodeExamples(2, "inf node 1", "inf node 2");
      gsaRecords.AddRange(gsaInfNodes);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAInfluenceNode));
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedInfNodes = gsaConvertedRecords.FindAll(r => r is GsaInfNode).Select(r => (GsaInfNode)r).ToList();
      var compareLogic = new CompareLogic();
      var result = compareLogic.Compare(gsaInfNodes, gsaConvertedInfNodes);
      Assert.Empty(result.Differences);
    }

    [Theory]
    [InlineData("m", "mm")]
    [InlineData(null, null)]
    public void GSAPathToNative(string gsaLengthUnit, string speckleLengthUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(GsaAxisExample("axis 1"));
      gsaRecords.Add(GsaMatSteelExample("steel material 1"));
      gsaRecords.Add(GsaPropMassExample("property mass 1"));
      gsaRecords.Add(GsaPropSprExample("property spring 1"));
      gsaRecords.AddRange(GsaNodeExamples(2, "node 1", "node 2"));
      gsaRecords.Add(GsaCatalogueSectionExample("section 1"));
      gsaRecords.Add(GsaGridPlaneExamples(1, "grid plane 1").FirstOrDefault());
      gsaRecords.Add(GsaElement1dExamples(1, "beam 1").FirstOrDefault());
      gsaRecords.Add(GsaGridSurfaceExamples(1, "grid surface 1").FirstOrDefault());
      gsaRecords.Add(GsaAlignExamples(1, "align 1").FirstOrDefault());
      var gsaPaths = GsaPathExamples(2, "path 1", "path 2");
      gsaRecords.AddRange(gsaPaths);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAPath));
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedPaths = gsaConvertedRecords.FindAll(r => r is GsaPath).Select(r => (GsaPath)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPath x) => x.Left));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaPath x) => x.Right));
      var result = compareLogic.Compare(gsaPaths, gsaConvertedPaths);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaPaths.Count(); i++)
      {
        Assert.Equal(lengthFactor * gsaPaths[i].Left, gsaConvertedPaths[i].Left);
        Assert.Equal(lengthFactor * gsaPaths[i].Right, gsaConvertedPaths[i].Right);
      }
    }

    [Theory]
    [InlineData("m", "N", "mm", "kN")]
    [InlineData("m", null, "mm", null)]
    [InlineData(null, null, null, null)]
    public void GSAUserVehicleToNative(string gsaLengthUnit, string gsaForceUnit, string speckleLengthUnit, string speckleForceUnit)
    {
      //unit conversion factors
      var lengthFactor = (string.IsNullOrEmpty(speckleLengthUnit) || string.IsNullOrEmpty(gsaLengthUnit)) ? 1 : Units.GetConversionFactor(speckleLengthUnit, gsaLengthUnit);
      var forceFactor = (string.IsNullOrEmpty(speckleForceUnit) || string.IsNullOrEmpty(gsaForceUnit)) ? 1 : ForceUnits.GetConversionFactor(speckleForceUnit, gsaForceUnit);

      //Create native objects
      var gsaRecords = new List<GsaRecord>();
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Length, Name = gsaLengthUnit });
      gsaRecords.Add(new GsaUnitData() { Option = UnitDimension.Force, Name = gsaForceUnit });
      var gsaVehicles = GsaUserVehicleExamples(2, "vehicle 1", "vehicle 2");
      gsaRecords.AddRange(gsaVehicles);
      Instance.GsaModel.Cache.Upsert(gsaRecords);

      //Convert
      Instance.GsaModel.StreamLayer = GSALayer.Both;
      Instance.GsaModel.StreamSendConfig = StreamContentConfig.ModelOnly;
      var speckleModels = converter.ConvertToSpeckle(gsaRecords.Select(i => (object)i).ToList()).FindAll(so => so is Model).Select(so => (Model)so).ToList();
      var speckleDesignModel = speckleModels.Where(so => so.layerDescription == "Design").FirstOrDefault();
      var speckleAnalysisModel = speckleModels.Where(so => so.layerDescription == "Analysis").FirstOrDefault();
      var speckleObjects = new List<Base>();
      speckleObjects.AddRange(speckleAnalysisModel.elements.FindAll(o => o is GSAUserVehicle));
      speckleAnalysisModel.specs.settings.modelUnits.length = speckleLengthUnit; //change the units
      speckleAnalysisModel.specs.settings.modelUnits.force = speckleForceUnit; //change the units
      speckleObjects.Add(speckleAnalysisModel.specs.settings.modelUnits);
      var gsaConvertedRecords = converter.ConvertToNative(speckleObjects);

      //Checks
      var gsaConvertedVehicles = gsaConvertedRecords.FindAll(r => r is GsaUserVehicle).Select(r => (GsaUserVehicle)r).ToList();
      var compareLogic = new CompareLogic();
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaUserVehicle x) => x.Width));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaUserVehicle x) => x.AxlePosition));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaUserVehicle x) => x.AxleOffset));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaUserVehicle x) => x.AxleLeft));
      compareLogic.Config.MembersToIgnore.Add(GetPropertyName((GsaUserVehicle x) => x.AxleRight));
      var result = compareLogic.Compare(gsaVehicles, gsaConvertedVehicles);
      Assert.Empty(result.Differences);
      for (var i = 0; i < gsaVehicles.Count(); i++)
      {
        Assert.Equal(lengthFactor * gsaVehicles[i].Width, gsaConvertedVehicles[i].Width);
        Assert.Equal(gsaVehicles[i].AxlePosition.Select(v => lengthFactor * v).ToList(), gsaConvertedVehicles[i].AxlePosition);
        Assert.Equal(gsaVehicles[i].AxleOffset.Select(v => lengthFactor * v).ToList(), gsaConvertedVehicles[i].AxleOffset);
        Assert.Equal(gsaVehicles[i].AxleLeft.Select(v => forceFactor * v).ToList(), gsaConvertedVehicles[i].AxleLeft);
        Assert.Equal(gsaVehicles[i].AxleRight.Select(v => forceFactor * v).ToList(), gsaConvertedVehicles[i].AxleRight);
      }
    }
    #endregion

    #region Results
    #endregion

    #region General Data
    
    [Fact]
    public void GSAListCanConvertToNative_ReturnsTrue()
    {
      var gsaList = new GSAList();

      var isConvertable = converter.CanConvertToNative(gsaList);

      Assert.True(isConvertable);
    }

    [Fact]
    public void GSAListToNative_WithValidParameters_HasPopulatedDefinition()
    {
      var mockListDefinition = GsaNodeExamples(4, "node1", "node3", "node4", "node5");
      Instance.GsaModel.Cache.Upsert(mockListDefinition);

      var speckleGsaList = new GSAList()
      {
        name = "Test List",
        listType = GSAListType.Node,
        definitionRefs = new List<string>() { "node1", "node3", "node5"}
      };

      var convertedGsaList = converter.ConvertToNative(speckleGsaList);

      var castedRecords = (List<GsaRecord>)convertedGsaList;

      Assert.NotEmpty(castedRecords);

      var gsaList = (GsaList)castedRecords[0];

      Assert.Equal("Test List", gsaList.Name);
      Assert.Equal("NODE", gsaList.Type);
      Assert.Equal(1, gsaList.Definition[0]);
      Assert.Equal(2, gsaList.Definition[1]);
      Assert.Equal(4, gsaList.Definition[2]);
    }

    [Fact]
    public void GSAListToNative_WithListTypeDefinitionMismatch_HasEmptyDefinition()
    {
      var mockListDefinition = GsaNodeExamples(4, "node1", "node3", "node4", "node5");
      Instance.GsaModel.Cache.Upsert(mockListDefinition);

      var speckleGsaList = new GSAList()
      {
        name = "Test List",
        listType = GSAListType.Element,
        definitionRefs = new List<string>() { "node1", "node3", "node5" }
      };

      var convertedGsaList = converter.ConvertToNative(speckleGsaList);

      var castedRecords = (List<GsaRecord>)convertedGsaList;

      Assert.NotEmpty(castedRecords);

      var gsaList = (GsaList)castedRecords[0];

      Assert.Equal("Test List", gsaList.Name);
      Assert.Equal("ELEMENT", gsaList.Type);
      Assert.Empty(gsaList.Definition);
    }

    [Fact]
    public void GSAListToNative_WithMultipleDefinitionTypes_HasIndicesOfListTypeOnly()
    {
      var mockNodes = GsaNodeExamples(4, "node1", "node3", "node4", "node5");
      var mockMembers = GsaMemberExamples(2, "mem1", "mem2");
      Instance.GsaModel.Cache.Upsert(mockNodes);
      Instance.GsaModel.Cache.Upsert(mockMembers);

      var speckleGsaList = new GSAList()
      {
        name = "Test List",
        listType = GSAListType.Member,
        definitionRefs = new List<string>() 
        { "node1", "node3", "node4", 
          "node5", "mem1", "mem2"}
      };

      var convertedGsaList = converter.ConvertToNative(speckleGsaList);

      var castedRecords = (List<GsaRecord>)convertedGsaList;

      Assert.NotEmpty(castedRecords);

      var gsaList = (GsaList)castedRecords[0];

      Assert.Equal("Test List", gsaList.Name);
      Assert.Equal("MEMBER", gsaList.Type);
      Assert.Equal(2, gsaList.Definition.Count);
    }

    #endregion

    #region Helper
    #region Geometry
    private List<GSANode> SpeckleNodeExamples(int num, params string[] appIds)
    {
      var speckleNodes = new List<GSANode>()
      {
        GetNode(),
        new GSANode()
        {
          nativeId = 2,
          name = "",
          basePoint = new Point(1, 0, 0),
          constraintAxis = SpeckleGlobalAxis(),
          localElementSize = 1,
          colour = "NO_RGB",
          restraint = new Restraint(RestraintType.Free),
          springProperty = null,
          massProperty = null,
          damperProperty = null,
          units = "",
        },
        new GSANode()
        {
          nativeId = 3,
          name = "",
          basePoint = new Point(1, 1, 0),
          constraintAxis = SpeckleGlobalAxis(),
          localElementSize = 1,
          colour = "NO_RGB",
          restraint = new Restraint(RestraintType.Free),
          springProperty = null,
          massProperty = null,
          damperProperty = null,
          units = "",
        },
        new GSANode()
        {
          nativeId = 4,
          name = "",
          basePoint = new Point(0, 1, 0),
          constraintAxis = SpeckleGlobalAxis(),
          localElementSize = 1,
          colour = "NO_RGB",
          restraint = new Restraint(RestraintType.Free),
          springProperty = null,
          massProperty = null,
          damperProperty = null,
          units = "",
        },
        new GSANode()
        {
          nativeId = 5,
          name = "",
          basePoint = new Point(2, 0, 0),
          constraintAxis = SpeckleGlobalAxis(),
          localElementSize = 1,
          colour = "NO_RGB",
          restraint = new Restraint(RestraintType.Free),
          springProperty = null,
          massProperty = null,
          damperProperty = null,
          units = "",
        }
      };
      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleNodes[i].applicationId = appIds[i];
      }
      return speckleNodes.GetRange(0, num);
    }

    private GSANode GetNode()
    {
      return new GSANode()
      {
        nativeId = 1,
        name = "",
        basePoint = new Point(0, 0, 0),
        constraintAxis = SpeckleGlobalAxis(),
        localElementSize = 1,
        colour = "NO_RGB",
        restraint = new Restraint(RestraintType.Free),
        springProperty = null,
        massProperty = null,
        damperProperty = null,
        units = "",
      };
    }

    private List<GSAElement1D> SpeckleElement1dExamples(int num, params string[] appIds)
    {
      var speckleNodes = SpeckleNodeExamples(3, "node 1", "node 2", "node 3");
      var speckleElements = new List<GSAElement1D>()
      {
        new GSAElement1D()
        {
          nativeId = 1,
          name = "",
          baseLine = new Line(),
          property = SpeckleProperty1dExamples(1,"property 1").First(),
          type = ElementType1D.Beam,
          end1Releases = new Restraint(RestraintType.Fixed),
          end2Releases = new Restraint(RestraintType.Fixed),
          orientationNode = null,
          orientationAngle = 0,
          localAxis = SpeckleGlobalAxis().definition,
          parent = null,
          end1Node = speckleNodes[0],
          end2Node = speckleNodes[1],
          topology = speckleNodes.Select(n => (Node)n).ToList().GetRange(0, 2),
          displayValue = null,
          units = "",
          group = 0,
          colour = "NO_RGB",
          action = "",
          isDummy = false,
        },
        new GSAElement1D()
        {
          nativeId = 2,
          name = "",
          baseLine = new Line(),
          property = SpeckleProperty1dExamples(1,"property 1").First(),
          type = ElementType1D.Beam,
          end1Releases = new Restraint(RestraintType.Fixed),
          end2Releases = new Restraint(RestraintType.Fixed),
          orientationNode = null,
          orientationAngle = 0,
          localAxis = SpeckleGlobalAxis().definition,
          parent = null,
          end1Node = speckleNodes[1],
          end2Node = speckleNodes[2],
          topology = speckleNodes.Select(n => (Node)n).ToList().GetRange(1, 2),
          displayValue = null,
          units = "",
          group = 0,
          colour = "NO_RGB",
          action = "",
          isDummy = false,
        },
      };

      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleElements[i].applicationId = appIds[i];
      }
      return speckleElements.GetRange(0, num);
    }

    private List<GSAElement2D> SpeckleElement2dExamples(int num, params string[] appIds)
    {
      var speckleNodes = SpeckleNodeExamples(5, "node 1", "node 2", "node 3", "node 4", "node 5");
      var speckleElements = new List<GSAElement2D>()
      {
        new GSAElement2D()
        {
          nativeId = 1,
          name = "",
          property = SpeckleProperty2dExamples(1,"property 1").First(),
          type = ElementType2D.Quad4,
          orientationAngle = 0,
          parent = null,
          topology = (new int[] { 1, 2, 4 }).Select(i => (Node)speckleNodes[i]).ToList(),
          displayMesh = null,
          units = "",
          group = 0,
          colour = "NO_RGB",
          isDummy = false,
          offset = 0,
        },
        new GSAElement2D()
        {
          nativeId = 2,
          name = "",
          property = SpeckleProperty2dExamples(1,"property 1").First(),
          type = ElementType2D.Triangle3,
          orientationAngle = 1,
          parent = null,
          topology = speckleNodes.Select(n => (Node)n).ToList().GetRange(1, 3),
          displayMesh = null,
          units = "",
          group = 0,
          colour = "NO_RGB",
          isDummy = false,
          offset = 0.1,
        },
      };

      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleElements[i].applicationId = appIds[i];
      }
      return speckleElements.GetRange(0, num);
    }
    #endregion

    #region Loading
    #endregion

    #region Materials
    private List<GSASteel> SpeckleSteelExamples(int num, params string[] appIds)
    {
      var speckleSteels = new List<GSASteel>()
      {
        new GSASteel()
        {
          nativeId = 1,
          name = "",
          grade = "",
          materialType = MaterialType.Steel,
          designCode = "",
          codeYear = "",
          strength = 2e8,
          elasticModulus = 2e11,
          poissonsRatio = 0.3,
          shearModulus = 8e10,
          density = 7850,
          thermalExpansivity = 1e-6,
          dampingRatio = 0,
          cost = 0,
          materialSafetyFactor = 1,
          colour = "NO_RGB",
          yieldStrength = 2e8,
          ultimateStrength = 2.5e8,
          maxStrain = 0.05,
          strainHardeningModulus = 0,
        },
        new GSASteel()
        {
          nativeId = 2,
          name = "",
          grade = "",
          materialType = MaterialType.Steel,
          designCode = "",
          codeYear = "",
          strength = 2e8,
          elasticModulus = 2e11,
          poissonsRatio = 0.3,
          shearModulus = 8e10,
          density = 7850,
          thermalExpansivity = 1e-6,
          dampingRatio = 0,
          cost = 0,
          materialSafetyFactor = 1,
          colour = "NO_RGB",
          yieldStrength = 2e8,
          ultimateStrength = 2.5e8,
          maxStrain = 0.05,
          strainHardeningModulus = 0,
        },
      };
      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleSteels[i].applicationId = appIds[i];
      }
      return speckleSteels.GetRange(0, num);
    }
    #endregion

    #region Properties
    private List<GSAProperty1D> SpeckleProperty1dExamples(int num, params string[] appIds)
    {
      var speckleProperty1d = new List<GSAProperty1D>()
      {
        new GSAProperty1D()
        {
          nativeId = 1,
          designMaterial = null,
          additionalMass = 0,
          cost = null,
          poolRef = null,
          colour = "NO_RGB",
          memberType = MemberType.Beam,
          material = SpeckleSteelExamples(1, "steel 1").First(),
          profile = SpeckleProfileExamples(1, "profile 1").First(),
          referencePoint = BaseReferencePoint.Centroid,
          offsetY = 0,
          offsetZ = 0,
        },
        new GSAProperty1D()
        {
          nativeId = 2,
          designMaterial = null,
          additionalMass = 0,
          cost = null,
          poolRef = null,
          colour = "NO_RGB",
          memberType = MemberType.Beam,
          material = SpeckleSteelExamples(1, "steel 1").First(),
          profile = SpeckleProfileExamples(1, "profile 1").First(),
          referencePoint = BaseReferencePoint.Centroid,
          offsetY = 0,
          offsetZ = 0,
        },
      };
      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleProperty1d[i].applicationId = appIds[i];
      }
      return speckleProperty1d.GetRange(0, num);
    }

    private List<SectionProfile> SpeckleProfileExamples(int num, params string[] appIds)
    {
      var speckleProfiles = new List<SectionProfile>()
      {
        new Rectangular()
        {
          name = "",
          shapeType = ShapeType.Rectangular,
          width = 0.1,
          depth = 0.4,
          webThickness = 0,
          flangeThickness = 0
        },
        new ISection()
        {
          name = "",
          shapeType = ShapeType.I,
          width = 0.1,
          depth = 0.4,
          webThickness = 0.01,
          flangeThickness = 0.02
        },
      };
      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleProfiles[i].applicationId = appIds[i];
      }
      return speckleProfiles.GetRange(0, num);
    }

    private List<GSAProperty2D> SpeckleProperty2dExamples(int num, params string[] appIds)
    {
      var speckleProperties = new List<GSAProperty2D>()
      {
        new GSAProperty2D()
        {

        },
        new GSAProperty2D()
        {

        },
      };
      for (int i = 0; i < appIds.Count(); i++)
      {
        speckleProperties[i].applicationId = appIds[i];
      }
      return speckleProperties.GetRange(0, num);
    }
    #endregion

    #region Results
    #endregion

    #region Constraints
    #endregion

    #region Analysis Stages
    #endregion

    #region Bridges
    private GSAAlignment GetGsaAlignment()
    {
      var axis = SpeckleGlobalAxis();
      var gsaGridPlane = new GSAGridPlane(axis, 1, "myGsaGridPlane", 1);
      var gsaAlignment = new GSAAlignment(new GSAGridSurface(gsaGridPlane, new List<Base>(), 10, GridSurfaceElementType.TwoD, 
        GridSurfaceSpanType.OneWay, 0, LoadExpansion.PlaneCorner, "myGsaGridSurface", 1),
        new List<double>() { 0, 1 },
        new List<double>() { 3, 3 }, "myGsaAlignment", 2);
      return gsaAlignment;
    }

    private static GSAElement1D GetElement1d1()
    {
      return new GSAElement1D(null, null, ElementType1D.Bar, orientationAngle: 0D) { applicationId = "appl1dforGsaElement1d" };
    }

    #endregion

    #region Other

    private static T GenericTestForList<T>(List<GsaRecord> gsaRecord)
    {
      Assert.NotEmpty(gsaRecord);
      Assert.Contains(gsaRecord, so => so is T);

      var obj = (T)(object)(gsaRecord.First());
      return obj;
    }

    private Axis SpeckleGlobalAxis()
    {
      return new Axis()
      {
        //applicationId = "",
        name = "",
        axisType = AxisType.Cartesian,
        definition = new Plane()
        {
          xdir = new Vector(1, 0, 0),
          ydir = new Vector(0, 1, 0),
          normal = new Vector(0, 0, 1),
          origin = new Point(0, 0, 0)
        }
      };
    }

    private bool Map<T, U>(List<T> source, out List<U> destination) where T : Base
    {
      destination = new List<U>();
      var config = new MapperConfiguration(cfg => { cfg.CreateMap<T, U>(); });
      var mapper = new Mapper(config);
      foreach (var s in source)
      {
        destination.Add(Activator.CreateInstance<U>());
        mapper.Map(s, destination.Last());
      }
      return true;
    }

    private bool Map<T, U>(T source, out U destination) where T : Base
    {
      destination = Activator.CreateInstance<U>();
      var config = new MapperConfiguration(cfg => { cfg.CreateMap<T, U>(); });
      var mapper = new Mapper(config);
      mapper.Map(source, destination);
      return true;
    }
    #endregion
    #endregion
  }
}
