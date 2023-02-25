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
    [Group("gacha","collectibles")]
    public class GachaModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Interactions { get; set; }
        public WebService _webService{get;set;}
        public ImageService _imageService{get;set;}
        public GameService GameService{get;set;}
        public AchievementService AchievementService{get;set;}
        public Random Rand{get;set;}
        public DiscordSocketClient _discord{get;set;}

        public EconomyService _economyService{get;set;}


        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("list","List available collections")]
        public async Task listAsync()
        {
            var collections = await GameService.GetCollections();
            var response = $"";
            foreach(var collection in collections)
            {

                if(response.Length + collection.ToString().Length > 1990)
                {
                    await RespondAsync(response);
                    response = "";
                }
                response += $"- {collection.ToString()}\n";
           
            }
            await RespondAsync(response); 
        }

        [SlashCommand("buy","Buy random items from a collection")]
        public async Task buyAsync(string collection, [MinValue(1)] int quantity)
        {
            bool compactResult =false;

            if(quantity < 1)
                return;

            if (quantity > 10)
                compactResult = true;

            var collections = await GameService.GetCollections();
            if(collections.Where(x=> x.Name.ToLower() == collection.ToLower()).Count()==0)
            {
                await RespondAsync($"Collection: {collection} doesn't exist.");
                return;
            }
            if(!await GameService.AddRonPoints(Context.User,-50*quantity))
            {
                await RespondAsync($"Not Enough Points! Costs {50*quantity} RonPoints.");
                return;
            }
            var items = await GameService.PurchaseCollection(collection,quantity,Context.User);
            items = items.OrderBy(x=> x.Rarity).ThenBy(x=> x.Name);   

            if(!compactResult)
            {
                var embedItems = new List<Embed>();
                foreach(var item in items)
                {

                    embedItems.Add(CustomEmbeds.BuildEmbed(item));
            
                }
                await RespondAsync("You have received the following items.",embeds: embedItems.ToArray());
            }
            else
            {
                await RespondAsync("You have received the following items.",embed: CustomEmbeds.BuildEmbed(items));
            }
            
        }

        [SlashCommand("inventory","View your gacha collection")]
        public async Task inventoryAsync(IUser user = null)
        {
            user = user ?? Context.User;
            var items = await GameService.GetUsersItems(user);
            var collections = await GameService.GetCollections();

            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select a Collection")
                .WithCustomId("inventory_details")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach(var col in collections)
                menuBuilder.AddOption(col.Name,col.Name);

            var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

            await RespondAsync($"{user.Username}'s Gacha Collection", embed:CustomEmbeds.BuildEmbed(items,collections),components:builder.Build());
        }

    }
}