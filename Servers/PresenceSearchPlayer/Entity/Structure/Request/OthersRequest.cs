﻿using System.Collections.Generic;
using PresenceSearchPlayer.Entity.Enumerator;
using PresenceSearchPlayer.Entity.Structure.Model;

namespace PresenceSearchPlayer.Entity.Structure.Request
{
    public class OthersRequest : PSPRequestBase
    {
        public OthersRequest(Dictionary<string, string> recv) : base(recv)
        {
        }

        public uint ProfileID { get; private set; }
        public string GameName { get; private set; }
        public uint NamespaceID { get; protected set; }
        public override GPError  Parse()
        {
            var flag = base.Parse();
            if (flag != GPError.NoError)
            {
                return flag;
            }

            if (!_rawRequest.ContainsKey("gamename"))
            {
                return GPError.Parse;
            }

            if (!_rawRequest.ContainsKey("profileid") || !_rawRequest.ContainsKey("namespaceid"))
            {
                return GPError.Parse;
            }

            uint profileID = 0;
            if (!_rawRequest.ContainsKey("profileid") && !uint.TryParse(_rawRequest["profileid"], out profileID))
            {
                return GPError.Parse;

            }

            if (_rawRequest.ContainsKey("namespaceid"))
            {
                uint namespaceID;
                if (!uint.TryParse(_rawRequest["namespaceid"], out namespaceID))
                {
                    return GPError.Parse;
                }

                NamespaceID = namespaceID;
            }

            ProfileID = profileID;
            GameName = _rawRequest["gamename"];
            return GPError.NoError;
        }
    }
}
