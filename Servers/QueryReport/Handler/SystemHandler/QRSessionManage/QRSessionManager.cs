﻿using System.Collections.Concurrent;
using System.Net;
using GameSpyLib.Common.BaseClass;
using QueryReport.Server;

namespace QueryReport.Handler.SystemHandler.QRSessionManage
{
    public class QRSessionManager : ExpireManagerBase
    {
        public static ConcurrentDictionary<EndPoint, QRSession> Sessions;

        public QRSessionManager()
        {
            Sessions = new ConcurrentDictionary<EndPoint, QRSession>();
        }
        public override void Start()
        {

        }
        public static bool IsSessionExist(EndPoint end)
        {
            return GetSession(end, out _);
        }

        public static bool GetSession(EndPoint end, out QRSession session)
        {
            if (Sessions.TryGetValue(end, out session))
            {
                return true;
            }
            else
            {
                session = null;
                return false;
            }
        }

        public static bool AddSession(QRSession session)
        {
            if(IsSessionExist(session.RemoteEndPoint))
            {
                return false;
            }
            else
            {
                return Sessions.TryAdd(session.RemoteEndPoint, session);
            }
        }
        protected override void CheckExpire()
        {
            base.CheckExpire();

           // todo check expired sessions
        }
    }
}
