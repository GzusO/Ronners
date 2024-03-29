using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Models;
using Ronners.Bot.Services;


namespace Ronners.Bot.Modules
{
    [Group("gacha")]
    public class ShopModule : ModuleBase<SocketCommandContext>
    {
        public GameService GameService{ get; set; }
        public CommandService _commandService {get;set;}


        [Command("help")]
        [Alias("?")]
        [Summary("USAGE: !gacha help {PAGE:INT}")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            var module = _commandService.Modules.First(mod => mod.Name=="gacha");
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

        [Command("collections")]
        [Summary("List available collections.\nUSAGE: !gacha collections")]
        public async Task collectionsAsync()
        {
            var collections = await GameService.GetCollections();
            var response = $"";
            foreach(var collection in collections)
            {

                if(response.Length + collection.ToString().Length > 1990)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += $"- {collection.ToString()}\n";
           
            }
            await ReplyAsync(response); 
        }

        [Command("buy")]
        [Summary("Buy some items from a collection. Cost 50 RonPoints each.\nUSAGE: !gacha buy {collection} {quantity}")]
        public async Task buyAsync(string collectionName, int quantity)
        {
            if ( quantity < 1)
            {
                await ReplyAsync("Quantity must be greater than 0.");
                return;
            }
            if (quantity > 10)
            {
                await ReplyAsync("Quantity must be less than 11.");
                return;
            }

            var collections = (await GameService.GetCollections()).Where(x=>x.Name.ToLower() == collectionName.ToLower());

            if(collections.Count() == 0)
            {
                await ReplyAsync($"Collection: {collectionName} doesn't exist.");
                return;
            }
            var collection = collections.First();
            if(!await GameService.AddRonPoints(Context.User,-1*collection.Cost*quantity))
            {
                await ReplyAsync($"Not Enough Points! Costs {collection.Cost*quantity} RonPoints.");
                return;
            }
            var items = await GameService.PurchaseCollection(collectionName,quantity,Context.User);
            items = items.OrderByDescending(x=> x.Rarity);   
            var response = $"";
            
            var embedItems = new List<Embed>();
            foreach(var item in items)
            {

                embedItems.Add(CustomEmbeds.BuildEmbed(item));
           
            }
            await ReplyAsync("You have received the following items.",embeds: embedItems.ToArray());
        }


    }
}