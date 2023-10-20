using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.Audio;
using System.Collections.Concurrent;
using System;
using System.IO;
using Ronners.Bot.Extensions;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Ronners.Bot.Models.JellyFin;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Ronners.Bot.Models;
using System.ComponentModel;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using SixLabors.ImageSharp.ColorSpaces;

namespace Ronners.Bot.Services
{
    public class AdminService
    {
        public DiscordSocketClient _discord  {get;set;}
        public readonly Random _rand;
        public readonly GameService _gameService;
        public Dictionary<ulong,ChannelModeration> ChannelModerationLevel {get;set;}
        public AdminService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _rand = services.GetRequiredService<Random>();
            _gameService = services.GetRequiredService<GameService>();
            
        }

        public async Task InitializeAsync()
        {
             _discord.MessageReceived += MessageReceivedAsync;
             _discord.MessageUpdated += MessageEdittedAsync;

             var moderatedChannels = await _gameService.GetModeratedChannels();
             ChannelModerationLevel = moderatedChannels.ToDictionary(x => x.ChannelID);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            ChannelModeration moderationData;
            if(ChannelModerationLevel.TryGetValue(message.Channel.Id, out moderationData))
            {
                switch (moderationData.ModerationLevel)
                {
                    case 1:
                        await ModerationOne(message);
                        break;
                    case 2:
                        await ModerationTwo(message);
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task ModerationOne(SocketUserMessage message)
        {
            if(sha1(message.Content+message.Author.Id))
                return;
            await _gameService.LogCensorship(message.Author);
            await message.DeleteAsync();
            await LoggingService.LogAsync("politics",LogSeverity.Info,$"Censorsed {message.Author.Username}",null);
        }

        public async Task ModerationTwo(SocketUserMessage message)
        {
            return;
        }

        public async Task MessageEdittedAsync(Cacheable<IMessage, ulong> oldMessage,SocketMessage rawMessage, ISocketMessageChannel channel)
        {
            await MessageReceivedAsync(rawMessage);
        }

        public static bool sha1(string message)
        {

            var crypt = SHA1.HashData(Encoding.Unicode.GetBytes(message));
            return (crypt[0]&1) ==0;
            
        }
    }
}