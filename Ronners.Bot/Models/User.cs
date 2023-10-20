using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        public ulong UserId{get;set;}
        public string Username{get;set;}
        public ulong PogCount {get;set;}
        public int RonPoints{get;set;}
        public int Censors{get;set;}

        public User(ulong id, string username, ulong pogcount)
        {
            UserId = id;
            Username = username;
            PogCount = pogcount;
            RonPoints = 0;
            Censors =0;
        }

        public User(ulong id, string username, ulong pogcount,int ronpoints)
        {
            UserId = id;
            Username = username;
            PogCount = pogcount;
            RonPoints = ronpoints;
            Censors =0;
        }

        public User()
        {
            //Required By Dapper
        }

        public string PointString()
        {
            return $"{Username}: {RonPoints}";
        }
    }
}

