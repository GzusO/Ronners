using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Ronners.Bot.Modules;
using System;
using Ronners.Bot.Models;
using System.Net.Http.Headers;
using System.Web;
using Ronners.Bot.Models.GoogleBooks;

namespace Ronners.Bot.Services
{
    public class BookService
    {
        private readonly HttpClient _http;

        public BookService(HttpClient http)
            => _http = http;

        public async Task<Volume> GetBookDetails(string query)
        {
            var stringRequest = string.Format($"https://www.googleapis.com/books/v1/volumes?q={HttpUtility.UrlEncode(query)}&maxResults=1&key={ConfigService.Config.GoogleBooksKey}");

            var resp = await  _http.GetAsync(stringRequest);
            if(!resp.IsSuccessStatusCode)
            {
                return null;
            }
            var contentStream = await resp.Content.ReadAsStreamAsync();

            var data = await JsonSerializer.DeserializeAsync<VolumeList>(contentStream);
            if(data.Count == 0)
                return null;
            return data.Items[0];
        }
    }
}