using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Speckle.Core.Credentials;
using Logging = Speckle.Core.Logging;
using Speckle.Core.Models.Extensions;

namespace ConnectorGrasshopper.Streams
{
  public class AccountSelectorComponent : GH_SpeckleComponent
  {
    public AccountSelectorComponent() : base("Account Selector", "AccSel", "Selects Account based on Input or List Account", ComponentCategories.PRIMARY_RIBBON,
      ComponentCategories.STREAMS)
    {
    }

    public override Guid ComponentGuid => new Guid("F74EA81C-062F-43D1-8D64-53600C72171D");

    protected override Bitmap Icon => Properties.Resources.AccountSelector;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddTextParameter("Details", "D", "Details to Create Account From", GH_ParamAccess.item);
      var acc = pManager.AddTextParameter("Account", "A", "Account to get stream with.", GH_ParamAccess.item);

      Params.Input[0].Optional = true;
      Params.Input[1].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Account", "Acc", "Account to use", GH_ParamAccess.item);
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
        string details = null;
        string acc = null;

        DA.GetData(0, ref details);
        DA.GetData(1, ref acc);

        if (string.IsNullOrEmpty(details) && string.IsNullOrEmpty(acc))
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No input or account provided. please specify either.");
        }
        else if (string.IsNullOrEmpty(details))
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "No input provided. Using account input.");
          DA.SetData(0, acc);
        }
        else // empty account and non-empty details
        {
          try
          {
            dynamic detailsDeserialised = Newtonsoft.Json.JsonConvert.DeserializeObject(details);
            account = Task.Run(async () =>
            {
              Account theAccount = null;
              try
              {
                var token = (string)detailsDeserialised.token;
                var server = (string)detailsDeserialised.server;

                theAccount = await AccountManager.CreateAccountFromToken(token, server);
                return theAccount;
              }
              catch (Exception e)
              {
                error = e;
                return null;
              }
            }).Result;
            if (account != null)
            {
              DA.SetData(0, account.userInfo.id);
              Rhino.RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
            }
          }
          catch (Exception ex)
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.ToFormattedString());
          }
        }
      }
      else
      {
        DA.SetData(0, account.userInfo.id);
        account = null;
      }
    }

    protected override void BeforeSolveInstance()
    {
      base.BeforeSolveInstance();
    }

  }
}
