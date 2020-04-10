﻿using GameSpyLib.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace GameSpyLib.Extensions
{
    public enum RedisDBNumber
    {
        DedicatedServer = 0,
        PeerGroup = 1,
        PeerServer = 2
    }

    public class RedisExtensions
    {
        #region General Methods
        public static bool SerializeSet<T>(string key, T value, int dbNumber)
        {
            var redis = ServerManager.Redis.GetDatabase(dbNumber);
            string jsonStr = JsonConvert.SerializeObject(value);
            return redis.StringSet(key, jsonStr);
        }

        public static T SerilizeGet<T>(string key, int dbNumber)
        {
            var redis = ServerManager.Redis.GetDatabase(dbNumber);
            T t = JsonConvert.DeserializeObject<T>(redis.StringGet(key));
            return t;
        }

        /// <summary>
        /// Search our sub key in database get full keys which contain sub key
        /// </summary>
        /// <param name="subStringOfKey">the substring of a key</param>
        /// <returns></returns>
        public static List<string> SearchKeys(string subStringOfKey, int dbNumber)
        {
            List<string> matchKeys = new List<string>();

            foreach (var end in ServerManager.Redis.GetEndPoints())
            {
                var server = ServerManager.Redis.GetServer(end);
                foreach (var key in server.Keys(dbNumber, pattern: $"*{subStringOfKey}*"))
                {
                    matchKeys.Add(key);
                }
            }
            return matchKeys;
        }
        #endregion

        public static List<string> SearchPeerServerKeys(string gameName)
        {
            return SearchKeys(gameName, (int)RedisDBNumber.PeerServer);
        }

        //public static bool DeletePeerServer(EndPoint endPoint, string gameName)
        //{
        //    string key = GeneratePeerGameServerKey(endPoint, gameName);
        //    var redis = ServerManagerBase.Redis.GetDatabase((int)RedisDBNumber.PeerServer);
        //    return redis.KeyDelete(key);
        //}

        //public static string GeneratePeerGameServerKey(EndPoint end, string gameName)
        //{
        //    return GenerateDedicatedGameServerKey(end, gameName);
        //}
        //public static void UpdatePeerGameServer<T>(EndPoint end, string gameName, T gameServer)
        //{
        //    string key = GenerateDedicatedGameServerKey(end, gameName);
        //    SerializeSet(key, gameServer, (int)RedisDBNumber.PeerServer);
        //}

        //public static List<T> GetPeerGameServers<T>(string subKey)
        //{
        //    List<string> allServerKeys = SearchKeys(subKey, (int)RedisDBNumber.PeerServer);
        //    List<T> gameServer = new List<T>();
        //    foreach (var key in allServerKeys)
        //    {
        //        gameServer.Add(SerilizeGet<T>(key, (int)RedisDBNumber.DedicatedServer));
        //    }
        //    return gameServer;
        //}
    }
}
