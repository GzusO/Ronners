using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;
using System;

namespace Ronners.Bot.Modules
{
    [Group("slots")]
    public class SlotsModule : ModuleBase<SocketCommandContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public SlotService SlotService{get;set;}

        [Command("bet")]
        public async Task BetAsync(int betAmount)
        {
            var user = await GameService.GetUserByID(Context.User.Id);
            if(betAmount <= 0)
            {
                await ReplyAsync("Bet must be greater than 0");
                return;
            }
            if(user is null)
            {
                await ReplyAsync("Unable to find User");
                return;
            }
            if(user.RonPoints < betAmount)
            {
                await ReplyAsync("Not Enough Points");
                return;
            }

            await GameService.AddRonPoints(Context.User,-1*betAmount);
            int winnings;
            Slot slot;
            (winnings,slot) = SlotService.Play(betAmount);
            await GameService.AddRonPoints(Context.User, winnings);
            await ReplyAsync($"```{slot.ToString()}```\n{Context.User.Username} received {winnings} RonPoints");
        }

        [Command("help")]
        [Alias("?")]
        public async Task helpAsync()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Slots Help")
                .WithDescription("**Syntax:**\n```!slots bet {amount}```")
                .WithUrl("")
                .WithColor(Color.Gold)
                .AddField("Payouts","■ ■     1x\n♢♢     2x\n♧♧     3x\n♡♡     4x\nඞඞ     20x",true)
                .AddField("Payouts","■ ■ ■   2x\n♢♢♢    5x\n♧♧♧    8x\n♡♡♡   10x\nඞඞඞ   750x",true)
                .AddField("Examples", "!slots bet 69");
            var embed = builder.Build();

            await ReplyAsync("",false,embed);
        }

    }
}