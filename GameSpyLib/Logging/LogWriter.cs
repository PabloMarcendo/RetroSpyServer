﻿using GameSpyLib.Common;
using GameSpyLib.RetroSpyConfig;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace GameSpyLib.Logging
{

    /// <summary>
    /// Provides an object wrapper for a file that is used to
    /// store LogMessage's into. Uses a Multi-Thread safe Queueing
    /// system, and provides full Asynchronous writing and flushing
    /// </summary>
    public class LogWriter
    {

        public Logger Log { get; protected set; }

        public LogWriter(string serverName)
        {
            switch (ConfigManager.Config.MinimumLogLevel)
            {
                case LogEventLevel.Verbose:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
                case LogEventLevel.Information:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
                case LogEventLevel.Debug:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
                case LogEventLevel.Warning:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
                case LogEventLevel.Error:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
                case LogEventLevel.Fatal:
                    Log = new LoggerConfiguration()
                .MinimumLevel.Fatal()
                .WriteTo.Console(
               outputTemplate: "{Timestamp:[HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}")
                .WriteTo.File($"Logs/[{serverName}]-.log",
                outputTemplate: "{Timestamp:[yyyy-MM-dd HH:mm:ss]} [{Level:u4}] {Message:}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                    break;
            }
        }

        /// <summary>
        /// Convient to print log
        /// </summary>
        /// <param name="level"></param>
        /// <param name="error"></param>
        public static void ToLog(LogEventLevel level, string error)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    ServerManager.LogWriter.Log.Verbose(error);
                    break;
                case LogEventLevel.Information:
                    ServerManager.LogWriter.Log.Information(error);
                    break;
                case LogEventLevel.Debug:
                    ServerManager.LogWriter.Log.Debug(error);
                    break;
                case LogEventLevel.Error:
                    ServerManager.LogWriter.Log.Error(error);
                    break;
                case LogEventLevel.Fatal:
                    ServerManager.LogWriter.Log.Fatal(error);
                    break;
                case LogEventLevel.Warning:
                    ServerManager.LogWriter.Log.Warning(error);
                    break;
            }
        }

        public static void ToLog(string message)
        {
            ToLog(LogEventLevel.Information, message);
        }
        public static void WriteException(Exception e)
        {
            ToLog(e.ToString());
        }
    }

}
