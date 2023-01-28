using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("useritem")]
    public class UserItem
    {
        [Key]
        public ulong UserID{get;set;}
        public int ItemID{get;set;}
        public int Quantity{get;set;}

        public UserItem()
        {
            //Required By Dapper
        }
    }
}

