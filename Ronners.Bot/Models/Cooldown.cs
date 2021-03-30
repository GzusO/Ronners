using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("cooldowns")]
    public class Cooldown
    {
        [Key]
        public string Command{get;set;}
        public long LastExecuted{get;set;}


        public Cooldown(string command, long time)
        {
            Command= command;
            LastExecuted = time;
        }
        public Cooldown()
        {
            //Required By Dapper
        }
    }
}

