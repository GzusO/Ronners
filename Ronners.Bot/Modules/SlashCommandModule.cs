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
    
    public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Interactions { get; set; }
        public WebService _webService{get;set;}
        public ImageService _imageService{get;set;}
        public GameService GameService{get;set;}
        public BattleService _battleService{get;set;}


        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("ping", "Recieve a pong")]
        public async Task Ping ( )
        {
            await RespondAsync("pong");
        }

        [SlashCommand("button","Button demo")]
        public async Task Button ()
        {
            var builder = new ComponentBuilder();
            builder.WithButton("Primary","primary-button",ButtonStyle.Primary);
            builder.WithButton("Secondary","secondary-button",ButtonStyle.Secondary);
            builder.WithButton("Success","success-button",ButtonStyle.Success);
            builder.WithButton("Danger","danger-button",ButtonStyle.Danger);
            builder.WithButton("Link",null,ButtonStyle.Link,url:@"https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            
            await RespondAsync("Here is a button.",components:builder.Build());
        }

        [SlashCommand("battle","Battle Demo")]
        public async Task BattleAsync([Choice("Random",""),Choice("Stick","Stick"), Choice("Rusty Nail","Rusty Nail"), Choice("None","Fists")] string weapon ="",int ronners = 9,int objectivity = 1, int normalcy = 1, int nutrition = 1, int erudition = 1, int rapidity = 0, int strength = 1)
        {
            var results = _battleService.Demo(Context.User.Username, ronners,objectivity,normalcy,nutrition,erudition,rapidity,strength, weapon);

            var playerWeapon = results.Player.HandSlot is null ? "" : results.Player.HandSlot.Name;
            await RespondAsync(embed: CustomEmbeds.BuildEmbed(results.Logs,$"{results.Player.Name} with {playerWeapon} vs. {results.Enemy.Name}"));
        }

        // [SlashCommand("gift", "Give a gift")]
        // public async Task Gift (SocketGuildUser user, string message)
        // {
        //     try
        //     {
        //         var channel = await user.CreateDMChannelAsync();
        //         await channel.SendMessageAsync(message);

        //         await RespondAsync("Sent the gift!",null,false,true);
        //     }
        //     catch (Discord.Net.HttpException e)
        //     {
        //         await RespondAsync("Failed to send gift.",null,false,true);
        //     }
                
        // }

        [MessageCommand("ronify")]
        public async Task Ronify(IMessage message)
        {
            if (message.Attachments.Count > 0)
            {
                await RespondAsync(":thumbsup:",null,false,true);
                var reference = new MessageReference(message.Id);
                foreach(var attachment in message.Attachments)
                {
                    var stream = await _webService.GetFileAsStream(attachment.Url);
                    var image = _imageService.EdgeDetectAndSave(stream,attachment.Filename);
                    var result = await message.Channel.SendFileAsync(image,Context.User.Mention+" Ronners!",false,null,null,false,null,reference);
                }
            }
            else
            {
                await RespondAsync(":thumbsdown:",null,false,true);
            }
        }

        [MessageCommand("remind")]
        public async Task Remind(IMessage message)
        {
            await Context.User.SendMessageAsync(message.GetJumpUrl());
            await RespondAsync(":thumbsup:", ephemeral: true);
        }

        [ComponentInteraction("inventory_details")]
        public async Task RoleSelection(string id, string[] selectedCollection)
        {
            var collectionKey = selectedCollection[0];
            var items = await GameService.GetUsersItems(Context.User);
            var collections = await GameService.GetCollections();
            var collection = collections.First(x=> x.Name==collectionKey);

            await RespondAsync($"{Context.User.Username}'s {collectionKey} Collection",embed:BuildEmbed(items.Where(x=> x.Item.Collection==collection.Name),collection));
        }   

        private Embed BuildEmbed(IEnumerable<UserItemDetailed> items,Collection collection)
        {
            EmbedBuilder builder = new EmbedBuilder();
            var completion = (double)items.Count()/collection.NumberOfItems;
            var color = new Color(Convert.ToByte((1-completion)*255), Convert.ToByte(completion*255),Convert.ToByte(0));
            builder.WithTitle($"{collection.Name} ({items.Count()}/{collection.NumberOfItems})");
            builder.WithColor(color);
            foreach(var item in items.OrderBy(x=> x.Item.Rarity))
            {
                builder.AddField($"{item.Item.Name} ({item.Item.RarityName()}) -- x{item.Quantity}",$"- {item.Item.Description}");
            }
            builder.WithCurrentTimestamp();
            return builder.Build();
        }
    }
}