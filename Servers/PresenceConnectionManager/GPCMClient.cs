﻿using GameSpyLib.Common;
using GameSpyLib.Extensions;
using GameSpyLib.Logging;
using GameSpyLib.Network;
using GameSpyLib.Network.TCP;
using PresenceConnectionManager.Application;
using PresenceConnectionManager.Enumerator;
using PresenceConnectionManager.Structures;
using System;
using System.Collections.Generic;
using System.Net;

namespace PresenceConnectionManager
{
    /// <summary>
    /// Gamespy Client Manager
    /// This class is used to proccess the client login process,
    /// create new user accounts, and fetch profile information
    /// <remarks>gpcm.gamespy.com</remarks>
    /// </summary>
    public class GPCMClient : TCPClientBase, IEquatable<GPCMClient>
    {


        /// <summary>
        /// Indicates whether this player successfully completed the login process
        /// </summary>
        public bool CompletedLoginProcess { get; set; } = false;

        /// <summary>
        /// The profile id parameter that is sent back to the client is initially 2, 
        /// and then 5 everytime after that. So we set here, whether we have sent the 
        /// profile to the client initially (with \id\2) yet.
        /// </summary>
        public bool ProfileSent = false;

        /// <summary>
        /// This boolean checks if the client has received buddy information
        /// </summary>
        public bool BuddiesSent = false;

        /// <summary>
        /// Indicates the date and time this connection was created
        /// </summary>
        public readonly DateTime Created = DateTime.Now;

        /// <summary>
        /// Our CRC16 object for generating Checksums
        /// </summary>
        protected static Crc16 Crc = new Crc16(Crc16Mode.Standard);

        /// <summary>
        /// An Event that is fired when the client successfully logs in.
        /// </summary>
        public static event GPCMConnectionUpdate OnSuccessfulLogin;

        /// <summary>
        /// Event fired when that remote connection logs out, or
        /// the socket gets disconnected. This event will not fire
        /// unless OnSuccessfulLogin event was fired first.
        /// </summary>
        public static event GPCMConnectionClosed OnDisconnect;

        /// <summary>
        /// Event fired when the client status or location is changed,
        /// so the data could be notified to all clients
        /// </summary>
        public static event GPCMStatusChanged OnStatusChanged;

        public GPCMPlayerInfo PlayerInfo { get; protected set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ReadArgs">The Tcp Client connection</param>
        public GPCMClient(TCPStream stream, long connectionid) : base(stream, connectionid)
        {

            PlayerInfo = new GPCMPlayerInfo();

            Stream.OnDataReceived += ProcessData;
            Stream.IsMessageFinished += IsMessageFinished;
            Stream.OnDisconnected += ClientDisconnected;
            Stream.BeginReceive();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GPCMClient()
        {
            if (!Disposed)
                Dispose();
        }

        /// <summary>
        /// Disposes of the client object. The connection is no longer
        /// closed here and the DisconnectByReason even is NO LONGER fired
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            try
            {
                if (disposing)
                {
                    Stream.OnDataReceived -= ProcessData;

                    Stream.IsMessageFinished -= IsMessageFinished;

                    Stream.OnDisconnected -= ClientDisconnected;

                    if (!Stream.SocketClosed)
                        Stream.Close(true);
                }
                //if (PlayerInfo != null)
                //{
                //    PlayerInfo = null;
                //}
            }
            catch { }
            // Preapare to be unloaded from memory
            Disposed = true;
        }

        public override void SendServerChallenge(uint serverID)
        {
            // Only send the login challenge once
            if (PlayerInfo.LoginStatus != LoginStatus.Connected)
            {
                DisconnectByReason(DisconnectReason.ClientChallengeAlreadySent);

                // Throw the error                
                LogWriter.Log.Write("The server challenge has already been sent. Cannot send another login challenge.", LogLevel.Warning);
            }

            // We send the client the challenge key
            ServerChallengeKey = GameSpyLib.Common.Random.GenerateRandomString(10, GameSpyLib.Common.Random.StringType.Alpha);
            PlayerInfo.LoginStatus = LoginStatus.Processing;
            Stream.SendAsync(@"\lc\1\challenge\{0}\id\{1}\final\", ServerChallengeKey, serverID);
        }



        /// <summary>
        /// Main listner loop. Keeps an open stream between the client and server while
        /// the client is logged in / playing
        /// </summary>
        protected override void ProcessData(string message)
        {
            if (message[0] != '\\')
            {
                GameSpyUtils.SendGPError(Stream, GPErrorCode.General, "An invalid request was sended.");
                return;
            }
            string[] commands = message.Split("\\final\\");

            foreach (string command in commands)
            {
                if (command.Length < 1)
                    continue;
                // Read client message, and parse it into key value pairs
                string[] recieved = command.TrimStart('\\').Split('\\');
                Dictionary<string, string> dict = GameSpyUtils.ConvertGPResponseToKeyValue(recieved);

                CommandSwitcher.Switch(this, dict, OnSuccessfulLogin, OnStatusChanged);
            }
        }

        /// <summary>
        /// Event fired when the stream disconnects unexpectedly
        /// </summary>
        protected override void ClientDisconnected()
        {
            DisconnectByReason(DisconnectReason.Disconnected);
        }

        /// <summary>
        /// Logs the client out of the game client, and closes the stream
        /// </summary>
        /// <param name="reason">
        /// The disconnect reason code. 
        /// </param>
        /// <remarks>
        /// If set the <paramref name="reason"/> is set to <see cref="DisconnectReason.ForcedServerShutdown"/>, 
        /// the OnDisconect event will still be called, but the EventArgs objects will NOT be returned to the IO pool. 
        /// You should only set to <see cref="DisconnectReason.ForcedServerShutdown"/> for a planned server shutdown.
        /// </remarks>
        public void DisconnectByReason(DisconnectReason reason)
        {
           
            // Set status and log
            if (PlayerInfo.LoginStatus == LoginStatus.Completed)
            {
                if (reason == DisconnectReason.NormalLogout)
                {
                    ToLog(LogLevel.Info, "Logout", "", "{0} - {1} - {2}", PlayerInfo.PlayerNick, PlayerInfo.PlayerId, RemoteEndPoint);
                }
                else if (reason != DisconnectReason.ForcedServerShutdown)
                {
                    ToLog(
                        LogLevel.Info,
                        "Disconnected", "",
                        "{0} - {1} - {2}, Code={3}",
                        PlayerInfo.PlayerNick,
                        PlayerInfo.PlayerId,
                        RemoteEndPoint,
                        Enum.GetName(typeof(DisconnectReason), reason));
                }
            }

            // Preapare to be unloaded from memory
            PlayerInfo.PlayerStatus = PlayerStatus.Offline;
            PlayerInfo.LoginStatus = LoginStatus.Disconnected;

            Dispose();

            // Call disconnect event
            OnDisconnect?.Invoke(this);
        }

        public bool Equals(GPCMClient other)
        {
            if (other == null) return false;
            return (PlayerInfo.PlayerId == other.PlayerInfo.PlayerId || PlayerInfo.PlayerNick == other.PlayerInfo.PlayerNick);
        }

        public override void Send(string sendingBuffer)
        {
            Stream.SendAsync(sendingBuffer);
        }
        //public override bool Equals(object obj)
        //{
        //    return Equals(obj as GPCMClient);
        //}

        //public override int GetHashCode()
        //{
        //    return (int)PlayerInfo.PlayerId;
        //}

    }
}
