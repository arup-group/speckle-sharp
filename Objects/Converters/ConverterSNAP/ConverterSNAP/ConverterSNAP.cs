using Objects.Structural.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Speckle.SNAP.API;
using Objects.Structural.Analysis;

namespace ConverterSNAP
{
  public partial class ConverterSNAP : ISpeckleConverter
  {
    public static string AppName = HostApplications.SNAP.Name;

    public string Description => "Default Speckle Kit for SNAP";

    public string Name => nameof(ConverterSNAP);

    public string Author => "Arup";

    public string WebsiteOrEmail => "https://www.arup.com/";

    public HashSet<Exception> ConversionErrors { get; } = new HashSet<Exception>();

    public ProgressReport Report { get; } = new ProgressReport();

    public void SetConverterSettings(object settings)
    {
      throw new NotImplementedException("This converter does not have any settings.");
    }

    private Dictionary<Type, Func<Base, List<object>>> ToNativeFns;
    private Dictionary<Type, Func<object, bool>> ToSpeckleFns;

    public ConverterSNAP()
    {
      SetupToNativeFns();
    }

    public bool CanConvertToSpeckle(object @object)
    {
      var t = @object.GetType();
      return (ToSpeckleFns.ContainsKey(t));
    }

    public bool CanConvertToNative(Base @object)
    {
      var t = @object.GetType();
      return (ToNativeFns.ContainsKey(t));
    }

    public object ConvertToNative(Base @object)
    {
      var objectType = @object.GetType();

      return (ToNativeFns.ContainsKey(objectType) ? ToNativeFns[objectType](@object) : null);
    }

    public List<object> ConvertToNative(List<Base> objects)
    {
      var retList = new List<object>();

      var models = objects.Where(o => o is Model).Cast<Model>().ToList();

      if (models.Count == 1)
      {
        retList.AddRange(ModelToNative(models.First()));
      }
      else if (models.Count > 1)
      {
        //Prefer analysis if present
        var analysisModels = models.Where(m => m.layerDescription.ToLower().Contains("analysis"));
        if (analysisModels.Any())
        {
          retList.AddRange(ModelToNative(analysisModels.First()));
        }
      }
      var remaining = objects.Except(models).ToList();
      if (remaining.Any())
      {
        retList.AddRange(SpeckleObjectsToNative(remaining));
      }
      return retList;
    }

    public Base ConvertToSpeckle(object @object)
    {
      throw new NotImplementedException();
    }

    public List<Base> ConvertToSpeckle(List<object> objects)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<string> GetServicedApplications() => new string[] { AppName };

    public void SetContextDocument(object doc)
    {
      throw new NotImplementedException();
    }

    public void SetContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      throw new NotImplementedException();
    }

    public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      throw new NotImplementedException();
    }
  }
}
