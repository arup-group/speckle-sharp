using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ConverterSNAPTests
{
  public class ConverterSNAPTests
  {
    private ISpeckleConverter converter;

    public ConverterSNAPTests()
    {
      converter = KitManager.GetDefaultKit().LoadConverter(HostApplications.SNAP.Name);
    }

    [Fact]
    public void NodeToNative()
    {
      var speckleNode = new Objects.Structural.Geometry.Node()
      {
        name = "test_node",
        applicationId = "testnode01",
        basePoint = new Objects.Geometry.Point(10, 20, 30)
      };

      Assert.True(converter.CanConvertToNative(speckleNode));
      var nativeObjects = converter.ConvertToNative(new List<Base> { speckleNode });
      Assert.NotNull(nativeObjects);
      Assert.Single(nativeObjects);

      Assert.Equal(typeof(SecondaryNode), nativeObjects.First().GetType());
      var secondaryNode = (SecondaryNode)nativeObjects.First();
      Assert.Equal(10, secondaryNode.X);
      Assert.Equal(20, secondaryNode.Y);
      Assert.Equal(30, secondaryNode.Z);
    }
  }
}
