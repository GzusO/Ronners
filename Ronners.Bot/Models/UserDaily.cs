using System;
using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("userdaily")]
    public class UserDaily
    {
        [Key]
        public ulong UserID{get;set;}
        public long LastCheckIn{get;set;}
        public int Streak{get;set;}

        public UserDaily()
        {
            //Required By Dapper
        }
    }
}

