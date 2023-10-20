using System.Threading.Tasks;
using Discord;
using Ronners.Bot.Services;
using System;
using Discord.Interactions;

namespace Ronners.Bot.Modules
{
    [Group("blackjack","Blackjack Game")]
    public class BlackjackModule : InteractionModuleBase<SocketInteractionContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public BlackjackService BlackjackService{get;set;}

        [SlashCommand("bet","Bet at Blackjack")]
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
            // Charge points
            // if(!await GameService.AddRonPoints(Context.User,-1*betAmount))
            // {
            //     await RespondAsync("Not Enough Points");
            //     return;
            // }

            var bjs = BlackjackService.StartGame(Context.User.Id);
            if(bjs == null)
            {
                await RespondAsync("Already have game in progress");
                return;
            }
                

            var builder = new ComponentBuilder();
            builder.WithButton("Hit",$"bj:hit,{bjs.GameID}",ButtonStyle.Primary,disabled:!bjs.CanHit);
            builder.WithButton("Stay",$"bj:stay,{bjs.GameID}",ButtonStyle.Secondary,disabled:!bjs.CanStay);
            await RespondAsync("Game Loading", new Embed[] {CustomEmbeds.BuildEmbed(bjs)}, components:builder.Build());
        
        }

        [ComponentInteraction("bj:*,*",true)]
        public async Task Hit (string command,string id)
        {
            ulong userID;
            if(!ulong.TryParse(id.Substring(36),out userID))
                return;
            if(userID != Context.User.Id)
            {
                await RespondAsync("Not your game.",ephemeral:true);
                return;
            }

            BlackjackState bjs;

            if(command == "hit")
                bjs = BlackjackService.HitGame(id);
            else
                bjs = BlackjackService.StayGame(id);
            if(bjs == null)
                return;
            
            var builder = new ComponentBuilder();
            builder.WithButton("Hit",$"bj:hit,{bjs.GameID}",ButtonStyle.Primary,disabled:!bjs.CanHit);
            builder.WithButton("Stay",$"bj:stay,{bjs.GameID}",ButtonStyle.Secondary,disabled:!bjs.CanStay);


            await Context.Interaction.ModifyOriginalResponseAsync(x=>
            {
                x.Embeds = new Embed[]{CustomEmbeds.BuildEmbed(bjs)};
                //x.Components = builder.Build();
            });
        }
    }
}