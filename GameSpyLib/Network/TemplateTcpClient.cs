﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameSpyLib.Common;
using GameSpyLib.Extensions;
using GameSpyLib.Logging;
using GameSpyLib.RetroSpyConfig;
using Serilog.Events;
using TcpClient = NetCoreServer.TcpClient;

namespace GameSpyLib.Network
{

    public class TemplateTcpClient : TcpClient
    {
        private EndPoint _endPoint;
        /// <summary>
        /// We automatic connect to remote server address
        /// </summary>
        public TemplateTcpClient() : base
            (
                ConfigManager.Config.Servers.Where(s => s.Name == ServerManager.ServerName).First().RemoteAddress
               , ConfigManager.Config.Servers.Where(s => s.Name == ServerManager.ServerName).First().RemotePort
            )
        {
        }

        protected override void OnConnected()
        {
            _endPoint = Socket.RemoteEndPoint;
            ToLog(LogEventLevel.Information, $"[Proxy] [Conn] IRC server: {_endPoint}");
        }

        protected override void OnDisconnected()
        {
            ToLog(LogEventLevel.Information, $"[Proxy] [Disc] IRC server: {_endPoint}");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            ToLog(LogEventLevel.Debug,
                $"[Proxy] [Recv] {StringExtensions.ReplaceUnreadableCharToHex(buffer, 0, (int)size)}");

            byte[] tempBuffer = new byte[size];
            Array.Copy(buffer, 0, tempBuffer, 0, size);
            OnReceived(tempBuffer);
        }

        protected virtual void OnReceived(byte[] buffer)
        {
            OnReceived(Encoding.ASCII.GetString(buffer));
        }
        protected virtual void OnReceived(string buffer)
        {
        }

        public override bool SendAsync(byte[] buffer, long offset, long size)
        {
            ToLog(LogEventLevel.Debug,
                $"[Proxy] [Send] {StringExtensions.ReplaceUnreadableCharToHex(buffer, 0, (int)size)}");
            return base.SendAsync(buffer, offset, size);
        }

        public override long Send(byte[] buffer, long offset, long size)
        {
            ToLog(LogEventLevel.Debug,
                $"[Proxy] [Send] {StringExtensions.ReplaceUnreadableCharToHex(buffer, 0, (int)size)}");
            return base.Send(buffer, offset, size);
        }

        protected override void OnError(SocketError error)
        {
            ToLog(LogEventLevel.Error, error.ToString());
        }

        public virtual void ToLog(LogEventLevel level, string text)
        {
            LogWriter.ToLog(level, $"[{ServerManager.ServerName}] " + text);
        }
    }
}
