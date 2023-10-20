using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using Ronners.Bot.Extensions;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class CaptchaService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GameService _game;
        private readonly AchievementService _achievements;
        private Dictionary<ulong,GameState> Captchas;
       
        public CaptchaService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;
            _achievements = services.GetRequiredService<AchievementService>();
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            Captchas = new Dictionary<ulong, GameState>();
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (message.ReferencedMessage is null) return;

            if(message.Author.Id == 146979125675032576)//Block squirtle
                return;

            var achievementResult = AchievementResult.FromSuccess();
            achievementResult.AchievementType = AchievementType.Captcha;
            achievementResult.User = rawMessage.Author;
            GameState captcha = null;
            if(!Captchas.TryGetValue(message.ReferencedMessage.Id,out captcha))
                return;
            switch(captcha)
            {
                case CaptchaState captchaState:
                    if(captchaState.Validate(message.Content))
                    {
                        await _game.AddRonPoints(message.Author,10);
                        await message.ReferencedMessage.ModifyAsync(c => c.Content = "Solved");
                        achievementResult.BoolValue = true;
                    }
                    else
                    {
                        achievementResult.BoolValue = false;
                        await message.ReferencedMessage.ModifyAsync(c => c.Content = "Failed");
                    }
                        
                    Captchas.Remove(message.ReferencedMessage.Id);
                    break;
            }

            
            _achievements.ProcessMessage(achievementResult);
        }

        public void AddCaptcha(CaptchaState cs)
        {
            Captchas.Add(cs.gameMessage.Id, cs);
        }
    }
}