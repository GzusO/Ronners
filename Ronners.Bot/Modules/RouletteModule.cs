using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;
using System;

namespace Ronners.Bot.Modules
{
    [Group("roulette")]
    public class RouletteModule : ModuleBase<SocketCommandContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public RouletteService RouletteService{get;set;}

        [Command("bet")]
        public async Task BetAsync(int betAmount, string typeString, params string [] bets)
        {
            var user = await GameService.GetUserByID(Context.User.Id);
            var bet = RouletteService.GetBet(typeString,bets);

            if(betAmount <= 0)
            {
                await ReplyAsync("Bet must be greater than 0");
                return;
            }
            if(bet.Type == BetType.Unknown)
            {
                await ReplyAsync("Unknown Bet Type");
                return;
            }
            if(!RouletteService.IsValid(bet))
            {
                await ReplyAsync("Invalid Bet");
                return;
            }
            if(user is null)
            {
                await ReplyAsync("Unable to find User");
                return;
            }
            if(!await GameService.AddRonPoints(Context.User,-1*betAmount))
            {
                await ReplyAsync("Not Enough Points");
                return;
            }

            
            int winnings;
            string result;
            (winnings,result) = RouletteService.GetResult(bet,betAmount);
            await GameService.AddRonPoints(Context.User, winnings);
            await ReplyAsync($"Roulette Rolled a {result}, resulting in {user.Username} winning {winnings} RonPoints");
        }

        [Command("help")]
        [Alias("?")]
        public async Task helpAsync()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Roulette Help")
                .WithDescription("**Syntax:**\n```!roulette bet {amount} {Bet Type} {Numbers}```")
                .WithUrl("")
                .WithColor(new Color(0x9286B6))
                .WithImageUrl("https://gamesofroulette.com/img/pictures/roulette-rules/american-roulette-table.gif")
                .AddField("Bet Types(* require numbers)", "Straight*\nSplit*\nCorner*\nHigh/Low\nEven/Odd\nRed/Black\nDozen*\nColumn*\nBasket\nStreet*\nTrio*\nSnake")
                .AddField("Examples", "!roulette bet 10 Straight 1\n!roulette bet 10 Corner 4 5 7 8\n!roulette bet 10 Even\n!roulette bet 10 street 10 11 12\n!roulette bet 10 split 25 28");
            var embed = builder.Build();

            await ReplyAsync("",false,embed);
        }

    }
}