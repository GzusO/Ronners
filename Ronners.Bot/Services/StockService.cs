using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Ronners.Bot.Modules;
using System;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class StockService
    {
        private readonly HttpClient _http;

        public StockService(HttpClient http)
            => _http = http;

        public async Task<StockQuoteIEX> GetStockPrice(string ticker)
        {
            var stringRequest = string.Format(@"https://cloud.iexapis.com/stable/stock/{0}/quote?displayPercent=true&token={1}",ticker,ConfigService.Config.IEXKey);
            var resp = await  _http.GetAsync(stringRequest);
            if(!resp.IsSuccessStatusCode)
            {
                return null;
            }
            var contentStream = await resp.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<StockQuoteIEX>(contentStream);

            return data;
        }
    }
}