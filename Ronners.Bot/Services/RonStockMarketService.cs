using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class RonStockMarketService
    {
        public List<RonStock> Stocks {get;set;}
        public string StockFile {get;set;}
        public Random _rand {get;set;}

        public RonStockMarketService(IServiceProvider services)
        {
            _rand = services.GetRequiredService<Random>();
        }
        public async Task InitializeAsync(string stockFile)
        {
            StockFile = stockFile;
            var json = string.Empty;

            if(!File.Exists(stockFile))
            {
                json = JsonSerializer.Serialize(GenerateNewRonStock(),new JsonSerializerOptions(){WriteIndented =true});
                File.WriteAllText(stockFile,json,new UTF8Encoding(false));
            }

            json = File.ReadAllText(stockFile,new UTF8Encoding(false));
            Stocks = JsonSerializer.Deserialize<List<RonStock>>(json);
        }
        public async void RefreshMarket(object state)
        {
            foreach(var stock in Stocks)
            {
                var randChange = (2*_rand.NextDouble()-1)*stock.Volatility*stock.Average;
                int newPrice = (int)Math.Round(stock.Min+.5*(stock.Max-stock.Min)*(1+Math.Sin((stock.Increment*stock.Spread)+stock.Shift))+randChange);
                if( newPrice < 1)
                    newPrice = 1;
                stock.Change = newPrice - stock.Price;
                stock.Price = newPrice;
                stock.Increment++;
            }
            await WriteStocksToFile();

            await LoggingService.LogAsync("RonStock",Discord.LogSeverity.Info, "RonStock Market refreshed.");
        }
        internal IEnumerable<RonStock> GetAllStocks()
        {
            return Stocks;
        }

        internal RonStock GetStock(string ticker)
        {
            return Stocks.Find(stock => stock.Symbol == ticker);
        }

        internal void AddStock(string symbol, string company, int min, int max, double spread, double volatility, double shift=0, long increment=0)
        {
            Stocks.Add(new RonStock(symbol,company,min,max,spread,volatility,shift,increment));
        }

        public static List<RonStock> GenerateNewRonStock()
        {
            var stocks = new List<RonStock>();
            stocks.Add(new RonStock("TEST","Test Company",1,1,0,0));
            stocks.Add(new RonStock("DEEZ","Deez Nuts",69,420,.0069,.024,2));
            stocks.Add(new RonStock("CUM","Cum Rocket - The Last Crypto",420,6969,.0015,.09,11));
            return stocks;
        }

        public async Task WriteStocksToFile()
        {
            string json = JsonSerializer.Serialize(Stocks, new JsonSerializerOptions(){WriteIndented=true});
            await File.WriteAllTextAsync(StockFile,json,new UTF8Encoding(false));
        }
    }
}