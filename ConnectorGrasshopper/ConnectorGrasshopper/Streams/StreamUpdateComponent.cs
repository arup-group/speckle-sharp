using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using System.Threading.Tasks;
using ConnectorGrasshopper.Extras;
using Grasshopper.Kernel;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models.Extensions;
using Logging = Speckle.Core.Logging;

namespace ConnectorGrasshopper.Streams
{
  public class StreamUpdateComponent : GH_SpeckleComponent
  {
    public StreamUpdateComponent() : base("Stream Update", "sUp", "Updates a stream with new details", ComponentCategories.PRIMARY_RIBBON,
      ComponentCategories.STREAMS)
    { }
    public override Guid ComponentGuid => new Guid("F83B9956-1A5C-4844-B7F6-87A956105831");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      var stream = pManager.AddParameter(new SpeckleStreamParam("Stream", "S", "Unique ID of the stream to be updated.", GH_ParamAccess.item));
      var name = pManager.AddTextParameter("Name", "N", "Name of the stream.", GH_ParamAccess.item);
      var desc = pManager.AddTextParameter("Description", "D", "Description of the stream", GH_ParamAccess.item);
      var isPublic = pManager.AddBooleanParameter("Public", "P", "True if the stream is to be publicly available.",
        GH_ParamAccess.item);
      var jobNumber = pManager.AddTextParameter("Job Number", "JN", "Job number associated with the stream.", GH_ParamAccess.item);
      Params.Input[name].Optional = true;
      Params.Input[desc].Optional = true;
      Params.Input[isPublic].Optional = true;
      Params.Input[jobNumber].Optional = true;
    }

    protected override Bitmap Icon => Properties.Resources.StreamUpdate;

    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Stream ID", "ID", "Unique ID of the stream to be updated.", GH_ParamAccess.item);
    }

    private Stream stream;
    Exception error = null;

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      DA.DisableGapLogic();
      GH_SpeckleStream ghSpeckleStream = null;
      string name = null;
      string description = null;
      bool isPublic = false;
      string jobNumber = null;

      if (!DA.GetData(0, ref ghSpeckleStream)) return;
      DA.GetData(1, ref name);
      DA.GetData(2, ref description);
      DA.GetData(3, ref isPublic);
      DA.GetData(4, ref jobNumber);

      var streamWrapper = ghSpeckleStream.Value;
      if (error != null)
      {
        Message = null;
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.ToFormattedString());
        error = null;
      }
      else if (stream == null)
      {
        if (streamWrapper == null)
        {
          Message = "";
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not a stream wrapper!");
          return;
        }
        if (DA.Iteration == 0)
          Tracker.TrackNodeRun();
        Message = "Fetching";

        if (AccountManager.AccountFromToken == null)
        {
          Task.Run(StreamUpdateTask(name, description, isPublic, jobNumber, streamWrapper));
        }
        else
        {
          // just wait for task to finish
          var temp = Task.Run(StreamUpdateTask(name, description, isPublic, jobNumber, streamWrapper)).Result;
          if (temp != null) SetData(DA, streamWrapper);
        }
      }
      else
      {
        SetData(DA, streamWrapper);
      }
    }

    private void SetData(IGH_DataAccess DA, StreamWrapper streamWrapper)
    {
      stream = null;
      Message = "Done";
      DA.SetData(0, streamWrapper.StreamId);
    }

    private Func<Task<Stream>> StreamUpdateTask(string name, string description, bool isPublic, string jobNumber, StreamWrapper streamWrapper)
    {
      return async () =>
      {
        try
        {
          var account = streamWrapper.GetAccount().Result;
          var client = new Client(account);
          stream = await client.StreamGet(streamWrapper.StreamId);

          var input = new StreamUpdateInput();
          if (!string.IsNullOrEmpty(jobNumber)) input = new StreamWithJobNumberUpdateInput { id = streamWrapper.StreamId, name = name ?? stream.name, description = description ?? stream.description, jobNumber = jobNumber ?? stream.jobNumber };
          else input = new StreamUpdateInput { id = streamWrapper.StreamId, name = name ?? stream.name, description = description ?? stream.description };

          if (stream.isPublic != isPublic) input.isPublic = isPublic;

          await client.StreamUpdate(input);
        }
        catch (Exception e)
        {
          error = e;
        }
        finally
        {
          Rhino.RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
        }
        return stream;
      };
    }
  }
}
