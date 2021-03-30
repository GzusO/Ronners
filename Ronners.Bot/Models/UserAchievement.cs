using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("userachievements")]
    public class UserAchievement
    {
        [Key]
        public ulong UserAchievementId{get;set;}
        public ulong UserID{get;set;}
        public ulong AchievementId{get;set;}

        public UserAchievement()
        {
            //Required By Dapper
        }
    }
}

