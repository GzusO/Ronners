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

namespace Ronners.Bot.Services
{
    public class KeyService
    {

        private readonly Random _rand;
        private readonly GameService _gameService;


        public KeyService(Random rand, GameService game)
        {
            _rand = rand;
            _gameService = game;
        }

        public async Task<string> AddKey(string originalMessage)
        {
            if(_rand.Next(0,10)!=0)
                return originalMessage;

            var key = await _gameService.GetRonKey();
            if(key == null)
                return originalMessage;

            await _gameService.UseRonKey(key.KeyId);

            var builder = new StringBuilder(originalMessage);

            var spaces = originalMessage.AllIndexesOf(" ");
            var randomSpaceIndex = _rand.Next(spaces.Count());

            builder.Insert(spaces.ElementAt(randomSpaceIndex),$" Free {key.Source} Key: ' {key.Key} '");

            return builder.ToString();
        }


        

    }
}