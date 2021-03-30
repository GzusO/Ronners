using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("achievementmessages")]
    public class AchievementMessage
    {
        [Key]
        public ulong AchievementMessageId{get;set;}
        public int AchievementType{get;set;}
        public int? IntValue{get;set;}
        public string StringValue{get;set;}
        public bool? BoolValue{get;set;}
        public double? DoubleValue{get;set;}
        public ulong UserID{get;set;}


        public AchievementMessage()
        {
            //Required By Dapper
        }

        public AchievementMessage(AchievementResult result)
        {
            AchievementType = (int)result.AchievementType;
            IntValue = result.IntValue;
            StringValue = result.StringValue;
            BoolValue = result.BoolValue;
            DoubleValue = result.DoubleValue;
            UserID = result.User.Id;
        }
    }
}

