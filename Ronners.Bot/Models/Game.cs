using System.ComponentModel;

namespace Ronners.Bot.Models
{
    public enum Game
    {
        [Description("fetch")]
        Fetch = 1,
        [Description("Chess on a really big board")]
        Chess = 2,
        [Description("Oddball")]
        Oddball = 3
    }
}