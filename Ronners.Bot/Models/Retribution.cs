using System;
using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("retributions")]
    public class Retribution
    {
        public ulong RetributerUserId{get;set;}
        public ulong RetributeeUserId{get;set;}
        public string Reason{get;set;}
        public int PointsRedistributed{get;set;}
        public int numUsers{get;set;}
        public long Time {get;set;}
        public int Success{get;set;}


        public Retribution(ulong retributer, ulong retributee, string reason, int points, int receivers, long seconds, bool happened)
        {
            RetributerUserId = retributer;
            RetributeeUserId = retributee;
            Reason = reason;
            PointsRedistributed = points;
            numUsers = receivers;
            Time = seconds;
            Success = happened ? 1 : 0;
        }
        public Retribution()
        {
            //Required By Dapper
        }

        public string UtcTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(Time).ToUniversalTime().ToString();
        }
    
        public override string ToString()
        {
            if(Success == 1)
                return String.Format("On {0}: <@!{1}> retributed <@!{2}> for {3}. Resulting in {4} RonPoints being distributed to {5} users.",UtcTime(),RetributerUserId,RetributeeUserId,Reason,PointsRedistributed,numUsers-1);
            return String.Format("On {0}: <@!{1}> attempted to retribute <@!{2}> for {3}.",UtcTime(),RetributerUserId,RetributeeUserId,Reason);
        }
    }
}

