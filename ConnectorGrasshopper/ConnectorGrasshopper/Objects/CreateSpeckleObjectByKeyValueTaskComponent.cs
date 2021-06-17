﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConnectorGrasshopper.Extras;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Utilities = ConnectorGrasshopper.Extras.Utilities;

namespace ConnectorGrasshopper.Objects
{
  public class CreateSpeckleObjectByKeyValueTaskComponent: SelectKitTaskCapableComponentBase<Base>
  {
    public override Guid ComponentGuid => new Guid("B5232BF7-7014-4F10-8716-C3CEE6A54E2F");

    public CreateSpeckleObjectByKeyValueTaskComponent() : base("Create Speckle Object by Key/Value", "K/V",
      "Creates a speckle object from key value pairs", ComponentCategories.PRIMARY_RIBBON, ComponentCategories.OBJECTS)
    {
      
    }
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddTextParameter("Keys", "K", "List of keys", GH_ParamAccess.list);
      pManager.AddGenericParameter("Values", "V", "List of values", GH_ParamAccess.tree);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddParameter(new SpeckleBaseParam("Object", "O", "Speckle object", GH_ParamAccess.item));
    }
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      if (InPreSolve)
      {
        var keys = new List<string>();
        var valueTree = new GH_Structure<IGH_Goo>();
        
        DA.GetDataList(0, keys);
        DA.GetDataTree(1, out valueTree);
        TaskList.Add(Task.Run(()=> DoWork(keys, valueTree)));
        return;
      }

      if (!GetSolveResults(DA, out Base result))
      {
        // Normal mode not supported
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This component only works with Parallel Computing");
        return;
      }

      if (result != null)
      {
        DA.SetData(0, result);
      }
    }
    
    
    public Base DoWork(List<string> keys, GH_Structure<IGH_Goo> valueTree)
    {
      try
      {
        // 👉 Checking for cancellation!
        if (CancelToken.IsCancellationRequested) return null;
        
        // Create a path from the current iteration
        var searchPath = new GH_Path(RunCount - 1);

        // Grab the corresponding subtree from the value input tree.
        var subTree = Utilities.GetSubTree(valueTree, searchPath);
        var speckleObj = new Base();
        // Find the list or subtree belonging to that path
        if (valueTree.PathExists(searchPath) || valueTree.Paths.Count == 1)
        {
          var list = valueTree.Paths.Count == 1 ? valueTree.Branches[0] : valueTree.get_Branch(searchPath);
          // We got a list of values
          var ind = 0;
          var hasErrors = false;
          keys.ForEach(key =>
          {
            if (ind < list.Count)
              try
              {
                if (Converter != null)
                  speckleObj[key] = Utilities.TryConvertItemToSpeckle(list[ind], Converter);
                else
                  speckleObj[key] = list[ind];
              }
              catch (Exception e)
              {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                hasErrors = true;
              }

            ind++;
          });
          if (hasErrors)
          {
            speckleObj = null;
          }
        }
        else
        {
          // We got a tree of values

          // Create the speckle object with the specified keys
          var index = 0;
          var hasErrors = false;
          keys.ForEach(key =>
          {
            var itemPath = new GH_Path(index);

            var branch = subTree.get_Branch(itemPath);
            if (branch != null)
            {
              var objs = new List<object>();
              foreach (var goo in branch)
              {
                if(Converter != null)
                  objs.Add(Utilities.TryConvertItemToSpeckle(goo, Converter));
                else
                  objs.Add(goo);
              }

              if (objs.Count > 0)
                try
                {
                  speckleObj[key] = objs;
                }
                catch (Exception e)
                {
                  AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                  hasErrors = true;
                }
            }

            index++;
          });

          if (hasErrors)
          {
            speckleObj = null;
          }
        }

        return speckleObj;
      }
      catch (Exception e)
      {
        // If we reach this, something happened that we weren't expecting...
        Log.CaptureException(e);
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Something went terribly wrong... " + e.Message);
        return null;
      }
    }

  }
}
