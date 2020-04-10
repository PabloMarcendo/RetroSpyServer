using GameSpyLib.Database.DatabaseModel.MySql;
using GameSpyLib.Database.Entity;
using GameSpyLib.Extensions;
using GameSpyLib.Logging;
using GameSpyLib.Network;
using GameSpyLib.RetroSpyConfig;
using Serilog.Events;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;

namespace GameSpyLib.Common
{
    public class ServerManager : IDisposable
    {
        public readonly string RetroSpyVersion = "0.5.1";
        public static ConfigManager Config { get; protected set; }
        public static LogWriter LogWriter { get; protected set; }
        public static ConnectionMultiplexer Redis { get; protected set; }

        public static string ServerName { get; protected set; }

        private IDisposable Server;

        public ServerManager(string serverName, Type serverType)
        {
            ServerName = serverName;
            Server = null;

            LogWriter = new LogWriter(serverName);
            StringExtensions.ShowRetroSpyLogo(RetroSpyVersion);
            LoadDatabaseConfig();
            StartServer(serverType);
        }

        private void StartServer(Type serverType)
        {
            LogWriter.ToLog(LogEventLevel.Information, StringExtensions.FormatServerTableHeader("-----------", "--------------", "------"));
            LogWriter.ToLog(LogEventLevel.Information, StringExtensions.FormatServerTableContext("Server Name", "Host Name", "Port"));
            LogWriter.ToLog(LogEventLevel.Information, StringExtensions.FormatServerTableHeader("-----------", "--------------", "------"));
            // Add all servers
            foreach (ServerConfig cfg in ConfigManager.Config.Servers)
            {
                if (cfg.Name == ServerName)
                {
                    LogWriter.ToLog(LogEventLevel.Information, StringExtensions.FormatServerTableContext(cfg.Name, cfg.ListeningAddress, cfg.ListeningPort.ToString()));
                    LogWriter.ToLog(LogEventLevel.Information, StringExtensions.FormatServerTableHeader("-----------", "--------------", "------"));

                    if (!serverType.IsClass)
                    {
                        throw new InvalidOperationException("Invalid type");
                    }

                    if (serverType.IsSubclassOf(typeof(TemplateTcpServer)))
                    {
                        TemplateTcpServer server = (TemplateTcpServer)Activator.CreateInstance(serverType, IPAddress.Parse(cfg.ListeningAddress), cfg.ListeningPort);

                        server.Start();

                        Server = server;
                    }
                    else if (serverType.IsSubclassOf(typeof(TemplateUdpServer)))
                    {
                        TemplateUdpServer server = (TemplateUdpServer)Activator.CreateInstance(serverType, IPAddress.Parse(cfg.ListeningAddress), cfg.ListeningPort);

                        server.Start();

                        Server = server;
                    }
                    else
                    {
                        throw new Exception("Invalid class type");
                    }

                    LogWriter.ToLog(LogEventLevel.Information, "Server is successfully started! ");
                    return;
                }
            }

            throw new Exception("Invalid server specified!");
        }

        private void LoadDatabaseConfig()
        {
            DatabaseConfig dbConfig = ConfigManager.Config.Database;
            // Determine which database is using and create the database connection

            switch (ConfigManager.Config.Database.Type)
            {
                case DatabaseEngine.MySql:
                    string mySqlConnStr =
                        string.Format(
                            "Server={0};Database={1};Uid={2};Pwd={3};Port={4};SslMode={5};SslCert={6};SslKey={7};SslCa={8}",
                            dbConfig.RemoteAddress, dbConfig.DatabaseName, dbConfig.UserName, dbConfig.Password,
                            dbConfig.RemotePort, dbConfig.SslMode, dbConfig.SslCert, dbConfig.SslKey, dbConfig.SslCa);
                    retrospyContext.RetroSpyMySqlConnStr = mySqlConnStr;
                    using (var db = new retrospyContext())
                    {
                        db.Users.Where(u => u.Userid == 0);
                    }
                    break;
                case DatabaseEngine.SQLite:
                    string SQLiteConnStr = "Data Source=" + dbConfig.DatabaseName + ";Version=3;New=False";

                    break;
                default:
                    throw new Exception("Unknown database engine!");
            }
            LogWriter.Log.Information($"Successfully connected to {dbConfig.Type}!");

            RedisConfig redisConfig = ConfigManager.Config.Redis;
            Redis = ConnectionMultiplexer.Connect(redisConfig.RemoteAddress + ":" + redisConfig.RemotePort.ToString());
            LogWriter.Log.Information($"Successfully connected to Redis!");

        }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}
