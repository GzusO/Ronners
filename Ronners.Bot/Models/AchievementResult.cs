using Discord;
using Discord.Commands;
namespace Ronners.Bot.Models
{
    public class AchievementResult : RuntimeResult
    {
        public AchievementType AchievementType{get;set;}
        public int? IntValue{get;set;}
        public string StringValue{get;set;}
        public bool? BoolValue{get;set;}
        public double? DoubleValue{get;set;}
        public IUser User{get;set;}
        public AchievementResult(CommandError? error, string reason) : base(error, reason)
        {
        }
        public static AchievementResult FromError(string reason) =>
            new AchievementResult(CommandError.Unsuccessful, reason);
        public static AchievementResult FromSuccess(string reason = null) =>
            new AchievementResult(null, reason);
    }

    public enum AchievementType
    {
        Undefined = 0,
        Roll,
        User,
        EightBall,
        Ideas,
        RonPoints,
        Response,
        Slots,
        Baccarat,
        Roulette,
        TicTacToe,
        Captcha,
        Slurp,
        Daily
    }
}