﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sentry;
using Speckle.Core.Credentials;
using Serilog;
using Serilog.Sinks.File;
using Serilog.Context;
using Serilog.Events;

namespace Speckle.Core.Logging
{
  /// <summary>
  ///  Anonymous telemetry to help us understand how to make a better Speckle.
  ///  This really helps us to deliver a better open source project and product!
  /// </summary>
  public static class Log
  {
    private static bool _initialized = false;

    public static void Initialize()
    {
      try
      {
        if (_initialized)
          return;

      var dsn = "https://77622965196240e6b5c7e607c1397a68@o141124.ingest.sentry.io/5567027";

        var env = "production";
        var debug = false;
#if DEBUG
        env = "dev";
        dsn = null;
        debug = true;
#endif

        SentrySdk.Init(o =>
        {
          o.Dsn = dsn;
          o.Environment = env;
          o.Debug = debug;
          o.Release = "SpeckleCore@" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
          o.StackTraceMode = StackTraceMode.Enhanced;
          o.AttachStacktrace = true;
        });


        var da = AccountManager.GetDefaultAccount();
        var id = da != null ? da.GetHashedEmail() : "unknown";


        SentrySdk.ConfigureScope(scope =>
        {
          scope.User = new User { Id = id, };
          scope.SetTag("hostApplication", Setup.HostApplication);
        });



        // SeriLog
        var logPath = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        logPath = System.IO.Path.Combine(logPath, "Arup", "Speckle", "Logs", "Grasshopper", "log-.txt");

        Serilog.Log.Logger = new LoggerConfiguration()
          .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
          .CreateLogger();


        _initialized = true;
      }
      catch (Exception ex)
      {
        //swallow
      }
    }


    //capture and make sure Sentry is initialized
    public static void CaptureException(Exception e, SentryLevel level = SentryLevel.Info, List<KeyValuePair<string, object>> extra = null)
    {
      try
      {
        Initialize();

        //ignore infos as they're hogging us
        if (level == SentryLevel.Info)
          return;

        SentrySdk.WithScope(s =>
        {
          s.Level = level;

          if (extra != null)
            s.SetExtras(extra);
          if (e is AggregateException aggregate)
            aggregate.InnerExceptions.ToList().ForEach(ex => SentrySdk.CaptureException(e));
          else
            SentrySdk.CaptureException(e);
        });

      }
      catch (Exception ex)
      {
        //swallow
      }
    }

    public static void AddBreadcrumb(string message)
    {
      try
      {
        Initialize();
        SentrySdk.AddBreadcrumb(message);
      }
      catch (Exception ex)
      {
        //swallow
      }
    }
  }
}
