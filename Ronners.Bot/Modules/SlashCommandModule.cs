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


        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("ping", "Recieve a pong")]
        public async Task Ping ( )
        {
            await RespondAsync("pong");
        }
        [SlashCommand("compact", "Compact Embeds")]
        [RequireRole("Admin")]
        public async Task Compact(IMessageChannel channel)
        {
            var messages = await channel.GetMessagesAsync(2).FlattenAsync();
            await RespondAsync(":thumbsup:",ephemeral:true);
            foreach(var message in messages)
            {
                if (message.Source != MessageSource.User)
                    continue;

                if(message.Embeds.Count == 0)
                    continue;
                foreach(var embed in message.Embeds)
                {
                    if(embed.Type == EmbedType.Video)
                    {
                        var newMessage = await channel.SendMessageAsync(string.Format("[{0}: {1}]",embed.Title,embed.Url));
                        await newMessage.ModifyAsync(x=> x.Flags = MessageFlags.SuppressEmbeds);
                    }
                   await LoggingService.LogAsync("RONR",LogSeverity.Debug,$"Embed Type: {embed.Type} Embed Title: {embed.Title} Embed Url: {embed.Url}");
                }
            }
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

        [SlashCommand("gift", "Give a gift")]
        public async Task Gift (SocketGuildUser user, string message)
        {
            try
            {
                var channel = await user.CreateDMChannelAsync();
                await channel.SendMessageAsync(message);

                await RespondAsync("Sent the gift!",null,false,true);
            }
            catch (Discord.Net.HttpException e)
            {
                await RespondAsync("Failed to send gift.",null,false,true);
            }
                
        }

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