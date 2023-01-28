using System.Threading.Tasks;
using Discord;
using Ronners.Bot.Services;
using System;
using Discord.Interactions;

namespace Ronners.Bot.Modules
{
    [Group("baccarat","Baccarat Game")]
    public class BaccaratModule : InteractionModuleBase<SocketInteractionContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public BaccaratService BaccaratService{get;set;}

        [SlashCommand("bet","Bet at Baccarat")]
        public async Task BetAsync([MinValue(1)]int betAmount, BaccaratBetType betType)
        {
            var user = await GameService.GetUserByID(Context.User.Id);

            if(betAmount <= 0)
            {
                await RespondAsync("Bet must be greater than 0");
                return;
            }
            if(user is null)
            {
                await RespondAsync("Unable to find User");
                return;
            }
            if(!await GameService.AddRonPoints(Context.User,-1*betAmount))
            {
                await RespondAsync("Not Enough Points");
                return;
            }
            
            int winnings;
            string result;
            (winnings,result) = BaccaratService.Play(betType,betAmount);
            await GameService.AddRonPoints(Context.User, winnings);
            if(winnings> 0 )
            {
                await RespondAsync($"{result}{user.Username} won {winnings} RonPoints!");
            }
            else
            {
                await RespondAsync($"{result}{user.Username} lost {betAmount} RonPoints!");
            }
        }

/**
        [SlashCommand("help","Description of Baccarat.")]
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

            await RespondAsync("",false,embed);
        }
**/
    }
}