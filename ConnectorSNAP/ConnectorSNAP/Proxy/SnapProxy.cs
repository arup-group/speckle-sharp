using ConnectorSNAP.Proxy.s81CsvMaps;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public class SnapProxy : ISNAPProxy
  {
    private List<object> recordsToSave; //Just used as a holding pen between Write and SaveAs - not intended to be used as a cache
    private List<string> errors;

    public List<string> Errors => errors;

    public bool SaveAs(string saveAsFilePath)
    {
      filePath = saveAsFilePath;
      return Writes8iCsv(saveAsFilePath, recordsToSave, out errors);
    }
    private string filePath;

    public void Close()
    {
      throw new NotImplementedException();
    }

    public void LoadResults(object rg, out int numErrorRows)
    {
      throw new NotImplementedException();
    }

    public bool OpenFile(string filePath)
    {
      this.filePath = filePath;
      return File.Exists(filePath);
    }

    public void NewFile(bool v)
    {
      //Nothing to do here as the stream writer will create/overwrite file during SaveAs without needing to open it first
    }

    public void WriteModel(List<object> snapRecords)
    {
      var recordsByType = snapRecords.GroupBy(r => r.GetType()).ToDictionary(r => r.Key, r => r.ToList());
      recordsToSave = recordsByType.Keys.SelectMany(k => recordsByType[k]).ToList();
    }

    public void PrepareResults(object resultTypes)
    {
      throw new NotImplementedException();
    }

    private dynamic RuntimeCast(dynamic source, Type dest) => Convert.ChangeType(source, dest);

    private dynamic RuntimeCast(dynamic source) => Convert.ChangeType(source, source.GetType());

    private static T Cast<T>(object o) => (T)o;

    private bool Writes8iCsv(string filePath, List<object> records, out List<string> errors)
    {
      errors = new List<string>();

      if (records != null)
      {
        var writer = new StreamWriter(filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
          HasHeaderRecord = false,
          IgnoreBlankLines = true,
          MissingFieldFound = null,
          ShouldQuote = (v) => false
        };

        using (var csvWriter = new CsvWriter(writer, config))
        {
          csvWriter.Context.RegisterClassMap<MaterialSteelMap>();
          csvWriter.Context.RegisterClassMap<BeamMap>();
          csvWriter.Context.RegisterClassMap<NodeMap>();
          csvWriter.Context.RegisterClassMap<SecondaryNodeMap>();
          csvWriter.Context.RegisterClassMap<GirderMap>();
          csvWriter.Context.RegisterClassMap<EndReleasesMap>();
          csvWriter.Context.RegisterClassMap<NodalSupportMap>();
          csvWriter.Context.RegisterClassMap<SectionMap>();

          foreach (var record in records)
          {
            csvWriter.WriteRecord(RuntimeCast(record));
            csvWriter.NextRecord();
          }
        }
      }
      return true;
    }

    private bool Reads8iCsv(string filePath, Dictionary<string, Type> typesByKeyword, out List<object> records, out List<string> errors)
    {
      records = new List<object>();
      errors = new List<string>();

      var reader = File.OpenText(filePath);

      var config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        HasHeaderRecord = false,
        IgnoreBlankLines = true,
        MissingFieldFound = null
      };

      using (var csvReader = new CsvReader(reader, config))
      {
        //1.  You manually read the csv file row by row
        while (csvReader.Read())
        {
          var discriminator = csvReader.GetField<string>(0).Split('/').First().Trim();
          if (string.IsNullOrEmpty(discriminator))
          {
            continue;
          }

          csvReader.Context.RegisterClassMap<BeamMap>();
          csvReader.Context.RegisterClassMap<SecondaryNodeMap>();

          foreach (var k in typesByKeyword.Keys)
          {
            if (k.Equals(discriminator, StringComparison.InvariantCultureIgnoreCase))
            {
              try
              {
                var record = csvReader.GetRecord(typesByKeyword[k]);
                if (record != null)
                {
                  records.Add(record);
                }
              }
              catch (Exception ex)
              {
                errors.Add(ex.Message);
              }
            }
          }
        }
      }
      reader.Dispose();
      return errors.Count == 0;
    }

    public bool GetRecords(out List<object> records)
    {
      //These are all the record types currently recognised
      var typesByKeyword = new Dictionary<string, Type>
      {
        { Keyword.BM.GetStringValue(), typeof(Beam) },
        { Keyword.SN.GetStringValue(), typeof(SecondaryNode) }
      };
      var readResult = Reads8iCsv(filePath, typesByKeyword, out records, out errors);

      return (readResult && (errors == null || errors.Count == 0));
    }
  }
}
