using System;
using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("ronkey")]
    public class RonKey
    {
        public int KeyId{get;set;}
        public ulong UserID{get;set;}
        public string Key {get;set;} 
        public string Source {get;set;}
        public int Used{get;set;}

        public RonKey(ulong user, string key, string source)
        {
            UserID = user;
            Used = 0;
            Source =source;
            Key=key;
        }
        public RonKey()
        {
            //Required By Dapper
        }

    }
}

