using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.Core.Credentials
{
  public static partial class AccountManager
  {
    private static Account _accountFromToken;

    /// <summary>
    /// Creates an in-memory account by using token
    /// </summary>
    /// <param name="token">the token</param>
    /// <param name="refreshToken">the refresh token</param>
    /// <param name="server">Server to use to create the account</param>
    /// <returns></returns>
    public static async Task<Account> CreateAccountFromToken(string token, string refreshToken, string server)
    {
      _accountFromToken = null;
      _accountFromToken = await CreateAccount(token, refreshToken, server);
      return _accountFromToken;
    }

    /// <summary>
    /// Gets all the accounts present in this environment.
    /// </summary>
    /// <param name="includeAccountFromToken">whether to consider account created using token or not</param>
    /// <returns></returns>
    public static IEnumerable<Account> GetAccounts(bool includeAccountFromToken)
    {
      var accounts = GetAccounts();
      if (includeAccountFromToken && _accountFromToken != null)
      {
        accounts = accounts.Concat(new[] { _accountFromToken });
      }
      return accounts;
    }

    /// <summary>
    /// Gets this environment's default account if any. If there is no default, the first found will be returned and set as default.
    /// </summary>
    /// <param name="includeAccountFromToken">whether to consider account created using token or not</param>
    /// <returns>The default account or null.</returns>
    public static Account GetDefaultAccount(bool includeAccountFromToken)
    {
      var defaultAccount = GetDefaultAccount();
      if (includeAccountFromToken && _accountFromToken != null)
      {
        defaultAccount = _accountFromToken;
      }
      return defaultAccount;
    }
  }
}
