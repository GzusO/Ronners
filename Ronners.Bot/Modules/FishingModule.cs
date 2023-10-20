// Interation modules must be public and inherit from an IInterationModuleBase
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ronners.Bot.Models;
using Ronners.Bot.Services;

namespace Ronners.Bot.Modules
{
    [Group("fish","Fishing Mingame")]
    public class FishingModule : InteractionModuleBase<SocketInteractionContext>
    {
        public GameService GameService{get;set;}
        public Random Rand{get;set;}
        public FishingService FishingService{get;set;}

        
        [SlashCommand("leaderboard","Top fishers by fish count")]
        public async Task Leaderboard([MinValue(1)]int count=10)
        {
            var allowedMentions = new AllowedMentions(null)
            {
                MentionRepliedUser = true
            };

            var users = await GameService.GetFishCount(count);
            var response = "";
            int i = 0;
            foreach(var user in users)
            {
                i++;
                var str = $"{i}. {Discord.MentionUtils.MentionUser(user.userID)} : {user.count}";
                if(response.Length + str.Length > 2000)
                {
                    await RespondAsync(response,null,false,false,allowedMentions);
                    response = "";
                }
                response += str+"\n";
           
            }
            await RespondAsync(response,null,false,false,allowedMentions);
        }
    }
}