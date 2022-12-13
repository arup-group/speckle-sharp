using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConnectorGrasshopper.Extras;
using GraphQL.Client.Http;
using GraphQL;
using Grasshopper.Kernel;
using Speckle.Core.Api.GraphQL.Serializer;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Api;
using Speckle.Core.Models.Extensions;
using System.Security.Principal;
using Grasshopper.Kernel.Data;

namespace ConnectorGrasshopper.Accounts
{
  public class CreateAccount : GH_SpeckleComponent
  {
    public CreateAccount() : base("Create Account", "cAccount", "Creates an account from token and server url",
      ComponentCategories.PRIMARY_RIBBON,
      ComponentCategories.STREAMS)
    {
    }

    public override Guid ComponentGuid => new Guid("{59A6814F-AAFD-40CD-8EF0-10C8E7A67537}");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Token", "T", "Speckle Token", GH_ParamAccess.item);
      pManager.AddGenericParameter("Server URL", "S", "Speckle Server URL", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Account", "A", "Speckle Account",
        GH_ParamAccess.item);
    }

    private Account account;
    private Exception error;
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      if (error != null)
      {
        Message = null;
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.ToFormattedString());
        error = null;
      }
      else if (account == null)
      {
        string token = null;
        if (!DA.GetData(0, ref token))
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Token is not provided");
          Message = null;
          return;
        }

        string server = null;
        if (!DA.GetData(1, ref server))
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Server Url not specified");
          Message = null;
          return;
        }        

        Task.Run(async () =>
        {
          try
          {
            account = null;
            account = await AccountManager.CreateAccountFromToken(token, null, server);
          }
          catch (Exception e)
          {
            error = e;
          }
          finally
          {
            Rhino.RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
          }
        });
      }
      else
      {
        DA.SetData(0, account.userInfo.id);
      }
    }

  }
}
