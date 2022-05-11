using ConnectorGSA;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConnectorGSATests
{
  //Issues with receiving:
  // - members vs elements - i.e. layers
  // - polylines - design layer: write multi-node lines; for analysis layer: break up into multiple line elements
  // - meshes - design layer: write 2D element using outside lines only (the GSA does meshing after all);
  //            for analysis layer: write faces of mesh as elements
  // - merging of existing objects with newly-received ones before writing back to the GSA instance, only for relevant streams
  //Other notes:
  // - results are ignored
  public class ReceiveTests : SpeckleConnectorFixture
  {
    //TO DO: add new test for 2 models, both design and analysis models
    [Fact]
    public async void ReceiveDesignModelOnly()
    {
      Instance.GsaModel.Proxy = new Speckle.ConnectorGSA.Proxy.GsaProxy();
      ((Speckle.ConnectorGSA.Proxy.GsaProxy)Instance.GsaModel.Proxy).NewFile(false);
      Instance.GsaModel.StreamLayer = GSALayer.Design;
      Instance.GsaModel.CoincidentNodeAllowance = 0.01;

      /* these lines would cause the libraries in %AppData% to be loaded, including the objects.dll library, which contains types
      * which C# would recognise as being different to the ones used in the converter code
      * */
      //var kit = KitManager.GetDefaultKit();
      //var converter = kit.LoadConverter(Applications.GSA);
      var converter = new ConverterGSA.ConverterGSA();
      var account = AccountManager.GetDefaultAccount();
      var client = new Client(account);

      var nativeTypeGenerations = Instance.GsaModel.Proxy.GetTxTypeDependencyGenerations(GSALayer.Design);

      var numExpectedByObjectType = new Dictionary<Type, int>()
      {
        { typeof(GsaAxis), 3 },
        { typeof(GsaMatConcrete), 1 },
        { typeof(GsaPropSpr), 8 },
        { typeof(GsaSection), 1 },
        { typeof(GsaProp2d), 1 },
        { typeof(GsaNode), 95 },
        { typeof(GsaLoadCase), 3 },
      };

      try
      {
        //First receive
        var result = await CoordinateReceive(converter, client, new Progress<MessageEventArgs>());

        Assert.True(result.Received);
        Assert.True(result.Converted);
        Assert.NotNull(result.ConvertedObjects);
        Assert.NotEmpty(result.ConvertedObjects);

        var objectsByType = result.ConvertedObjects.GroupBy(o => o.GetType()).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var t in numExpectedByObjectType.Keys)
        {
          Assert.True(objectsByType.ContainsKey(t));
          Assert.NotNull(objectsByType[t]);
          Assert.Equal(numExpectedByObjectType[t], objectsByType[t].Count());
        }

        //Second receive
        result = await CoordinateReceive(converter, client, new Progress<MessageEventArgs>());

        Assert.True(result.Received);
        Assert.True(result.Converted);
        Assert.NotEmpty(result.ConvertedObjects);

        objectsByType = result.ConvertedObjects.GroupBy(o => o.GetType()).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var t in numExpectedByObjectType.Keys)
        {
          Assert.True(objectsByType.ContainsKey(t));
          Assert.NotNull(objectsByType[t]);
          Assert.Equal(numExpectedByObjectType[t], objectsByType[t].Count());
        }
      }
      catch (Exception ex)
      {
        throw new Xunit.Sdk.XunitException(ex.Message);
      }
      finally
      {
        ((Speckle.ConnectorGSA.Proxy.GsaProxy)Instance.GsaModel.Proxy).Close();
      }
    }

    [Fact(Skip ="Not implemented yet")]
    public void HeadlessReceiveDesignLayer()
    {

    }

    [Fact(Skip = "Not implemented yet")]
    public void HeadlessReceiveBothLayers()
    {

    }


    private async Task<CoordinateReceiveResult> CoordinateReceive(ISpeckleConverter converter, Client client, IProgress<MessageEventArgs> loggingProgress)
    {
      var result = new CoordinateReceiveResult();

      var streamState = await GetTestStream(client);
      if (streamState.Stream == null)
      {
        return new CoordinateReceiveResult() { Received = false };
      }

      //var branchName = streamState.Stream.branches.items.First().name;
      //var branch = await client.BranchGet(streamState.Stream.id, branchName, 1);
      var commitId = streamState.Stream.branch.commits.items.FirstOrDefault().referencedObject;

      var transport = new ServerTransport(streamState.Client.Account, streamState.Stream.id);
      var topLevelObjects = new List<Base>();
      result.Received = await Commands.Receive(commitId, streamState, transport, topLevelObjects);
      if (result.Received)
      {
        result.Converted = Commands.ConvertToNative(topLevelObjects, converter, loggingProgress); //This writes it to the cache
      }
      if (!result.Received || !result.Converted)
      {
        return result;
      }

      var nativeTypeGenerations = Instance.GsaModel.Proxy.GetTxTypeDependencyGenerations(GSALayer.Both);
      var natives = new List<GsaRecord>();
      foreach (var gen in nativeTypeGenerations)
      {
        foreach (var t in gen)
        {
          //Getting it from the cache means the objects are extracted after merging between existing and new is done
          if (Instance.GsaModel.Cache.GetNative(t, out var currNatives) && currNatives != null && currNatives.Any())
          {
            natives.AddRange(currNatives);
          }
        }
      }

      if (natives.Any())
      {
        result.ConvertedObjects = natives;
      }

      return result;
    }

    private async Task<StreamStateOld> GetTestStream(Client client)
    {
      StreamStateOld streamState = null;
      Speckle.Core.Api.Stream testStream = null;

      try
      {
        var streams = await client.StreamsGet(50);
        testStream = streams.FirstOrDefault(s => s.name.Contains(testStreamMarker));
        if (testStream != null)
        {
          var branches = await client.StreamGetBranches(testStream.id, 1);
          testStream.branch = branches.First();
        }
      }
      catch (Exception e)
      {

      }

      streamState = new StreamStateOld() { Client = client, Stream = testStream };
      return streamState;
    }

    public struct CoordinateReceiveResult
    {
      public bool Received;
      public bool Converted;
      public List<GsaRecord> ConvertedObjects;
    }
  }
}
