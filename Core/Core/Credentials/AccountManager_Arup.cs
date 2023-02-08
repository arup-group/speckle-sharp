using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Speckle.Core.Logging;
using Speckle.Newtonsoft.Json;

namespace Speckle.Core.Credentials
{
  /// <summary>
  /// The partial declaration of <see cref="AccountManager"/> which is used for RhinoCompute compatibility
  /// </summary>
  public static partial class AccountManager
  {
    private static Account _account;

    /// <summary>
    /// Creates an in-memory account by using token
    /// </summary>
    /// <param name="token">the token</param>
    /// <param name="server">Server to use to create the account</param>
    /// <returns></returns>
    public static async Task<Account> CreateAccountFromToken(string token, string server)
    {
      _account = await CreateAccount(token, server);
      return _account;
    }

    /// <summary>
    /// Gets all the accounts present in this environment.
    /// </summary>
    /// <param name="includeInMemoryAccount">whether to consider account created using token or not</param>
    /// <returns></returns>
    public static IEnumerable<Account> GetAccounts(bool includeInMemoryAccount)
    {
      var accounts = GetAccounts();
      if (includeInMemoryAccount && _account != null)
      {
        accounts = accounts.Concat(new[] { _account });
      }
      return accounts;
    }

    /// <summary>
    /// Gets this environment's default account if any. If there is no default, the first found will be returned and set as default.
    /// </summary>
    /// <param name="includeInMemoryAccount">whether to consider account created using token or not</param>
    /// <returns>The default account or null.</returns>
    public static Account GetDefaultAccount(bool includeInMemoryAccount)
    {
      var defaultAccount = GetDefaultAccount();
      if (includeInMemoryAccount && _account != null)
      {
        defaultAccount = _account;
      }
      return defaultAccount;
    }

    private static async Task<Account> CreateAccount(string token, string server)
    {
      if (String.IsNullOrWhiteSpace(token))
      {
        throw new ArgumentNullException("token cannot be null");
      }
      else if (String.IsNullOrWhiteSpace(server))
      {
        throw new ArgumentNullException("server cannot be null or empty");
      }

      var secret = Environment.GetEnvironmentVariable("SPECKLE_SECRET");
      if (!String.IsNullOrWhiteSpace(secret))
      {
        Console.WriteLine("decrypting tokens");
        token = Crypto.DecryptStringAES(token, secret);
      }

      server = server.TrimEnd(new[] { '/' });

      if (string.IsNullOrEmpty(server))
        server = GetDefaultServerUrl();

      UserServerInfoResponse userServerInfo = await GetUserServerInfo(token, server);

      var account = new Account()
      {
        token = token,
        refreshToken = null,
        isDefault = GetAccounts().Count() == 0,
        serverInfo = userServerInfo.serverInfo,
        userInfo = userServerInfo.user
      };

      account.serverInfo.url = server;
      return account;
    }

  }
}
