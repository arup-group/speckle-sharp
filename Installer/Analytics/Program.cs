using Piwik.Tracker;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;

namespace analytics
{
  class Program
  {
    private static readonly string PostHogBaseUrl = "https://posthog.insights.arup.com/";

    private static readonly string PiwikBaseUrl = "https://arupdt.matomo.cloud/";
    private static readonly int SiteId = 1;
    private static readonly string CacheLocation = "\\Speckle\\Accounts.db";

    static void Main(string[] args)
    {
      PiwikTracker _piwikTracker = new PiwikTracker(SiteId, PiwikBaseUrl);
      string _version = args[0];
      string _internalDomain = args[1];
      string _machineName = Environment.MachineName.ToLower(new CultureInfo("en-GB", false));
      string _domainName = Dns.GetHostEntry("").HostName;
      string _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      // If the cache has been set up this is an update, otherwise it's a new user
      string _installType = File.Exists(_appDataFolder + CacheLocation) ? "update" : "new";

      bool _isInternalDomain = _machineName.Contains(_internalDomain) || _domainName.Contains(_internalDomain);

      if (_isInternalDomain)
      {
        // Hash the username and send it with the installer info
        string _userEmail = Environment.UserName + "@" + _internalDomain + ".com";
        string _userId = _userEmail.ToLower(new CultureInfo("en-GB", false));
        var _hashedUserId = ComputeSHA256Hash(_userId);
        _piwikTracker.SetUserId(_hashedUserId);

        // Send this information to Matomo
        _piwikTracker.DoTrackEvent("SpeckleInstallerV2", _installType, _version);

        // Send info to Posthog as well
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(PostHogBaseUrl + "capture");
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        httpWebRequest.Accept = "text/plain";
        httpWebRequest.Method = "POST";
        httpWebRequest.UseDefaultCredentials = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        string _posthogToken = args[2];
        var properties = new Dictionary<string, object>()
          {
            { "distinct_id", _userId },
            { "install_type", _installType },
            { "version", _version }
          };

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
          string json = JsonConvert.SerializeObject(new
          {
            api_key = _posthogToken,
            @event = $"install_{_installType}",
            properties
          });

          streamWriter.Write("data=" + HttpUtility.UrlEncode(json));
          streamWriter.Flush();
          streamWriter.Close();
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
          var result = streamReader.ReadToEnd();
        }
      }

      // Swap to using server id (name) as hash; add unique constraint to hash if primary key or constraint does not exist
      UpdateAccountsDb();
    }

    /**
    * Perform a one-way hash of the input text.
    **/
    private static string ComputeSHA256Hash(string text)
    {
      using (var sha256 = new SHA256Managed())
      {
        byte[] _encodedText = Encoding.UTF8.GetBytes(text);
        byte[] _hash = sha256.ComputeHash(_encodedText);
        string _hashString = BitConverter.ToString(_hash);
        return _hashString.Replace("-", "").ToLower(new CultureInfo("en-GB", false));
      }
    }

    private static void UpdateAccountsDb()
    {
      string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + CacheLocation);
      var connection = new SqliteConnection($"Data Source={dbPath}");
      connection.Open();

      SqliteCommand selectCommand = new SqliteCommand
          ("SELECT * from objects", connection);

      SqliteDataReader query = selectCommand.ExecuteReader();

      while (query.Read())
      {
        var objs = new object[3];
        query.GetValues(objs);

        var hash = objs[0].ToString();
        var storedContent = System.Text.Json.JsonSerializer.Deserialize<SqliteContent>(objs[1].ToString());
        var serverName = storedContent.serverInfo.name;

        // If the url is already stored update hash to be server name
        if (storedContent != null && hash != serverName)
        {
          var updateCommand = connection.CreateCommand();
          updateCommand.CommandText =
              @"
                UPDATE objects
                SET hash = @server
                WHERE hash = @hash
              ";

          Console.WriteLine(connection.State);

          updateCommand.Parameters.AddWithValue("@hash", hash);
          updateCommand.Parameters.AddWithValue("@server", serverName);
          updateCommand.ExecuteNonQuery();
        }
      }

      SqliteCommand selectPKeyCommand = new SqliteCommand
          ("SELECT l.name FROM pragma_table_info('objects') as l WHERE l.pk <> 0", connection);

      query = selectPKeyCommand.ExecuteReader();

      if (!query.Read())
      {
        var createIndexCommand = connection.CreateCommand();
        createIndexCommand.CommandText =
          @"
            CREATE UNIQUE INDEX IF NOT EXISTS index_objects_hash ON objects (hash)
        ";
        createIndexCommand.ExecuteNonQuery();
      }

      connection.Close();
    }

    public class ServerInfo
    {
      public String name { get; set; }
      public String url { get; set; }
    }

    public class SqliteContent
    {
      public string id { get; set; }
      public bool isDefault { get; set; } = false;
      public string token { get; set; }
      public object user { get; set; }
      public ServerInfo serverInfo { get; set; }
      public string refreshToken { get; set; }
    }
  }
}
