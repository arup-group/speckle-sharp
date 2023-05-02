using System;
using System.Collections.Generic;
using System.Linq;
using SpeckleConnectionManagerUI.Models;
using SpeckleConnectionManagerUI.ViewModels;

namespace SpeckleConnectionManagerUI.Services
{
  public class Database
  {
    public IEnumerable<ConnectStatusItem> GetItems()
    {
      var savedConnections = Sqlite.GetData();

      var connectStatuses = new List<ConnectStatusItem>();
      int identifier = 0;

      foreach (var savedConnection in savedConnections)
      {
        var connectStatus = new ConnectStatusItem();
        connectStatus.ServerUrl = savedConnection.serverInfo.url;
        connectStatus.ServerName = savedConnection.serverInfo.name;
        connectStatus.Identifier = identifier;
        connectStatus.Disconnected = false;
        connectStatus.Default = savedConnection.isDefault;
        connectStatus.DefaultServerLabel = connectStatus.Default ? "DEFAULT" : "SET AS DEFAULT";
        connectStatus.Colour = connectStatus.Disconnected ? "Red" : "Green";
        connectStatus.Id = savedConnection.id;

        if (!connectStatuses.Contains(connectStatus)) connectStatuses.Add(connectStatus);
        identifier++;
      }

      var defaultConnectStatuses = new List<ConnectStatusItem>()
            {
                new ConnectStatusItem
                    {ServerUrl = "https://v2.speckle.arup.com", Identifier = 0}
            };

      var connectedServerUrls = connectStatuses.Select(x => x.ServerUrl).ToList();
      var connectedServerMaxIdentifier = connectedServerUrls.Count > 0 ? connectStatuses.Select(x => x.Identifier).Max() : 0;

      foreach (var defaultConnectStatus in defaultConnectStatuses)
      {
        if (!connectedServerUrls.Contains(defaultConnectStatus.ServerUrl))
        {
          var connected = savedConnections.Any(savedConnection => savedConnection.serverInfo.url == defaultConnectStatus.ServerUrl);

          var _serverName = savedConnections.Where(x => x.serverInfo.url == defaultConnectStatus.ServerUrl).Select(x => x.serverInfo.name).FirstOrDefault();
          var _default = savedConnections.Where(x => x.serverInfo.url == defaultConnectStatus.ServerUrl).Select(x => x.isDefault).FirstOrDefault();

          defaultConnectStatus.ServerName = _serverName;
          defaultConnectStatus.Disconnected = !connected;
          defaultConnectStatus.Default = _default;
          defaultConnectStatus.DefaultServerLabel = _default ? "DEFAULT" : "SET AS DEFAULT";
          defaultConnectStatus.Colour = connected ? "Green" : "Red";

          connectedServerMaxIdentifier = connectedServerMaxIdentifier + 1;
          defaultConnectStatus.Identifier = connectedServerMaxIdentifier;

          if (!connectStatuses.Contains(defaultConnectStatus)) connectStatuses.Add(defaultConnectStatus);
        }

      }

      return connectStatuses.ToArray();
    }

    //If Token is added to the ConnectStatusItem class and assigned in the GetItems method above,
    //it will be outdated by the scheduled TimedRefreshToken - hence a new connection to the DB is
    //created here so that it can obtain the latest token from the DB when it's triggered by the
    //COPY TOKEN button
    public string GetToken(string id)
    {
      if (!string.IsNullOrEmpty(id))
      {
        var savedConnections = Sqlite.GetData();

        foreach (var savedConnection in savedConnections)
        {
          if (!string.IsNullOrEmpty(savedConnection.id) && savedConnection.id.Equals(id))
          {
            return savedConnection.token;
          }
        }
      }
      return "";
    }
  }
}