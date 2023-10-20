using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Ronners.Bot.Models;
using Ronners.Bot.Models.GoogleBooks;
using Ronners.Bot.Models.Lancer;
using Ronners.Bot.Services;

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

        
        private static Color RarityToColor(int rare)
        {
            switch(rare)
            {
                case >=15:
                    return Color.DarkBlue;
                case >=10:
                    return Color.LighterGrey;
                case >=8:
                    return Color.Orange;
                case >=5:
                    return Color.Teal;
                case >=1:
                    return Color.Purple;
                default:
                    return Color.Red;
            }
        }

        public static Embed BuildEmbed(IUser user, DailyResult result)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{user.Username} is on a {result.Streak} day streak!");
            builder.AddField($"Daily Bonus({result.BonusLowerBound} - {result.BonusUpperBound})",result.DailyBonus);
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
            var collectionDict = new Dictionary<int,int>();
            foreach(var collection in collections)
            {
                collectionDict.Add(collection.CollectionID,0);
            }
            var groupedItems = items.GroupBy(x=> x.Item.CollectionID);
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
                var collectionDetails = collections.First(x=> x.CollectionID == collection.Key);
                builder.Description+= $"- {collectionDetails.Name} ( {collection.Value} / {collectionDetails.NumberOfItems} )\n";
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

        public static Embed BuildEmbed(BlackjackState state)
        {
            Discord.Color color = Color.DarkerGrey;
            string outcome = "In Progress";
            switch(state.GameState){
                case Outcome.Starting:
                    outcome ="Starting Soon";
                    break;
                case Outcome.Lose:
                    color = Color.Red;
                    outcome = "Loss";
                break;
                case Outcome.Win:
                    color = Color.Green;
                    outcome = "Win";
                    break;
                case Outcome.Blackjack:
                    color = Color.Gold;
                    outcome = "Blackjack";
                    break;
                case Outcome.Draw:
                    color = Color.Blue;
                    outcome = "Draw";
                    break;
                default:
                break;
            }
            string dealerString = string.Join(" ",state.Dealer.First().ToString());
            string playerString = string.Join(" ",state.Player.Select(x=> x.ToString()));

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(color);
            builder.WithTitle($"Blackjack - {outcome}.");
            if(state.GameState == Outcome.Starting)
                builder.WithDescription("Starting soon please wait.");
            else
            {
                builder.AddField("Dealer Hand",dealerString);
                builder.AddField("Player Hand",playerString);
            }
            return builder.Build();
        }

        internal static Embed BuildEmbed(FrameData frame)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(ManufacturerColor(frame.Source));
            builder.WithTitle($"{frame.Name} - {string.Join(',',frame.MechType)}");
            builder.WithDescription(frame.Description);
            builder.WithAuthor($"{frame.Source}",ManufactureIconUrl(frame.Source));
            builder.AddField("Size",frame.Stats.Size,true);
            builder.AddField("HP",frame.Stats.HP,true);
            builder.AddField("Heat Cap",frame.Stats.HeatCap,true);
            builder.AddField("Repair Cap",frame.Stats.RepCap,true);
            builder.AddField("Evasion",frame.Stats.Evasion,true);
            builder.AddField("E-Defense",frame.Stats.EDef,true);

            return builder.Build();
        }

        private static string ManufactureIconUrl(string Manufacturer)
        {
            switch(Manufacturer)
            {
                case "SSC":
                    return @"https://imgur.com/gl8vjn3.png";
                case "IPS-N":
                    return @"https://imgur.com/a3fZ7cL.png";
                case "HORUS":
                    return @"https://imgur.com/DsgB4RO.png";
                case "HA":
                    return @"https://i.imgur.com/RuYpZgX.png";
                default:
                    return null;
            }
        }

        private static Color ManufacturerColor(string Manufacturer)
        {
            switch(Manufacturer)
            {
                case "SSC":
                    return new Color(209,146,10);
                case "IPS-N":
                    return new Color(28,154,232);
                case "HORUS":
                    return new Color(0,162,86);
                case "HA":
                    return new Color(161,94,168);
                default:
                    return Color.DarkerGrey;
            }
        }
    } 
}