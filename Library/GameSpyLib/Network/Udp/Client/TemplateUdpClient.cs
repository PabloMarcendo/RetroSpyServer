﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UdpClient = NetCoreServer.UdpClient;

namespace GameSpyLib.Network.Udp
{
    public class TemplateUdpClient : UdpClient
    {

        public TemplateUdpClient(IPAddress address, int port) : base(address, port)
        {
        }
        protected override void OnConnected()
        {
            Console.WriteLine($"Echo UDP client connected a new session with Id {Id}");

            // Start receive datagrams
            ReceiveAsync();

        }
        protected override void OnDisconnected()
        {
            Console.WriteLine($"Echo UDP client disconnected a session with Id {Id}");
            base.OnConnected();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            Console.WriteLine("Incoming: " + Encoding.ASCII.GetString(buffer, (int)offset, (int)size));

            // Continue receive datagrams
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Echo UDP client caught an error with code {error}");
        }

    }
}
