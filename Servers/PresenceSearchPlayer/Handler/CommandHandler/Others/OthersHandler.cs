﻿using GameSpyLib.Database.DatabaseModel.MySql;
using PresenceSearchPlayer.Enumerator;
using System.Collections.Generic;
using System.Linq;

namespace PresenceSearchPlayer.Handler.CommandHandler.Others
{
    /// <summary>
    /// Get buddy's information
    /// </summary>
    public class OthersHandler : GPSPHandlerBase
    {
        public OthersHandler(Dictionary<string, string> recv) : base(recv)
        {
        }
        uint _profileid;

        protected override void CheckRequest(GPSPSession session)
        {
            if (!_recv.ContainsKey("profileid") || !_recv.ContainsKey("namespaceid"))
            {
                _errorCode = GPErrorCode.Parse;
            }

            if (!_recv.ContainsKey("profileid") && !uint.TryParse(_recv["profileid"], out _profileid))
            {
                _errorCode = GPErrorCode.Parse;
            }

            base.CheckRequest(session);
        }

        protected override void DataBaseOperation(GPSPSession session)
        {
            using (var db = new RetrospyDB())
            {
                var info = from b in db.Friends
                           where b.Profileid == _profileid && b.Namespaceid == _namespaceid
                           select b.Targetid;

                _sendingBuffer = @"\others\";
                foreach (var pid in info)
                {
                    var b = from p in db.Profiles
                            join n in db.Namespaces on p.Profileid equals n.Profileid
                            join u in db.Users on p.Userid equals u.Userid
                            where n.Namespaceid == _namespaceid && n.Profileid == pid && n.Gamename == _recv["gamename"]
                            select new { profileid = p.Profileid, nick = p.Nick, uniquenick = n.Uniquenick, last = p.Lastname, first = p.Firstname, email = u.Userid };
                    _sendingBuffer += @"\o\" + b.First().profileid;
                    _sendingBuffer += @"\nick\" + b.First().nick;
                    _sendingBuffer += @"\uniquenick\" + b.First().uniquenick;
                    _sendingBuffer += @"\first\" + b.First().first;
                    _sendingBuffer += @"\last\" + b.First().last;
                    _sendingBuffer += @"\email\" + b.First().email;
                }
                _sendingBuffer += @"\odone\final\";
            }
        }

        protected override void ConstructResponse(GPSPSession session)
        {
            if (_errorCode == GPErrorCode.DatabaseError)
            {
                _sendingBuffer = @"\others\\odone\final\";
                return;
            }
        }
    }
}