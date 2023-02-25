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

namespace Ronners.Bot.Services
{
    public class JellyFinService
    {

        private readonly HttpClient _http;

        private readonly Random _rand;


        public JellyFinService(Random rand,HttpClient http)
        {
            _rand = rand;
            _http = http;
        }

        public async Task<Models.JellyFin.Item> Random()
        {
            var stringRequest = $"http://192.168.10.155:8096/Items?api_key={ConfigService.Config.JellyfinKey}&limit=1&mediaTypes=Audio&userId={ConfigService.Config.JellyFinUserID}&Recursive=true&SortBy=Random";
            var resp = await  _http.GetAsync(stringRequest);
            if(!resp.IsSuccessStatusCode)
            {
                return null;
            }
            var contentStream = await resp.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<ItemsResult>(contentStream);

            return data.Items[0];
        }

        public async Task<Item> GetSongByID(string id)
        {
            var stringRequest = $"http://192.168.10.155:8096/Items?api_key={ConfigService.Config.JellyfinKey}&limit=1&mediaTypes=Audio&userId={ConfigService.Config.JellyFinUserID}&ids={id}";
            var resp = await  _http.GetAsync(stringRequest);
            if(!resp.IsSuccessStatusCode)
            {
                return null;
            }
            var contentStream = await resp.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<ItemsResult>(contentStream);

            return data.Items[0];
        }
    }
}