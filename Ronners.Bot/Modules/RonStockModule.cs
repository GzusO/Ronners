using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Models;
using Ronners.Bot.Services;


namespace Ronners.Bot.Modules
{
    [Group("ronstock")]
    public class RonStockModule : ModuleBase<SocketCommandContext>
    {

        public GameService GameService{ get; set; }
        public CommandService _commandService {get;set;}

        public RonStockMarketService RonStockMarketService{ get; set; }

        [Command]
        [Alias("help")]
        [Summary("Lists all commands in RonStock {optional parameter}")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            var module = _commandService.Modules.First(mod => mod.Name=="ronstock");
            var commands = module.Commands;
            EmbedBuilder embedBuilder = new EmbedBuilder();
            

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField($"{module.Group} {command.Name}", embedFieldText);
            }

            var commandCount = module.Commands.Count();
            int pageCount = (commandCount + 24)/ 25;

            await ReplyAsync($"Commands Page [{page}/{pageCount}]: ", false, embedBuilder.Build());
        }

        [Command("portfolio")]
        [Summary("Lists all stocks owned by a User. ronstock portfolio {User}.")]
        public async Task Portfolio(IUser user = null)
        {
            user = user ?? Context.User;

            var stocks = await GameService.GetUserRonStockByUserAsync(user);

            await ReplyAsync("",false,BuildEmbed(stocks));
        }

        [Command("buy")]
        [Summary("Buy stocks. ronstock buy SYMBOL QUANTITY")]
        public async Task BuyAsync(string ticker, int quantity)
        {
            if(quantity < 1)
            {
                await ReplyAsync("Invalid quantity. Must be greater than 0");
                return;
            }
            var upperTicker = ticker.ToUpperInvariant();

            RonStock stock = RonStockMarketService.GetStock(upperTicker);

            if(stock == null)
            {
                await ReplyAsync($"Stock {upperTicker} doesn't exist.");
                return;
            }

            int cost = stock.Price * quantity *-1;


            UserRonStock usr = await GameService.GetUserRonStockAsync(Context.User,upperTicker);


            if(!await GameService.AddRonPoints(Context.User,cost))
            {
                await ReplyAsync($"Can't Afford to buy {quantity} shares of {upperTicker} for {cost} rp.");
                return;
            }

            if(usr == null)
            {
                usr = new UserRonStock(){UserID=Context.User.Id, Symbol = upperTicker, Quantity = quantity};
                await GameService.AddUserRonStock(usr);
            }
            else
            {
                usr.Quantity+= quantity;
                await GameService.UpdateUserRonStock(usr);
            }

            await ReplyAsync($"Bought {quantity} shares of {upperTicker}.");
        }

        [Command("sell")]
        [Summary("Sell stocks. ronstock sell SYMBOL QUANTITY")]
        public async Task SellAsync(string ticker, int quantity)
        {
            var upperTicker = ticker.ToUpperInvariant();
            
            if(quantity < 1)
            {
                await ReplyAsync($"Invalid quantity. Must be greater than 0");
                return;
            }

            RonStock stock  = RonStockMarketService.GetStock(upperTicker);
            if(stock == null)
            {
                await ReplyAsync($"Stock {upperTicker} doesn't exist.");
                return;
            }
            UserRonStock usr = await GameService.GetUserRonStockAsync(Context.User,upperTicker);

            if(usr == null || usr.Quantity < quantity)
            {
                await ReplyAsync($"You don't own enough {upperTicker} to sell {quantity} shares.");
                return;
            }

            int cost = quantity * stock.Price;
            
            usr.Quantity-=quantity;
            if(usr.Quantity == 0)
                await GameService.DeleteUserRonStock(usr);
            else
                await GameService.UpdateUserRonStock(usr);

            await GameService.AddRonPoints(Context.User, cost);
            await ReplyAsync($"Sold {quantity} shares of {upperTicker} for {cost}.");
        }

        [Command("list")]
        [Summary("List all stocks")]
        public async Task ListAsync()
        {
            IEnumerable<RonStock> stocks = RonStockMarketService.GetAllStocks();

            await ReplyAsync("",false,BuildEmbed(stocks));
        }

        [Command("info")]
        [Summary("List info about a stock. ronstock info SYMBOL")]
        public async Task InfoAsync([Remainder] string ticker=null)
        {
            var upperTicker = ticker.ToUpperInvariant();
            RonStock stock = RonStockMarketService.GetStock(upperTicker);

            await ReplyAsync("",false,BuildEmbed(stock));
        }

        [RequireOwner]
        [Command("add")]
        [Summary("Owner Command")]
        public async Task AddAsync(string symbol, int price, double volatility, string companyName)
        {
            RonStockMarketService.AddStock(symbol,companyName,price,volatility);
        }

        private Embed BuildEmbed(RonStock stock)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(stock.CompanyName);
            builder.AddField("Ticker",stock.Symbol);
            builder.AddField("Price",string.Format("{0}rp",stock.Price),true);
            builder.AddField("Change",string.Format("{0}",stock.Change),true);
            builder.WithFooter("Stock Data provided by Ronners!");
            if(stock.Change > 0)
                builder.WithColor(Color.Green);
            else if(stock.Change < 0)
                builder.WithColor(Color.Red);
            else
                builder.WithColor(Color.LightGrey);
            return builder.Build();
        }
        private Embed BuildEmbed(IEnumerable<RonStock> stocks)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Stocks");
            foreach(RonStock stock in stocks)
            {
                builder.AddField($"{stock.Symbol} - {stock.CompanyName}",$"{stock.Price}rp");
            }
            builder.WithColor(Color.LightGrey);
            return builder.Build();
        }

        private Embed BuildEmbed(IEnumerable<UserRonStock> stocks)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Stocks");
            foreach(UserRonStock stock in stocks)
            {
                builder.AddField($"-",$"{stock.Symbol}  -  {stock.Quantity} shares.");
            }
            builder.WithColor(Color.LightGrey);
            return builder.Build();
        }
    }
}