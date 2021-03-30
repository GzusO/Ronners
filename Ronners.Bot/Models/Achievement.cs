using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("achievements")]
    public class Achievement
    {
        [Key]
        public ulong AchievementId{get;set;}
        public string Name{get;set;}
        public string Description{get;set;}
        public int Score{get;set;}


        public Achievement()
        {
            //Required By Dapper
        }

        public override string ToString()
        {
            return $"{Name} - {Description} ({Score})";
        }
    }
}

