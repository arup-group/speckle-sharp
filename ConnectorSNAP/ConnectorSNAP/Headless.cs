using DesktopUI2.Models;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public class Headless
  {
    //public static Func<string, string, SpeckleInterface.IStreamReceiver> streamReceiverCreationFn
    //  = ((url, token) => new SpeckleInterface.StreamReceiver(url, token, ProgressMessenger));
    ////public static Func<string, string, SpeckleInterface.IStreamSender> streamSenderCreationFn = ((url, token) => new SpeckleInterface.StreamSender(url, token, ProgressMessenger));
    //public static Func<string, string, SpeckleInterface.IStreamSender> streamSenderCreationFn;
    //public static IProgress<MessageEventArgs> loggingProgress = new Progress<MessageEventArgs>();
    //public static SpeckleInterface.ISpeckleAppMessenger ProgressMessenger = new ProgressMessenger(loggingProgress);

    //private Dictionary<string, string> arguments = new Dictionary<string, string>();
    private string cliMode = "";

    public string EmailAddress { get; private set; }
    public string RestApi { get; private set; }
    public string ApiToken { get; private set; }

    private UserInfo userInfo;

    [STAThread]
    public bool RunCLI(params string[] args)
    {
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

      Instance.SnapModel = new SnapModel();

      var argPairs = new Dictionary<string, string>();

      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(Applications.SNAP);

      IProgress<string> loggingProgress = new Progress<string>();
      //TO DO: add logging to console

      //A simplified one just for use by the proxy class
      var proxyLoggingProgress = new Progress<string>();
      proxyLoggingProgress.ProgressChanged += (object o, string e) =>
      {
        loggingProgress.Report(e);
      };

      Console.WriteLine("");

      cliMode = args[0];
      if (cliMode == "-h")
      {
        Console.WriteLine("\n");
        Console.WriteLine("Usage: ConnectorSNAP.exe <command>\n\n" +
          "where <command> is one of: receiver, sender\n\n");
        Console.Write("ConnectorGSA.exe <command> -h\thelp on <command>\n");
        return true;
      }
      if (cliMode != "receiver" && cliMode != "sender")
      {
        Console.WriteLine("Unable to parse command");
        return false;
      }

      var sendReceive = (cliMode == "receiver") ? SendReceive.Receive : SendReceive.Send;

      #region display_h_info
      if (sendReceive == SendReceive.Receive && argPairs.ContainsKey("h"))
      {
        Console.WriteLine("\n");
        Console.WriteLine("Usage: ConnectorGSA.exe receiver\n");
        Console.WriteLine("\n");
        Console.Write("Required arguments:\n");
        Console.Write("--file <path>\t\t\tFile to save to. If file does not exist, a new one will be created\n");
        Console.Write("--streamIDs <streamIDs>\t\tComma-delimited ID of streams to be received\n");
        Console.WriteLine("\n");
        Console.Write("Optional arguments:\n");
        Console.Write("--nodeAllowance <distance>\tMax distance before nodes are not merged\n");
        return true;
      }
      else if (sendReceive == SendReceive.Send && argPairs.ContainsKey("h"))
      {
        Console.WriteLine("\n");
        Console.WriteLine("Usage: ConnectorGSA.exe sender\n");
        Console.WriteLine("\n");
        Console.Write("Required arguments:\n");
        Console.Write("--file <path>\t\t\tFile path to open\n");
        Console.WriteLine("\n");
        Console.Write("Optional arguments:\n");
        Console.Write("--saveAs <path>\t\t\tFile path to save file with stream information.  Default is to use file\n");
        Console.Write("--designLayerOnly\t\tIgnores analysis information.  Default is to send all data from both layers\n");
        Console.Write("--sendAllNodes\t\t\tSend all nodes in model. Default is to send only 'meaningful' nodes\n");
        Console.Write("--result <options>\t\tType of result to send. Each input should be in quotation marks. Comma-delimited\n");
        Console.Write("--resultCases <cases>\t\tCases to extract results from. Comma-delimited\n");
        Console.Write("--resultInLocalAxis\t\tSend results calculated at the local axis. Default is global\n");
        Console.Write("--result1DNumPosition <num>\tNumber of additional result points within 1D elements\n");
        return true;
      }
      #endregion

      #region create_argpairs
      for (int index = 1; index < args.Length; index += 2)
      {
        string arg = args[index].Replace("-", "");
        if (args.Length <= index + 1 || args[index + 1].StartsWith("-"))
        {
          argPairs.Add(arg, "true");
          index--;
        }
        else
        {
          argPairs.Add(arg, args[index + 1].Trim(new char[] { '"' }));
        }
      }

      if (!argPairs.ContainsKey("file"))
      {
        Console.WriteLine("Missing --file argument");
        return false;
      }
      #endregion

      // Login
      if (argPairs.ContainsKey("email"))
      {
        EmailAddress = argPairs["email"];
      }
      if (argPairs.ContainsKey("server"))
      {
        RestApi = argPairs["server"];
      }
      if (argPairs.ContainsKey("token"))
      {
        ApiToken = argPairs["token"];
      }

      Account account;
      if (string.IsNullOrEmpty(RestApi) || string.IsNullOrEmpty(ApiToken))
      {
        Console.WriteLine("Retrieving default account stored on this machine");

        account = AccountManager.GetDefaultAccount();
        userInfo = account.userInfo;
      }
      else
      {
        Console.WriteLine("Retrieving matching account stored on this machine");

        userInfo = AccountManager.GetUserInfo(ApiToken, RestApi).Result;
        account = AccountManager.GetAccounts().FirstOrDefault(a => a.userInfo.id == userInfo.id);
      }

      var client = new Client(account);

      #region file
      // GSA File
      var fileArg = argPairs["file"];

      var filePath = fileArg.StartsWith(".") ? Path.Combine(AssemblyDirectory, fileArg) : fileArg;

      var fileDir = Path.GetDirectoryName(filePath);
      if (!Directory.Exists(fileDir))
      {
        Console.WriteLine("Could not locate directory: " + filePath);
        //sending needs the file to exist
        return false;
      }

      //If receiving, then it's valid for a file name not to exist - in this case, it's the file name that a new file should be saved as
      if (!File.Exists(filePath) && sendReceive == SendReceive.Send)
      {
        Console.WriteLine("Could not locate file: " + filePath);
        //sending needs the file to exist
        return false;
      }

      var saveAsFilePath = (argPairs.ContainsKey("saveAs")) ? argPairs["saveAs"] : filePath;

      if (sendReceive == SendReceive.Receive)
      {
        ((SnapProxy)Instance.SnapModel.Proxy).NewFile(false);

        //Instance.SnapModel.Messenger.Message(MessageIntent.Display, MessageLevel.Information, "Created new file.");

        //Ensure this new file has a file name, and internally sets the file name in the proxy
        ((SnapProxy)Instance.SnapModel.Proxy).SaveAs(saveAsFilePath);
      }
      else
      {
        Commands.OpenFile(filePath, false);
      }
      #endregion

      if (!ArgsToSettings(sendReceive, argPairs))
      {
        return false;
      }

      var streamStates = new List<StreamState>();
      bool cliResult = false;
      if (sendReceive == SendReceive.Receive)
      {
        var streamIds = argPairs["streamIDs"].Split(new char[] { ',' });

        var topLevelObjects = new List<Base>();

        Console.WriteLine("Attempting to receive from stream " + string.Join(", ", streamIds));

        //There seem to be some issues with HTTP requests down the line if this is run on the initial (UI) thread, so this ensures it runs on another thread
        cliResult = Task.Run(() =>
        {
          //Load data to cause merging
          Commands.LoadDataFromFile(null);

          foreach (var streamId in streamIds)
          {
            var streamState = new StreamState(account, new Speckle.Core.Api.Stream() { id = streamId });

            Console.WriteLine("Retrieving information about stream " + streamId + " from the server");

            streamState.CachedStream = streamState.Client.StreamGet(streamState.StreamId).Result;

            streamState.CachedStream.branch = client.StreamGetBranches(streamId, 1).Result.First();
            var commitId = streamState.CachedStream.branch.commits.items.FirstOrDefault().referencedObject;
            var transport = new ServerTransport(streamState.Client.Account, streamState.StreamId);

            Console.WriteLine("Retrieving objects in stream " + streamId + " from the server");

            var received = Commands.Receive(commitId, transport, topLevelObjects).Result;

            streamStates.Add(streamState);
          }

          Console.WriteLine("Converting objects into SNAP records in .s8i format");

          Commands.ConvertToNative(topLevelObjects, converter);

          //The cache is filled with natives
          if (Instance.SnapModel.Cache.GetNatives(out var snapRecords) && snapRecords != null && snapRecords.Count > 0)
          {
            Console.WriteLine("Writing " + snapRecords.Count() + " SNAP records in .s8i format to " + saveAsFilePath);

            ((SnapProxy)Instance.SnapModel.Proxy).WriteModel(snapRecords);

            var saved = ((SnapProxy)Instance.SnapModel.Proxy).SaveAs(saveAsFilePath);

            Console.WriteLine("Receiving complete");

            return saved;
          }
          else
          {
            Console.WriteLine("Conversion resulted in no records to be written to file");
            return true;
          }
        }).Result;
      }
      else //Send
      {
        //Not supported yet
      }

      return cliResult;
    }

    private async Task<Speckle.Core.Api.Stream> NewStream(Client client, string streamName, string streamDesc)
    {
      string streamId = "";

      try
      {
        streamId = await client.StreamCreate(new StreamCreateInput()
        {
          name = streamName,
          description = streamDesc,
          isPublic = false
        });

        return await client.StreamGet(streamId);

      }
      catch (Exception e)
      {
        try
        {
          if (!string.IsNullOrEmpty(streamId))
          {
            await client.StreamDelete(streamId);
          }
        }
        catch
        {
          // POKEMON! (server is prob down)
        }
      }

      return null;
    }

    private enum SendReceive
    {
      Send,
      Receive
    }

    private bool ArgsToSettings(SendReceive sendReceive, Dictionary<string, string> argPairs)
    {
      //This will create the logger
      Instance.SnapModel.LoggingMinimumLevel = 4; //Debug
      //TO DO: enable is as a command line argument
      Instance.SnapModel.Units = "m";

      if (sendReceive == SendReceive.Receive)
      {
        if (!argPairs.ContainsKey("streamIDs"))
        {
          Console.WriteLine("Missing -streamIDs argument");
          return false;
        }

        if (argPairs.ContainsKey("nodeAllowance") && double.TryParse(argPairs["nodeAllowance"], out double nodeAllowance))
        {
          Instance.SnapModel.CoincidentNodeAllowance = nodeAllowance;
        }
      }
      else if (sendReceive == SendReceive.Send)
      {
        //Not supported yet
      }
      return true;
    }

    #region Log
    [DllImport("Kernel32.dll")]
    public static extern bool AttachConsole(int processId);

    /// <summary>
    /// Message handler.
    /// </summary>
    private void ProcessMessage(object sender, MessageEventArgs e)
    {
      if (e.Level == MessageLevel.Debug || e.Level == MessageLevel.Information)
      {
        Console.WriteLine("[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + string.Join(" ", e.MessagePortions.Where(mp => !string.IsNullOrEmpty(mp))));
      }
      else
      {
        Console.WriteLine("[" + DateTime.Now.ToString("h:mm:ss tt") + "] ERROR: " + string.Join(" ", e.MessagePortions.Where(mp => !string.IsNullOrEmpty(mp))));
      }
    }
    #endregion

    private static string AssemblyDirectory
    {
      get
      {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }
  }
}
