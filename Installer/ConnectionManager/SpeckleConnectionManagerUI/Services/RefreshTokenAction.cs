using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Data.Sqlite;
using SpeckleConnectionManagerUI.Models;

namespace SpeckleConnectionManagerUI.Services
{
  class RefreshTokenAction
  {
    public class Info
    {
      public Speckle.Core.Credentials.UserInfo user { get; set; }
      public Speckle.Core.Credentials.ServerInfo serverInfo { get; set; }
    }

    public class InfoData
    {
      public Info data { get; set; }
    }

    public async Task<string> Run()
    {
      HttpClient client = new();
      string appDataFolder =
              Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      string appFolderFullName = Path.Combine(appDataFolder, "Speckle");


      if (!Directory.Exists(appFolderFullName))
      {
        Directory.CreateDirectory(appFolderFullName);
      }

      string dbPath = Path.Combine(appFolderFullName, "Accounts.db");
      var connection = new SqliteConnection($"Data Source={dbPath}");
      connection.Open();

      SqliteCommand selectCommand = new SqliteCommand
              ("SELECT * from objects", connection);

      SqliteDataReader query = selectCommand.ExecuteReader();

      var entries = new List<Row>();

      while (query.Read())
      {
        object[] objs = new object[3];
        var row = query.GetValues(objs);

        entries.Add(new Row
        {
          hash = objs[0].ToString(),
          content = JsonSerializer.Deserialize<Speckle.Core.Credentials.Account>(objs[1].ToString())
        });
      }


      foreach (var entry in entries)
      {
        var content = entry.content;
        var isDefault = entry.content.isDefault;
        var url = content.serverInfo.url;
        Console.WriteLine($"Auth token: {content.token}");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", content.token);

        HttpResponseMessage response;
        try {
          response = await client.PostAsJsonAsync($"{url}/auth/token", new
          {
            appId = "sdm",
            appSecret = "sdm",
            refreshToken = content.refreshToken,
          });
        }
        catch {
          continue;
        }
        Console.WriteLine(response.StatusCode);
        if (response.StatusCode != HttpStatusCode.OK)
        {
          continue;
        }
        var tokens = await response.Content.ReadFromJsonAsync<Tokens>();
        content.token = tokens.token;
        content.refreshToken = tokens.refreshToken;

        Console.WriteLine(tokens.token);

        HttpResponseMessage info;
        try
        {
          info = await client.PostAsJsonAsync($"{url}/graphql", new
          {
            query = "{\n  user {\n    id\n    name\n    email\n    company\n    avatar\n} serverInfo {\n name \n company \n canonicalUrl \n }\n}\n"
          });
        }
        catch {
          continue;
        }
;
        Console.WriteLine(response.StatusCode);
        if (response.StatusCode != HttpStatusCode.OK)
        {
          continue;
        }

        var infoContent = await info.Content.ReadFromJsonAsync<InfoData>();

        if (infoContent == null) return "";

        var serverInfo = infoContent.data.serverInfo;
        serverInfo.url = url;

        var userInfo = infoContent.data.user;

        var updateContent = new Speckle.Core.Credentials.Account()
        {
          token = tokens.token,
          refreshToken = tokens.refreshToken,
          isDefault = isDefault,
          serverInfo = serverInfo,
          userInfo = userInfo
        };

        string jsonString = JsonSerializer.Serialize(updateContent);

        var command = connection.CreateCommand();       

        command.CommandText =
          @"
            UPDATE objects
            SET content = @content
            WHERE hash = @hash
          ";

        Console.WriteLine(connection.State);

        command.Parameters.AddWithValue("@hash", content.id);
        command.Parameters.AddWithValue("@content", jsonString);
        command.ExecuteNonQuery();
        Console.WriteLine($"Updated {entry.hash}");
      };

      connection.Close();

      return "";
    }
  }
}

