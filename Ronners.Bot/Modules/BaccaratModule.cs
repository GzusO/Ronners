using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;
using System;


namespace Ronners.Bot.Modules
{
    [Group("baccarat")]
    public class BaccaratModule : ModuleBase<SocketCommandContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public BaccaratService BaccaratService{get;set;}

        [Command("bet")]
        public async Task BetAsync(int betAmount, string bet)
        {
            var user = await GameService.GetUserByID(Context.User.Id);
            var betType = BaccaratService.GetBetType(bet);

            if(betAmount <= 0)
            {
                await ReplyAsync("Bet must be greater than 0");
                return;
            }
            if(betType == BaccaratBetType.Unknown)
            {
                await ReplyAsync("Unknown Bet Type");
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
            string result;
            (winnings,result) = BaccaratService.Play(betType,betAmount);
            await GameService.AddRonPoints(Context.User, winnings);
            if(winnings> 0 )
            {
                await ReplyAsync($"{result}{user.Username} won {winnings} RonPoints!");
            }
            else
            {
                await ReplyAsync($"{result}{user.Username} lost {betAmount} RonPoints!");
            }
        }

        [Command("help")]
        [Alias("?")]
        public async Task helpAsync()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Baccarat Help")
                .WithDescription("**Syntax:**\n```!baccarat bet {amount} {Bet Type}```")
                .WithUrl("")
                .WithColor(new Color(0x9286B6))
                .AddField("Bet Types()", "player\nbanker\ntie")
                .AddField("Examples", "!baccarat bet 10 player\n!baccarat bet 10 banker\n!baccarat bet 10 tie");
            var embed = builder.Build();

            await ReplyAsync("",false,embed);
        }

    }
}