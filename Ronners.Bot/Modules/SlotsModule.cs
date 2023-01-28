using System.Threading.Tasks;
using Discord;
using Ronners.Bot.Services;
using System;
using Discord.Interactions;

namespace Ronners.Bot.Modules
{
    [Group("slots","Ronners Slot Machine")]
    public class SlotsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public SlotService SlotService{get;set;}

        [SlashCommand("bet","Bet on the Ronners slot machine")]
        public async Task BetAsync([MinValue(1)]int betAmount)
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
            Slot slot;
            (winnings,slot) = SlotService.Play(betAmount);
            await GameService.AddRonPoints(Context.User, winnings);
            await RespondAsync($"```{slot.ToString()}```\n{Context.User.Username} received {winnings} RonPoints");
        }

        [SlashCommand("help","Description of the Ronners slot machine")]
        public async Task helpAsync()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Slots Help")
                .WithDescription("**Syntax:**\n```/slots bet {amount}```")
                .WithUrl("")
                .WithColor(Color.Gold)
                .AddField("Payouts","■ ■     1x\n♢♢     2x\n♧♧     3x\n♡♡     4x\nඞඞ     20x",true)
                .AddField("Payouts","■ ■ ■   2x\n♢♢♢    5x\n♧♧♧    8x\n♡♡♡   10x\nඞඞඞ   769",true)
                .AddField("Examples", "/slots bet 1");
            var embed = builder.Build();
            Embed []  embeds = new Embed[]{embed};
            await RespondAsync("",embed:embed);
        }

    }
}