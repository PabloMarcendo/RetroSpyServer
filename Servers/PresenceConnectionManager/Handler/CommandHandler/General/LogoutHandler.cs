﻿using GameSpyLib.Common.Entity.Interface;
using System.Collections.Generic;

namespace PresenceConnectionManager.Handler.CommandHandler.General
{
    public class LogoutHandler : PCMCommandHandlerBase
    {
        public LogoutHandler(ISession session, Dictionary<string, string> recv) : base(session, recv)
        {
        }

        protected override void DataOperation()
        {
            _session.Disconnect();
            PCMServer.LoggedInSession.Remove(_session.Id, out _);
        }
    }
}
