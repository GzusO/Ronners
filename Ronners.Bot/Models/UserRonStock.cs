using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("userronstock")]
    public class UserRonStock
    {
        [Key]
        public ulong UserID{get;set;}
        public string Symbol{get;set;}
        public int Quantity{get;set;}

        public UserRonStock()
        {
            //Required By Dapper
        }
    }
}

