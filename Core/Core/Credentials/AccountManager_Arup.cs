using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.Core.Credentials
{
  /// <summary>
  /// The partial declaration of <see cref="AccountManager"/> which is used for RhinoCompute compatibility
  /// </summary>
  public static partial class AccountManager
  {
    /// <summary>
    /// gets the already created account object from token
    /// </summary>
    /// <remarks>The <see cref="CreateAccountFromToken(string, string, string)"/> must called before get.</remarks>
    public static Account AccountFromToken { get; private set; }

    /// <summary>
    /// Creates an in-memory account by using token
    /// </summary>
    /// <param name="token">the token</param>
    /// <param name="refreshToken">the refresh token</param>
    /// <param name="server">Server to use to create the account</param>
    /// <returns></returns>
    public static async Task<Account> CreateAccountFromToken(string token, string refreshToken, string server)
    {
      AccountFromToken = null;
      AccountFromToken = await CreateAccount(token, refreshToken, server);
      return AccountFromToken;
    }

    /// <summary>
    /// Gets all the accounts present in this environment.
    /// </summary>
    /// <param name="includeAccountFromToken">whether to consider account created using token or not</param>
    /// <returns></returns>
    public static IEnumerable<Account> GetAccounts(bool includeAccountFromToken)
    {
      var accounts = GetAccounts();
      if (includeAccountFromToken && AccountFromToken != null)
      {
        accounts = accounts.Concat(new[] { AccountFromToken });
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
      if (includeAccountFromToken && AccountFromToken != null)
      {
        defaultAccount = AccountFromToken;
      }
      return defaultAccount;
    }
  }
}
