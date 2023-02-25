using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Ronners.Bot.Models;
using Ronners.Bot.Models.GoogleBooks;

namespace Ronners.Bot
{
    public class CustomEmbeds
    {

        public static Embed BuildEmbed(IEnumerable<string> results, string title)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(title);
            builder.WithDescription(string.Join('\n',results));
            builder.WithCurrentTimestamp();
            return builder.Build();
        }

        public static Embed BuildEmbed(Item item)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(item.ToString());
            builder.WithDescription(item.Description);
            builder.WithColor(RarityToColor(item.Rarity));
            return builder.Build();
        }

        public static Embed BuildEmbed(IEnumerable<Item> items)
        {
            Dictionary<Item,int> itemDict = new Dictionary<Item, int>();

            foreach(var item in items)
            {
                if(itemDict.ContainsKey(item))
                    itemDict[item]++;
                else
                    itemDict[item] = 1;
            }

            EmbedBuilder builder = new EmbedBuilder();
            
            foreach(KeyValuePair<Item, int> pair in itemDict)
            {
                builder.AddField($"{pair.Value}x {pair.Key.ToString()}",$"- {pair.Key.Description}");
            }

            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        
        private static Color RarityToColor(Rarity rare)
        {
            switch(rare)
            {
                case Rarity.Common:
                return Color.DarkBlue;
                case Rarity.Uncommon:
                return Color.LighterGrey;
                case Rarity.Rare:
                return Color.Orange;
                case Rarity.SuperRare:
                return Color.Teal;
                case Rarity.UltraRare:
                return Color.Purple;
                default:
                return Color.Red;
            }
        }

        public static Embed BuildEmbed(IUser user, DailyResult result)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{user.Username} is on a {result.Streak} day streak!");
            builder.AddField("Daily Bonus:",result.DailyBonus);
            builder.AddField($"Streak Bonus ({result.StreakMulitiplier.ToString("P0")}):", result.StreakBonus);
            builder.AddField($"Daily Interest ({result.InterestRate.ToString("P0")}):",result.InterestBonus);

            builder.WithFooter($"Total points gained: {result.TotalBonus}");

            var completion = Math.Min((double)result.Streak/100,1);
            var color = new Color(Convert.ToByte((1-completion)*255), Convert.ToByte(completion*255),Convert.ToByte(0));
            builder.WithColor(color);
            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        public static Embed BuildEmbed(Models.JellyFin.Item song)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{song.Name} ----- {new TimeSpan(song.RunTimeTicks).ToString("h\\:mm\\:ss")}");
            builder.AddField("Artists",string.Join(',',song.Artists));
            builder.AddField("Album",song.Album);
            builder.WithFooter($"Replay Code: {song.Id}");
            return builder.Build();
        }

        public static Embed BuildEmbed(IEnumerable<UserItemDetailed> items, IEnumerable<Collection> collections)
        {  
            var collectionDict = new Dictionary<string,int>();
            foreach(var collection in collections)
            {
                collectionDict.Add(collection.Name,0);
            }
            var groupedItems = items.GroupBy(x=> x.Item.Collection);
            foreach(var group in groupedItems)
            {
                if(collectionDict.ContainsKey(group.Key))
                    collectionDict[group.Key] +=group.Count();
                else
                    collectionDict.Add(group.Key,group.Count());
            }
            var totalItems = collections.Sum(x=> x.NumberOfItems);
            var collectedItems = items.Count();

            var completion = (double)collectedItems/totalItems;
            var color = new Color(Convert.ToByte((1-completion)*255), Convert.ToByte(completion*255),Convert.ToByte(0));
            
            
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{collectedItems}/{totalItems} Unique Items Collected");
            builder.WithColor(color);
            foreach(var collection in collectionDict)
            {
                builder.Description+= $"- {collection.Key} ( {collection.Value} / {collections.First(x=> x.Name==collection.Key).NumberOfItems} )\n";
            }

            builder.WithCurrentTimestamp();
            return builder.Build();
        }

        public static Embed BuildEmbed(IEnumerable<UserItemDetailed> items,Collection collection)
        {
            EmbedBuilder builder = new EmbedBuilder();
            var completion = (double)items.Count()/collection.NumberOfItems;
            var color = new Color(Convert.ToByte((1-completion)*255), Convert.ToByte(completion*255),Convert.ToByte(0));
            builder.WithTitle($"{collection.Name} ({items.Count()}/{collection.NumberOfItems})");
            builder.WithColor(color);
            foreach(var item in items.OrderBy(x=> x.Item.Rarity))
            {
                builder.AddField($"{item.Item.Name} ({item.Item.RarityName()}) -- x{item.Quantity}",$"- {item.Item.Description}");
            }
            builder.WithCurrentTimestamp();
            return builder.Build();
        }

        public static Embed BuildEmbed(Volume book)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(book.VolumeInfo.Title);
            builder.WithAuthor(String.Join(',',book.VolumeInfo.Authors));
            builder.WithDescription(book.VolumeInfo.Description);
            builder.AddField("Page Count",book.VolumeInfo.PageCount);
            builder.WithUrl(book.VolumeInfo.InfoLink);
            return builder.Build();
        }

        public static Embed BuildEmbed(StockQuoteIEX quote)
        {
            EmbedBuilder builder = new EmbedBuilder();
            bool Up = quote.ChangePercent < 0;
            builder.WithTitle(quote.CompanyName);
            builder.AddField("Ticker",quote.Symbol);
            //builder.AddField("Price",string.Format("${0} \n{1} ({2}%)",quote.LatestPrice,quote.Change,quote.ChangePercent));
            builder.AddField("Price",string.Format("{0:c4}",quote.LatestPrice),true);
            builder.AddField("Change",string.Format("{0:0.00#################}",quote.Change),true);
            builder.AddField("Percent Change",string.Format("{0}%",quote.ChangePercent),true);
            builder.WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds((Int64)quote.LatestUpdate));
            builder.WithFooter("Stock Data provided by IEX Cloud https://iexcloud.io");
            if(quote.ChangePercent < 0)
                builder.WithColor(Color.Red);
            else if(quote.ChangePercent > 0)
                builder.WithColor(Color.Green);
            else
                builder.WithColor(Color.LightGrey);
            return builder.Build();
        }
    } 
}