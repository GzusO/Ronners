using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Interactions;
using System.Net.Http;
using System.IO;
using Ronners.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Dapper;
using Ronners.Bot.Modules;

namespace Ronners.Bot
{
    class Program
    {
        private string dbFileName ="ronners.db";
        private DiscordSocketClient _discord;
        private GameService gameService;

        private Timer ronStateTimer;

        private InteractionService inter;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            bool newDb = false;
            string connString = string.Format(@"Data Source={0};",dbFileName);
            if(!File.Exists(dbFileName))
            {
                newDb = true;
            }

            SqliteConnection connection = new SqliteConnection(connString);
            connection.Open();
            InitializeDB(connection,newDb);

            using (var services = ConfigureServices())
            {
                await services.GetRequiredService<ConfigService>().InitializeAsync();


                _discord = services.GetRequiredService<DiscordSocketClient>();

                gameService = services.GetRequiredService<GameService>();
                _discord.Log += LogAsync;
                _discord.Ready += ReadyAsync;
                _discord.MessageReceived += services.GetRequiredService<ImageService>().MessageReceivedAsync;
                _discord.MessageReceived += services.GetRequiredService<MarkovService>().MessageReceiveAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                services.GetRequiredService<GameService>().connection = connection;
                
                
                //stockTimer = new Timer(services.GetRequiredService<RonStockMarketService>().RefreshMarket,null,(int)TimeSpan.FromMinutes(1).TotalMilliseconds,(int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                
                //Load discord key from Config Service
                await _discord.LoginAsync(TokenType.Bot, ConfigService.Config.DiscordKey);
                await _discord.StartAsync();
                
                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                await services.GetRequiredService<HangmanService>().InitializeAsync();
                await services.GetRequiredService<ReactionService>().InitializeAsync();
                await services.GetRequiredService<CaptchaService>().InitializeAsync();
                await services.GetRequiredService<RonStockMarketService>().InitializeAsync(ConfigService.Config.StockJson);
                await services.GetRequiredService<AudioService>().InitializeAsync();
                await services.GetRequiredService<RonService>().InitializeAsync();

                inter = services.GetRequiredService<InteractionService>();

                ronStateTimer = new Timer(services.GetRequiredService<RonService>().UpdateRon,null,(int)TimeSpan.FromMinutes(1).TotalMilliseconds,(int)TimeSpan.FromSeconds(30).TotalMilliseconds);

                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task ReadyAsync()
        {
            await _discord.SetGameAsync("Ronners!");
            //await inter.RegisterCommandsToGuildAsync(312490455998660610,true);
        }
    

        private void InitializeDB(SqliteConnection conn, bool newDb)
        {
            string cmdTextVersionUpdate =@"UPDATE version set version = {0}";
            string cmdTextUsers = @"CREATE TABLE users(userid INTEGER, username TEXT, pogcount INTEGER, ronpoints INTEGER)";
            string cmdTextVersion = @"CREATE TABLE version(version INTEGER)";
            string cmdTextVersionInsert =@"INSERT INTO version (version) VALUES(1)";
            string cmdTextGetVersion = @"SELECT version FROM version LIMIT 1";
            string cmdTextIdeas = @"CREATE TABLE ideas(ideaid INTEGER PRIMARY KEY,idea TEXT, priority INTEGER)";
            string cmdTextCooldowns = @"CREATE TABLE cooldowns(command TEXT, lastexecuted INTEGER)";
            string cmdTextRetribution= @"CREATE TABLE retributions(RetributerUserId INTEGER, RetributeeUserId INTEGER, Reason TEXT, PointsRedistributed INTEGER, numUsers INTEGER, Time INTEGER, Success INTEGER)";
            string cmdTextAchievements = @"CREATE TABLE achievements(AchievementId INTEGER PRIMARY KEY, Name TEXT, Description TEXT, Score INTEGER)";
            string cmdTextUserAchievements = @"CREATE TABLE userachievements(UserAchievementId INTEGER PRIMARY KEY, UserID INTEGER, AchievementID INTEGER)";
            string cmdTextAchievementMessages =@"CREATE TABLE achievementmessages(AchievementMessageId INTEGER PRIMARY KEY, AchievementType INTEGER, IntValue INTEGER, StringValue TEXT, BoolValue INTEGER, DoubleValue REAL, UserID INTEGER)";
            string cmdInsertAchievement = @"INSERT INTO achievements(AchievementId,Name,Description,Score) VALUES(@achievementId,@name,@description,@score)";
            string cmdTextUserRonStock = @"CREATE TABLE userronstock(userid INTEGER, symbol TEXT, quantity INTEGER)";
            string cmdTextUserDaily = @"CREATE TABLE userdaily(userid INTEGER, lastcheckin INTEGER, streak INTEGER)";
            string cmdInsertItem = @"INSERT INTO item(name,collection,rarity,description) VALUES (@name,@collection,@rarity,@description)"; 

            SqliteCommand cmd;
            if(newDb)
            {
                cmd = new SqliteCommand(cmdTextUsers,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(cmdTextVersion,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(cmdTextVersionInsert,conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand(cmdTextGetVersion,conn);
            long version = (long)cmd.ExecuteScalar();

            if(version < 2)
            {
                cmd = new SqliteCommand(cmdTextIdeas,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 3)
            {
                cmd = new SqliteCommand(cmdTextCooldowns,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 4)
            {
                cmd = new SqliteCommand(cmdTextRetribution,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 5)
            {
                cmd = new SqliteCommand(cmdTextAchievements,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(cmdTextUserAchievements,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(cmdTextAchievementMessages,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 6)
            {
                conn.Execute(cmdInsertAchievement, new{achievementId=1,name="Nice!",description="Nice!",score=1});
                conn.Execute(cmdInsertAchievement, new{achievementId=2,name="Yes Man",description="3 Yes results from 8 ball in a row",score=5});
                conn.Execute(cmdInsertAchievement, new{achievementId=3,name="Wrath of Ron",description="Invoke Ron's Wrath",score=5});
                conn.Execute(cmdInsertAchievement, new{achievementId=4,name="Donners",description="Be Don",score=0});
                conn.Execute(cmdInsertAchievement, new{achievementId=5,name="Idea Man",description="Submit 10 ideas to Ronners",score=5});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 7)
            {
                conn.Execute(cmdInsertAchievement, new{achievementId=6,name="Blaze It",description="Smoke Weed Everyday",score=1});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 8)
            {
                conn.Execute(cmdInsertAchievement, new{achievementId=7,name="Definitely not a Robot",description="Passed one too many captchas",score=10});
                conn.Execute(cmdInsertAchievement, new{achievementId=8,name="Possibly a Robot",description="Failed one to many captchaz",score=10});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 9)
            {
                conn.Execute(cmdInsertAchievement, new{achievementId=9,name="Slurp God",description="Slurped 100 times.",score=10});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 10)
            {
                cmd = new SqliteCommand(cmdTextUserRonStock,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 11)
            {
                cmd = new SqliteCommand(cmdTextUserDaily,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 12)
            {
                var sqlString = @"CREATE TABLE item(itemid INTEGER PRIMARY KEY, name STRING, collection TEXT, rarity INTEGER, description TEXT)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"CREATE TABLE useritem(userid INTEGER, itemid INTEGER, quantity INTEGER)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 13)
            {
                var sqlString = @"CREATE TABLE ronstate(health INTEGER, hunger INTEGER, happiness INTEGER, birth INTEGER)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = $"INSERT INTO ronstate(health, hunger, happiness, birth) VALUES (100,100,100,{DateTimeOffset.UtcNow.ToUnixTimeSeconds()})";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 14)
            {
                var sqlString = @"ALTER TABLE ronstate ADD COLUMN experience integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"ALTER TABLE ronstate ADD COLUMN level integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 15)
            {
                var sqlString = @"ALTER TABLE ronstate ADD COLUMN skillpoints integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 16)
            {
                var sqlString = @"ALTER TABLE ronstate ADD COLUMN ronners integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN objectivity integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN nutrition integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN normalcy integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN erudition integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN rapidity integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();
                sqlString = @"ALTER TABLE ronstate ADD COLUMN strength integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 17)
            {
                var sqlString = @"CREATE TABLE rontalent(talent TEXT)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"ALTER TABLE ronstate ADD COLUMN talentpoints integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"UPDATE ronstate SET talentpoints = 0";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"ALTER TABLE ronstate ADD COLUMN maxhealth integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"UPDATE ronstate SET maxhealth = 100";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"ALTER TABLE ronstate ADD COLUMN maxhunger integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"UPDATE ronstate SET maxhunger = 100";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"ALTER TABLE ronstate ADD COLUMN maxhappiness integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"UPDATE ronstate SET maxhappiness = 100";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 18)
            {
                var sqlString = @"CREATE TABLE ronthread(threadid INTEGER, guildid INTEGER)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 19)
            {
                conn.Execute(cmdInsertItem, new{name="Banana",collection="Fruit",rarity=Models.Rarity.Common,description="For scale only."});
                conn.Execute(cmdInsertItem, new{name="Pineapple",collection="Fruit",rarity= Models.Rarity.Common,description="The better apple."});
                conn.Execute(cmdInsertItem, new{name="Pomegranate",collection="Fruit",rarity= Models.Rarity.Common,description="Sweet, tart fruit with thick, red skin."});
                conn.Execute(cmdInsertItem, new{name="Blueberry",collection="Fruit",rarity= Models.Rarity.Common,description="Blue or purple berries."});
                conn.Execute(cmdInsertItem, new{name="Apple",collection="Fruit",rarity= Models.Rarity.Common,description="With more than 7,500 known cultivars, who knows which kind this is."});
                conn.Execute(cmdInsertItem, new{name="Grapefruit",collection="Fruit",rarity= Models.Rarity.Uncommon,description="A natural hybrid."});
                conn.Execute(cmdInsertItem, new{name="Lemon",collection="Fruit",rarity= Models.Rarity.Uncommon,description="The juice of the lemon is about 5% to 6% citric acid, with a pH of around 2.2."});
                conn.Execute(cmdInsertItem, new{name="Papaya",collection="Fruit",rarity= Models.Rarity.Uncommon,description="Papaya, Papaw, or Pawpaw?"});
                conn.Execute(cmdInsertItem, new{name="Strawberry",collection="Fruit",rarity= Models.Rarity.Uncommon,description="Technically not a berry."});
                conn.Execute(cmdInsertItem, new{name="Orange",collection="Fruit",rarity= Models.Rarity.Rare,description="Orange."});
                conn.Execute(cmdInsertItem, new{name="Watermelon",collection="Fruit",rarity= Models.Rarity.Rare,description="Do you know how much water this bad boy can hold?"});
                conn.Execute(cmdInsertItem, new{name="Mango",collection="Fruit",rarity= Models.Rarity.Rare,description="Mango is the national fruit of India, Pakistan and the Philippines."});
                conn.Execute(cmdInsertItem, new{name="Peach",collection="Fruit",rarity= Models.Rarity.SuperRare,description="Or is this a nectarine?"});
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Fruit",rarity= Models.Rarity.SuperRare,description="Not to be confused with the bird."});
                conn.Execute(cmdInsertItem, new{name="Dragonfruit",collection="Fruit",rarity= Models.Rarity.UltraRare,description="Now this is a lot of seeds."});

                conn.Execute(cmdInsertItem, new{name="King Crab",collection="Crab",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Peekytoe Crab",collection="Crab",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Pea crab",collection="Crab",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Japanese Spider Crab",collection="Crab",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Floral Egg Crab",collection="Crab",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Heikegani",collection="Crab",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Kelp Crab",collection="Crab",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Dungeness Crab",collection="Crab",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Gazami Crab",collection="Crab",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Snow Crab",collection="Crab",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Blue Crab",collection="Crab",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Southern European Crab",collection="Crab",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Horsehair Crab",collection="Crab",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Tasmanian Giant Crab",collection="Crab",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Coconut Crab",collection="Crab",rarity= Models.Rarity.UltraRare,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 20)
            {
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Bird",rarity=Models.Rarity.Common,description="Not to be confused with New Zealanders."});
                conn.Execute(cmdInsertItem, new{name="Canada Goose",collection="Bird",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="American Robin",collection="Bird",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Horned puffin",collection="Bird",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Hyacinth Macaw",collection="Bird",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Burrowing owl",collection="Bird",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Turkey",collection="Bird",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Common Loon",collection="Bird",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Emu",collection="Bird",rarity= Models.Rarity.Uncommon,description="Emus 1. Australia 0."});
                conn.Execute(cmdInsertItem, new{name="Great Potoo",collection="Bird",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Snowy Owl",collection="Bird",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Adélie Penguin",collection="Bird",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Dickcissel",collection="Bird",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Frigatebird",collection="Bird",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Resplendent Quetzal",collection="Bird",rarity= Models.Rarity.UltraRare,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 21)
            {
                conn.Execute(cmdInsertItem, new{name="Yellowfin Tuna",collection="Fish",rarity=Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Chinook Salmon",collection="Fish",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Pike",collection="Fish",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Koi",collection="Fish",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Humuhumunukunukuapua'a",collection="Fish",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Lingcod",collection="Fish",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Alligator Gar",collection="Fish",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Mahi-Mahi",collection="Fish",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Sarcastic Fringehead",collection="Fish",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Mola Mola",collection="Fish",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Torafugu",collection="Fish",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Stoplight Loosejaw",collection="Fish",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Arapaima",collection="Fish",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Thresher Shark",collection="Fish",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Tripodfish",collection="Fish",rarity= Models.Rarity.UltraRare,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 22)
            {
                conn.Execute(cmdInsertItem, new{name="Georg Friedrich Bernhard Riemann",collection="Math",rarity=Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Thomas Joannes Stieltjes",collection="Math",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Carl Friedrich Gauss",collection="Math",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Leonhard Euler",collection="Math",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="John von Neumann",collection="Math",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Archimedes",collection="Math",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="George Boole",collection="Math",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Peter Gustav Lejeune Dirichlet",collection="Math",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Pafnuty Chebyshev",collection="Math",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Laurent-Moïse Schwartz",collection="Math",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Heron of Alexandria",collection="Math",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Pythagoras",collection="Math",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Baron Augustin-Louis Cauchy",collection="Math",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Christian Felix Klein",collection="Math",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Grigori Yakovlevich Perelman",collection="Math",rarity= Models.Rarity.UltraRare,description=""});

                conn.Execute(cmdInsertItem, new{name="Walrus",collection="Mammal",rarity=Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Golden-Capped Fruit Bat",collection="Mammal",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Mountain Bongo",collection="Mammal",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Shiba Inu",collection="Mammal",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Polar Bear",collection="Mammal",rarity= Models.Rarity.Common,description=""});
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Mammal",rarity= Models.Rarity.Uncommon,description="Not to be confused with the fruit."});
                conn.Execute(cmdInsertItem, new{name="Blue Whale",collection="Mammal",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Red Panda",collection="Mammal",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Mountain Tapir",collection="Mammal",rarity= Models.Rarity.Uncommon,description=""});
                conn.Execute(cmdInsertItem, new{name="Capybara",collection="Mammal",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Okapi",collection="Mammal",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Aye-aye",collection="Mammal",rarity= Models.Rarity.Rare,description=""});
                conn.Execute(cmdInsertItem, new{name="Dugong",collection="Mammal",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Platypus",collection="Mammal",rarity= Models.Rarity.SuperRare,description=""});
                conn.Execute(cmdInsertItem, new{name="Giant Pangolin",collection="Mammal",rarity= Models.Rarity.UltraRare,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 23)
            {
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Common,description="Ronners eating a cheeseburger."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Common,description="Ronners driving a supercar."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Common,description="Ronners shoveling their deck."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Common,description="Ronners drinking an energy drink after 6 pm."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Common,description="Ronners incoherently ranting."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Uncommon,description="Ronners is just had to spoil the treasury!*"});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Uncommon,description="Ronners playing Oddball."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Uncommon,description="Ronners lost in the woods."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Uncommon,description="Ronners microwaving fish in the office breakroom."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Rare,description="Ronners sleeping."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Rare,description="Ronners riding in the Groton."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.Rare,description="Ronners' first day of school."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.SuperRare,description="Ronners holding a ladder."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.SuperRare,description="Ronners in the discord voice chat."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= Models.Rarity.UltraRare,description="Ronners!"});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }


            cmd = new SqliteCommand(String.Format(cmdTextVersionUpdate,version),conn);
            cmd.ExecuteNonQuery();
        }

        private ServiceProvider ConfigureServices()
        {
            var cmdConfig = new CommandServiceConfig(){
                IgnoreExtraArgs = true
                };
            var clientConfig = new DiscordSocketConfig(){
                MessageCacheSize = 1000,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildWebhooks | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping
                
            };
            return new ServiceCollection()
                .AddSingleton<ConfigService>()
                .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(clientConfig))
                .AddSingleton<CommandService>(_ => new CommandService(cmdConfig))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<Random>()
                .AddSingleton<AchievementService>()
                .AddSingleton<AudioService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HangmanService>()
                .AddSingleton<ReactionService>()
                .AddSingleton<BaccaratService>()
                .AddSingleton<CaptchaService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<ImageService>()
                .AddSingleton<MarkovService>()
                .AddSingleton<StockService>()
                .AddSingleton<BookService>()
                .AddSingleton<GameService>()
                .AddSingleton<WebService>()
                .AddSingleton<RouletteService>()
                .AddSingleton<SlotService>()
                .AddSingleton<RonStockMarketService>()
                .AddSingleton<RonService>()
                //.AddSingleton<LootGenerator>(_ => new LootGenerator("TestData/"))
                .BuildServiceProvider();
        }

        private async Task LogAsync(LogMessage log)
        {
            await LoggingService.LogAsync(log.Source,log.Severity,log.Message);
        }
    }
}