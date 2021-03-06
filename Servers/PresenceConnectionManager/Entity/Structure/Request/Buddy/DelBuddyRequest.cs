﻿using System;
using System.Collections.Generic;
using PresenceConnectionManager.Entity.BaseClass;
using PresenceSearchPlayer.Entity.Enumerator;

namespace PresenceConnectionManager.Entity.Structure.Request.Buddy
{
    public class DelBuddyRequest : PCMRequest
    {
        //\delbuddy\\sesskey\<>\delprofileid\<>\final\
        public uint DeleteProfileID { get; protected set; }
        public DelBuddyRequest(Dictionary<string, string> recv) : base(recv)
        {
        }

        public override GPError  Parse()
        {
            var flag = base.Parse();
            if (flag != GPError.NoError)
            {
                return flag;
            }
            if (!_recv.ContainsKey("delprofileid"))
            {

                return GPError.Parse;
            }

            uint deleteProfileID;
            if (!uint.TryParse(_recv["delprofileid"], out deleteProfileID))
            {
                return GPError.Parse;
            }

            DeleteProfileID = deleteProfileID;
            return GPError.NoError;
        }
    }
}
