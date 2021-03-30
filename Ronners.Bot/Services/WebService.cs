using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Json;
using System;

namespace Ronners.Bot.Services
{
    public class WebService
    {
        private readonly HttpClient _http;
        public WebService(HttpClient http)
            => _http = http;

        public async Task<Stream> GetFileAsStream(string url)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
               return  await (await client.SendAsync(request)).Content.ReadAsStreamAsync();
        }
    }
}