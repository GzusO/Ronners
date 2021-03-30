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
    public class ReactionService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GameService _game;
        private Dictionary<ulong,GameState> Games;
       
        public ReactionService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;

            _discord.ReactionAdded += ReactionAddedAsync;
        }

        public async Task InitializeAsync()
        {
            Games = new Dictionary<ulong, GameState>();
        }

        public async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            GameState game;
            var message = await arg1.GetOrDownloadAsync();
            if(!Games.TryGetValue(message.Id, out game))
                return;

            switch(game)
            {
                case TicTacToeGameState gameState:
                    if(gameState.Update(arg3.Emote,arg3.UserId))
                        await message.ModifyAsync(m => { m.Content = gameState.ToString();});
                    if(gameState.GameComplete >=0)
                        Games.Remove(message.Id);
                    break;
                default:
                    return;
            }
        }

        public void AddTicTacToe(TicTacToeGameState gs)
        {
            Games.Add(gs.gameMessage.Id, gs);
        }
    }
}