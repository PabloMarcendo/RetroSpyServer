﻿using System;
using System.Net;
using GameSpyLib.Database;
using GameSpyLib.Logging;
using RetroSpyServer.XMLConfig;

namespace RetroSpyServer.Servers
{
    /// <summary>
    /// A factory that create the instance of servers
    /// </summary>
    public class ServerManager : IDisposable
    {
        private DatabaseDriver databaseDriver = null;

        private GPSP.GPSPServer gpspServer = null;

        private GPCM.GPCMServer gpcmServer = null;

        private CDKey.CDKeyServer cdkeyServer = null;

        private ServerBrowser.SBServer sbServer = null;

        private QueryReport.QRServer qrServer = null;

        private NatNeg.NatNegServer nnServer = null;

        private Stats.GStatsServer statsServer = null;
        private PeerChat.ChatServer peerChatServer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerManager()
        {
            Create();
            
        }

        /// <summary>
        /// Default deconstructor
        /// </summary>
        ~ServerManager()
        {
            Dispose();
        }

        /// <summary>
        /// Creates the server factory
        /// </summary>
        /// <param name="engine">The database engine to create</param>
        public void Create()
        {
            DatabaseConfiguration databaseConfiguration = ConfigManager.xmlConfiguration.Database;

            // Determine which database is using and create the database connection
            switch (databaseConfiguration.Type)
            {
                case DatabaseEngine.Mysql:
                    databaseDriver = null;// if using mysql we set this to null to make sure this value is null
                    break; // We don't need to create the connection here because each server will automaticly create it's own MySQL connection.
                case DatabaseEngine.Sqlite:
                    databaseDriver = new SqliteDatabaseDriver("Data Source=" + databaseConfiguration.Databasename + ";Version=3;New=False");
                    break;
                default:
                    throw new Exception("Unknown database engine!");
            }

            try
            {
                databaseDriver?.Connect();
            }
            catch (Exception ex)
            {
                LogWriter.Log.Write(ex.Message, LogLevel.Fatal);
                throw ex;
            }

            LogWriter.Log.Write(LogLevel.Info, "Successfully connected to the {0} database!",  databaseConfiguration.Type);
            LogWriter.Log.Write(LogLevel.Info, "|{0,-11}|{1,-10}|{2,-6}|{3,-14}|",  "-----------", "----------", "------", "--------------");
            LogWriter.Log.Write(LogLevel.Info, "|{0,-11}|{1,-10}|{2,-6}|{3,-14}|",  "Server Name","Host Name", "Port","Max Connection" );
            // Add all servers
            foreach (ServerConfiguration cfg in ConfigManager.xmlConfiguration.Servers)
            {
                StartServer(cfg);
            }
            LogWriter.Log.Write(LogLevel.Info, "|{0,-11}|{1,-10}|{2,-6}|{3,-14}|",  "-----------", "----------", "------", "--------------");
        }

        /// <summary>
        /// Free all resources created by the server factory
        /// </summary>
        public void Dispose()
        {
            StopAllServers();

            databaseDriver?.Dispose();
        }

        /// <summary>
        /// Stop all servers included in the server factory
        /// </summary>
        public void StopAllServers()
        {
            foreach (ServerConfiguration cfg in ConfigManager.xmlConfiguration.Servers)
            {
                StopServer(cfg);
            }
        }

        /// <summary>
        /// Checks if a specific server is running
        /// </summary>
        /// <param name="cfg">The specific server configuration</param>
        /// <returns>true if the server is running, false if the server is not running or the specified server does not exist</returns>
        public bool IsServerRunning(ServerConfiguration cfg)
        {
            switch (cfg.Name)
            {
                case "GPSP":
                    return gpspServer != null && !gpspServer.IsDisposed;
                case "GPCM":
                    return gpcmServer != null && !gpcmServer.IsDisposed;
                case "CDKEY":
                    return cdkeyServer != null && !cdkeyServer.IsDisposed;
                case "SB":
                    return sbServer != null && !sbServer.IsDisposed;
                case "QR":
                    return qrServer != null && !qrServer.IsDisposed;
                case "NATNEG":
                    return nnServer != null && !nnServer.IsDisposed;
                case "GSTATS":
                    return statsServer != null && !statsServer.IsDisposed;
                case "CHAT":
                    return peerChatServer != null && !peerChatServer.IsDisposed;
            }

            return false;
        }

        /// <summary>
        /// Starts a specific server
        /// </summary>
        /// <param name="cfg">The configuration of the specific server to run</param>
        public void StartServer(ServerConfiguration cfg)
        {
            //if (cfg.Disabled)
            //    return;            
            //LogWriter.Log.Write("Starting {2} server at  {0}:{1}.", LogLevel.Info, cfg.Hostname, cfg.Port, cfg.Name);
            //LogWriter.Log.Write("Maximum connections for {0} are {1}.", LogLevel.Info, cfg.Name, cfg.MaxConnections);
            LogWriter.Log.Write(LogLevel.Info, "|{0,-11}|{1,-10}|{2,-6}|{3,14}|", cfg.Name, cfg.Hostname, cfg.Port, cfg.MaxConnections);

            switch (cfg.Name)
            {
                case "GPSP":
                    gpspServer = new GPSP.GPSPServer(cfg.Name,databaseDriver, new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "GPCM":
                    gpcmServer = new GPCM.GPCMServer(cfg.Name, databaseDriver,new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "CDKEY":
                    cdkeyServer = new CDKey.CDKeyServer(cfg.Name, databaseDriver, new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "SB":
                    sbServer = new ServerBrowser.SBServer(cfg.Name,new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "QR":
                    qrServer = new QueryReport.QRServer(cfg.Name, databaseDriver, new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "NATNEG":
                    nnServer = new NatNeg.NatNegServer(cfg.Name,new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "GSTATS":
                    statsServer = new Stats.GStatsServer(cfg.Name, databaseDriver, new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
                case "CHAT":
                    peerChatServer = new PeerChat.ChatServer(cfg.Name, databaseDriver, new IPEndPoint(IPAddress.Parse(cfg.Hostname), cfg.Port), cfg.MaxConnections);
                    break;
            }
        }

        /// <summary>
        /// Stop a specific server
        /// </summary>
        /// <param name="cfg">The configuration of the specific server to stop</param>
        public void StopServer(ServerConfiguration cfg)
        {
            switch (cfg.Name)
            {
                case "GPSP":
                    gpspServer?.Dispose();
                    break;
                case "GPCM":
                    gpcmServer?.Dispose();
                    break;
                case "CDKEY":
                    cdkeyServer?.Dispose();
                    break;
                case "SB":
                    sbServer?.Dispose();
                    break;
                case "QR":
                    qrServer?.Dispose();
                    break;
                case "NATNEG":
                    nnServer?.Dispose();
                    break;
                case "GSTATS":
                    statsServer?.Dispose();
                    break;
                case "CHAT":
                    peerChatServer?.Dispose();
                    break;
            }
        }
    }
}
