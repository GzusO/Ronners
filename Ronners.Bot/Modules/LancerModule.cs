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
    [Group("lancer","lancer")]
    public class LancerModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Interactions { get; set; }

        public LancerService _lancerService{get;set;}

        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("frames","List frames")]
        public async Task framesAsync(string frameName="")
        {
            var frames = _lancerService.Frames;
            
            var frame = frames.FirstOrDefault(x=> x.Name.Contains(frameName,StringComparison.InvariantCultureIgnoreCase));

            if(frame != null)
            {
                await RespondAsync("",embed: CustomEmbeds.BuildEmbed(frame));
                return;
            }

            var frameNames = string.Join("\n",frames.Select(x=> x.Name));

            await RespondAsync(frameNames);
        }
    }
}