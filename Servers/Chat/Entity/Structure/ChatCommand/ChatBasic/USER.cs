﻿namespace Chat.Entity.Structure.ChatCommand
{
    public class USER : ChatCommandBase
    {
        public string UserName { get; protected set; }
        public string IPAddress { get; protected set; }
        public string ServerName { get; protected set; }
        public string NickName { get; protected set; }

        public override string BuildRPL(params string[] cmdParam)
        {
            return new PING().BuildRPL();
        }

        public override bool Parse(string request)
        {
            if (!base.Parse(request))
            {
                return false;
            }

            UserName = _cmdParams[0];
            IPAddress = _cmdParams[1];
            ServerName = _cmdParams[2];
            NickName = _longParam;
            return true;
        }
    }
}
