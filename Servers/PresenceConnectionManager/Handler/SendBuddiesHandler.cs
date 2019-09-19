﻿using PresenceConnectionManager.DatabaseQuery;
using PresenceConnectionManager.Enumerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace PresenceConnectionManager.Handler
{
    public class SendBuddiesHandler
    {
        public static void HandleSendBuddies(GPCMClient client,Dictionary<string,string> recv)
        {
            // \bdy\<number of friends>\list\<array of profileids>\
            //TODO


            //total number of friends
            // we have to separate friends by productid,namespaceid,partnerid,gamename 
            //because you will have different friends in different game

            

            if (client.BuddiesSent)
                return;

            /*Stream.SendAsync(
                @"\bdy\1\list\2,\final\");

            Stream.SendAsync(
            //    @"\bm\100\f\2\msg\|s|0|ss|Offline\final\"
            @"\bm\100\f\2\msg\Messaggio di prova|s|2|ss|Home|ls|locstr://Reversing the world...|\final\"
            );*/
            
            client.Stream.SendAsync(@"\bdy\1\list\13\final\");
            client.Stream.SendAsync(@"\bm\100\f\13\msg\|s|0|ss|Offline\final\");
            client.Stream.SendAsync(@"\bm\100\f\13\msg\1|signed|1");

            return;
           
            int[] pids = SendBuddiesQuery.GetProfileidArray(recv);
            int numBuddies = pids.Length;
            client.BuddiesSent = true;

            string sendingBuffer;
            string profileidArray = "";
            for (int i = 0; i < numBuddies; i++)
            {
                profileidArray += pids[i].ToString();
            }
            sendingBuffer = string.Format(@"\bdy\{0}\list\{1}\final\", numBuddies,profileidArray);
            client.Stream.SendAsync(sendingBuffer);
            

        }
        public static void SendBuddyInfo(uint profileid)
        {
            bool isBlocked = false;
            Dictionary<string, object> profile = SendBuddiesQuery.GetProfile(profileid);
            bool.TryParse(profile["deleted"] as string, out isBlocked);
            string locstr = profile["location"].ToString();
            string statstr;
            string sendingBuffer;
            if (isBlocked)
            {
                statstr = @"|s|0|ss|Offline";
            }
            else
            {
                statstr = string.Format(@"|s|{0}|ss|{1}{2}{3}|ip|{4}|p|{5}|qm|{6}",
                    profile["status"], profile["statusstring"], locstr != "0" ? "|ls|" : "",locstr,profile["lastip"],profile["port"],profile["quietflags"]);
            }
            sendingBuffer = string.Format(@"\bm\{0}\f\{1}\msg{2}",GPEnum.BmStatus,profileid,statstr);
        }


        public static void SendAddRequest()
        {
            //char query[256];
            //sprintf_s(query, sizeof(query), "SELECT `profileid`,`syncrequested`,`reason` FROM `Presence`.`addrequest` WHERE `targetid` = %d", getProfileID());
            //MYSQL_RES* res;
            //MYSQL_ROW row;
            //sentAddRequests = true;
            //if (mysql_query(server.conn, query))
            //{
            //    fprintf(stderr, "%s\n", mysql_error(server.conn));
            //    return;
            //}
            //res = mysql_store_result(server.conn);
            //while ((row = mysql_fetch_row(res)) != NULL)
            //{
            //    formatSend(sd, true, 0, "\\bm\\%d\\f\\%d\\msg\\%s|signed|%s", GPI_BM_REQUEST, atoi(row[0]), row[2], row[1]);
            //    //formatSend(c->sd,true,0,"\\bm\\%d\\f\\%d\\msg\\%s|signed|%s",GPI_BM_REQUEST,profileid,reason,signature);
            //}
            //mysql_free_result(res);
        }
    }
}