﻿using System.Collections.Generic;
using PresenceSearchPlayer.Entity.Enumerator;

namespace PresenceSearchPlayer.Entity.Structure.Model
{
    public class PSPRequestBase
    {
        protected Dictionary<string, string> _recv;

        public uint NamespaceID { get; protected set; }
        public ushort OperationID { get; protected set; }

        public PSPRequestBase(Dictionary<string, string> recv)
        {
            _recv = recv;
        }

        public virtual GPError Parse()
        {
            if (_recv.ContainsKey("id"))
            {
                ushort operationID;
                if (!ushort.TryParse(_recv["id"], out operationID))
                {
                    return GPError.Parse;
                }
                OperationID = operationID;
            }

            if (_recv.ContainsKey("namespaceid"))
            {
                uint namespaceID;
                if (!uint.TryParse(_recv["namespaceid"], out namespaceID))
                {
                    return GPError.Parse;
                }

                NamespaceID = namespaceID;
            }

            return GPError.NoError;
        }
    }
}