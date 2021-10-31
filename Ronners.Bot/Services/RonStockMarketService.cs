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
                var rnd = _rand.NextDouble();
                var changePercent = 2* stock.Volatility * rnd;
                if(changePercent> stock.Volatility)
                    changePercent -= (2*stock.Volatility);
                var oldPrice = stock.Price;
                int changeAmount = (int)Math.Round(oldPrice * changePercent);
                stock.Price = oldPrice + changeAmount;
                stock.Change = changeAmount;
            }
            await WriteStocksToFile();
        }
        internal IEnumerable<RonStock> GetAllStocks()
        {
            return Stocks;
        }

        internal RonStock GetStock(string ticker)
        {
            return Stocks.Find(stock => stock.Symbol == ticker);
        }

        internal void AddStock(string symbol, string company, int price, double volatility)
        {
            Stocks.Add(new RonStock(){Symbol=symbol,CompanyName=company,Price = price,Volatility=volatility});
        }

        public static List<RonStock> GenerateNewRonStock()
        {
            var stocks = new List<RonStock>();
            stocks.Add(new RonStock(){Symbol="TEST",CompanyName="Test Company",Price=25,Volatility=.1});
            stocks.Add(new RonStock(){Symbol="DEEZ",CompanyName="Deez Nuts",Price=50,Volatility=.2});
            stocks.Add(new RonStock(){Symbol="CUM",CompanyName="Cum Rocket - The Best Crypto",Price=6969,Volatility=.5});

            return stocks;
        }

        public async Task WriteStocksToFile()
        {
            string json = JsonSerializer.Serialize(Stocks, new JsonSerializerOptions(){WriteIndented=true});
            await File.WriteAllTextAsync(StockFile,json,new UTF8Encoding(false));
        }
    }
}