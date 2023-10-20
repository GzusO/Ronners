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


        public async Task ButtonHandler(SocketMessageComponent  comp)
        {
             Console.WriteLine(comp.Id);
        }

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
                _discord.ButtonExecuted += ButtonHandler;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                services.GetRequiredService<GameService>().connection = connection;
                
                
                //stockTimer = new Timer(services.GetRequiredService<RonStockMarketService>().RefreshMarket,null,(int)TimeSpan.FromMinutes(1).TotalMilliseconds,(int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                
                //Load discord key from Config Service
                await _discord.LoginAsync(TokenType.Bot, ConfigService.Config.DiscordKey);
                await _discord.StartAsync();
                
                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                await services.GetRequiredService<AdminService>().InitializeAsync();
                await services.GetRequiredService<HangmanService>().InitializeAsync();
                await services.GetRequiredService<ReactionService>().InitializeAsync();
                await services.GetRequiredService<CaptchaService>().InitializeAsync();
                await services.GetRequiredService<RonStockMarketService>().InitializeAsync(ConfigService.Config.StockJson);
                await services.GetRequiredService<AudioService>().InitializeAsync();
                await services.GetRequiredService<RonService>().InitializeAsync();
                await services.GetRequiredService<BattleService>().InitializeAsync();
                await services.GetRequiredService<LancerService>().InitializeAsync(ConfigService.Config.LCP);
                await services.GetRequiredService<FishingService>().InitializeAsync(ConfigService.Config.FishJson);

                inter = services.GetRequiredService<InteractionService>();

                //ronStateTimer = new Timer(services.GetRequiredService<RonService>().UpdateRon,null,(int)TimeSpan.FromMinutes(1).TotalMilliseconds,(int)TimeSpan.FromSeconds(30).TotalMilliseconds);

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
            string cmdInsertCollection = @"INSERT INTO collection (name, cost) VALUES(@name, @cost)";

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
                conn.Execute(cmdInsertItem, new{name="Banana",collection="Fruit",rarity=15,description="For scale only."});
                conn.Execute(cmdInsertItem, new{name="Pineapple",collection="Fruit",rarity= 15,description="The better apple."});
                conn.Execute(cmdInsertItem, new{name="Pomegranate",collection="Fruit",rarity= 15,description="Sweet, tart fruit with thick, red skin."});
                conn.Execute(cmdInsertItem, new{name="Blueberry",collection="Fruit",rarity= 15,description="Blue or purple berries."});
                conn.Execute(cmdInsertItem, new{name="Apple",collection="Fruit",rarity= 15,description="With more than 7,500 known cultivars, who knows which kind this is."});
                conn.Execute(cmdInsertItem, new{name="Grapefruit",collection="Fruit",rarity= 10,description="A natural hybrid."});
                conn.Execute(cmdInsertItem, new{name="Lemon",collection="Fruit",rarity= 10,description="The juice of the lemon is about 5% to 6% citric acid, with a pH of around 2.2."});
                conn.Execute(cmdInsertItem, new{name="Papaya",collection="Fruit",rarity= 10,description="Papaya, Papaw, or Pawpaw?"});
                conn.Execute(cmdInsertItem, new{name="Strawberry",collection="Fruit",rarity= 10,description="Technically not a berry."});
                conn.Execute(cmdInsertItem, new{name="Orange",collection="Fruit",rarity= 8,description="Orange."});
                conn.Execute(cmdInsertItem, new{name="Watermelon",collection="Fruit",rarity= 8,description="Do you know how much water this bad boy can hold?"});
                conn.Execute(cmdInsertItem, new{name="Mango",collection="Fruit",rarity= 8,description="Mango is the national fruit of India, Pakistan and the Philippines."});
                conn.Execute(cmdInsertItem, new{name="Peach",collection="Fruit",rarity= 5,description="Or is this a nectarine?"});
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Fruit",rarity= 5,description="Not to be confused with the bird."});
                conn.Execute(cmdInsertItem, new{name="Dragonfruit",collection="Fruit",rarity= 1,description="Now this is a lot of seeds."});

                conn.Execute(cmdInsertItem, new{name="King Crab",collection="Crab",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Peekytoe Crab",collection="Crab",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Pea crab",collection="Crab",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Japanese Spider Crab",collection="Crab",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Floral Egg Crab",collection="Crab",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Heikegani",collection="Crab",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Kelp Crab",collection="Crab",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Dungeness Crab",collection="Crab",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Gazami Crab",collection="Crab",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Snow Crab",collection="Crab",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Blue Crab",collection="Crab",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Southern European Crab",collection="Crab",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Horsehair Crab",collection="Crab",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Tasmanian Giant Crab",collection="Crab",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Coconut Crab",collection="Crab",rarity= 1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 20)
            {
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Bird",rarity=15,description="Not to be confused with New Zealanders."});
                conn.Execute(cmdInsertItem, new{name="Canada Goose",collection="Bird",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="American Robin",collection="Bird",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Horned puffin",collection="Bird",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Hyacinth Macaw",collection="Bird",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Burrowing owl",collection="Bird",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Turkey",collection="Bird",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Common Loon",collection="Bird",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Emu",collection="Bird",rarity= 10,description="Emus 1. Australia 0."});
                conn.Execute(cmdInsertItem, new{name="Great Potoo",collection="Bird",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Snowy Owl",collection="Bird",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Adélie Penguin",collection="Bird",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Dickcissel",collection="Bird",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Frigatebird",collection="Bird",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Resplendent Quetzal",collection="Bird",rarity= 1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 21)
            {
                conn.Execute(cmdInsertItem, new{name="Yellowfin Tuna",collection="Fish",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Chinook Salmon",collection="Fish",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Pike",collection="Fish",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Koi",collection="Fish",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Humuhumunukunukuapua'a",collection="Fish",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Lingcod",collection="Fish",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Alligator Gar",collection="Fish",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Mahi-Mahi",collection="Fish",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Sarcastic Fringehead",collection="Fish",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Mola Mola",collection="Fish",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Torafugu",collection="Fish",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Stoplight Loosejaw",collection="Fish",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Arapaima",collection="Fish",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Thresher Shark",collection="Fish",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Tripodfish",collection="Fish",rarity= 1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 22)
            {
                conn.Execute(cmdInsertItem, new{name="Georg Friedrich Bernhard Riemann",collection="Math",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Thomas Joannes Stieltjes",collection="Math",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Carl Friedrich Gauss",collection="Math",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Leonhard Euler",collection="Math",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="John von Neumann",collection="Math",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Archimedes",collection="Math",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="George Boole",collection="Math",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Peter Gustav Lejeune Dirichlet",collection="Math",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Pafnuty Chebyshev",collection="Math",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Laurent-Moïse Schwartz",collection="Math",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Heron of Alexandria",collection="Math",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Pythagoras",collection="Math",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Baron Augustin-Louis Cauchy",collection="Math",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Christian Felix Klein",collection="Math",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Grigori Yakovlevich Perelman",collection="Math",rarity= 1,description=""});

                conn.Execute(cmdInsertItem, new{name="Walrus",collection="Mammal",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Golden-Capped Fruit Bat",collection="Mammal",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Mountain Bongo",collection="Mammal",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Shiba Inu",collection="Mammal",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Polar Bear",collection="Mammal",rarity= 15,description=""});
                conn.Execute(cmdInsertItem, new{name="Kiwi",collection="Mammal",rarity= 10,description="Not to be confused with the fruit."});
                conn.Execute(cmdInsertItem, new{name="Blue Whale",collection="Mammal",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Red Panda",collection="Mammal",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Mountain Tapir",collection="Mammal",rarity= 10,description=""});
                conn.Execute(cmdInsertItem, new{name="Capybara",collection="Mammal",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Okapi",collection="Mammal",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Aye-aye",collection="Mammal",rarity= 8,description=""});
                conn.Execute(cmdInsertItem, new{name="Dugong",collection="Mammal",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Platypus",collection="Mammal",rarity= 5,description=""});
                conn.Execute(cmdInsertItem, new{name="Giant Pangolin",collection="Mammal",rarity= 1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 23)
            {
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 15,description="Ronners eating a cheeseburger."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 15,description="Ronners driving a supercar."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 15,description="Ronners shoveling their deck."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 15,description="Ronners drinking an energy drink after 6 pm."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 15,description="Ronners incoherently ranting."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 10,description="Ronners is just had to spoil the treasury!*"});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 10,description="Ronners playing Oddball."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 10,description="Ronners lost in the woods."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 10,description="Ronners microwaving fish in the office breakroom."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 8,description="Ronners sleeping."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 8,description="Ronners riding in the Groton."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 8,description="Ronners' first day of school."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 5,description="Ronners holding a ladder."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 5,description="Ronners in the discord voice chat."});
                conn.Execute(cmdInsertItem, new{name="Ronners",collection="Ronners",rarity= 1,description="Ronners!"});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 24)
            {
                var sqlString = @"CREATE TABLE rongifts(RonGiftID INTEGER PRIMARY KEY, ReceivedDate INTEGER, ReturnDate INTEGER, ReceivedPoints INTEGER, ReturnPoints INTEGER, UserID INTEGER, Returned INTEGER)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 25)
            {
                conn.Execute(cmdInsertItem, new{name="Oxygen",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Silicon",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Aluminium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Iron",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Calcium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Sodium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Magnesium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Potassium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Titanium",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Hydrogen",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Phosphorus",collection="Element",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Manganese",collection="Element",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Fluorine",collection="Element",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Barium",collection="Element",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Strontium",collection="Element",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Sulfur",collection="Element",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Carbon",collection="Element",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Zirconium",collection="Element",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Chlorine",collection="Element",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Vanadium",collection="Element",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Chromium",collection="Element",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Rubidium",collection="Element",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Nickel",collection="Element",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Zinc",collection="Element",rarity=1,description=""});
                conn.Execute(cmdInsertItem, new{name="Cerium",collection="Element",rarity=1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 26)
            {
                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 27)
            {
                conn.Execute(cmdInsertItem, new{name="Broken CD",collection="Junk",rarity=100,description=""});
                conn.Execute(cmdInsertItem, new{name="Empty Milk Carton",collection="Junk",rarity=100,description=""});
                conn.Execute(cmdInsertItem, new{name="Pile of Dirt",collection="Junk",rarity=100,description=""});
                conn.Execute(cmdInsertItem, new{name="Dead D Battery",collection="Junk",rarity=100,description=""});
                conn.Execute(cmdInsertItem, new{name="Paint Chips",collection="Junk",rarity=99,description="Yum red flavored!"});
                conn.Execute(cmdInsertItem, new{name="Shiny RonCoin",collection="Junk",rarity=1,description="Shiny but useless"});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 28)
            {
                conn.Execute(cmdInsertItem, new{name="Saltwater Crocodile",collection="Reptile",rarity=15,description="Can reach lengths of well over 6 m (19.7 ft.)"});
                conn.Execute(cmdInsertItem, new{name="Slow-Worm",collection="Reptile",rarity=15,description="Not a snake."});
                conn.Execute(cmdInsertItem, new{name="Komodo Dragon",collection="Reptile",rarity=15,description="Growing to 3 meters (10 ft.) in length, the Komodo dragon is the world's largest lizard."});
                conn.Execute(cmdInsertItem, new{name="Green Anaconda",collection="Reptile",rarity=15,description="The green anaconda is the world's heaviest species of snake."});
                conn.Execute(cmdInsertItem, new{name="Gharial",collection="Reptile",rarity=15,description="The snout of the gharial is long and thin, and filled with sharp, narrow teeth;"});
                conn.Execute(cmdInsertItem, new{name="Thorny Devil",collection="Reptile",rarity=10,description="The thorny devil has a number of adaptations for living in the desert, including grooves in its skin that channel moisture to its mouth."});
                conn.Execute(cmdInsertItem, new{name="Alligator Snapping Turtle",collection="Reptile",rarity=10,description="The largest individuals can reach weights of over 220 lb. (100 kg)."});
                conn.Execute(cmdInsertItem, new{name="Gila Monster",collection="Reptile",rarity=10,description="It is generally regarded as having the most painful venom produced by any vertebrate."});
                conn.Execute(cmdInsertItem, new{name="American Alligator",collection="Reptile",rarity=10,description="One of only two living alligators in the world."});
                conn.Execute(cmdInsertItem, new{name="Inland Taipan",collection="Reptile",rarity=8,description="The world's most venomous snake."});
                conn.Execute(cmdInsertItem, new{name="Leatherback Sea Turtle",collection="Reptile",rarity=8,description="With a recorded top speed of 21.92 mph / 35.28 km/h"});
                conn.Execute(cmdInsertItem, new{name="Brahminy Blind Snake",collection="Reptile",rarity=8,description="The tiny eyes are covered with translucent scales, rendering these snakes almost entirely blind."});
                conn.Execute(cmdInsertItem, new{name="Mojave Desert Tortoise",collection="Reptile",rarity=5,description="Has a shell length of 9 to 14.5 in. (23 to 37 cm)."});
                conn.Execute(cmdInsertItem, new{name="Noronha Skink",collection="Reptile",rarity=5,description="A species of skink from the island of Fernando de Noronha off northeastern Brazil."});
                conn.Execute(cmdInsertItem, new{name="Nano-Chameleon",collection="Reptile",rarity=1,description="Adult males measure 22 mm (0.87 inch) in total length (including tail)"});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 29)
            {
                conn.Execute(cmdInsertItem, new{name="Fire Salamander",collection="Amphibian",rarity=15,description="Fire salamanders can have a very long lifespan; one specimen lived for more than 50 years."});
                conn.Execute(cmdInsertItem, new{name="Crucifix Toad",collection="Amphibian",rarity=15,description="The frog exudes a tacky and elastic 'frog glue' onto its dorsal skin when provoked."});
                conn.Execute(cmdInsertItem, new{name="Hellbender",collection="Amphibian",rarity=15,description="It is the largest salamander in North America."});
                conn.Execute(cmdInsertItem, new{name="Green Tree Frog",collection="Amphibian",rarity=15,description="The green tree frog is a large species of tree frog, with a body length of up to 11.5 cm (4.5 in)."});
                conn.Execute(cmdInsertItem, new{name="Algerian Ribbed Newt",collection="Amphibian",rarity=15,description="Found in Algeria and Tunisia."});
                conn.Execute(cmdInsertItem, new{name="Nauta Salamander",collection="Amphibian",rarity=10,description="It is a member of the family Plethodontidae. Members of this family are known as “lungless salamanders”."});
                conn.Execute(cmdInsertItem, new{name="Cane Toad",collection="Amphibian",rarity=10,description="Large glands behind the cane toad's eyes produce powerful toxins."});
                conn.Execute(cmdInsertItem, new{name="Siberian Salamander",collection="Amphibian",rarity=10,description="They produce a special chemical called glycerol to prevent ice crystals from damaging their bodies' cells during prolonged sub-freezing conditions."});
                conn.Execute(cmdInsertItem, new{name="Horned Marsupial Frog",collection="Amphibian",rarity=10,description="Marsupial frogs are so-named because their young develop in pouches on the female's back."});
                conn.Execute(cmdInsertItem, new{name="Chinese Giant Salamander",collection="Amphibian",rarity=8,description="World's biggest amphibian, it grows to lengths of up to 1.8 m (5.9 ft.) and weighs up to 30 kg (66 lb.)."});
                conn.Execute(cmdInsertItem, new{name="Paedophryne amauensis",collection="Amphibian",rarity=8,description="It is considered the world's smallest known vertebrate."});
                conn.Execute(cmdInsertItem, new{name="Goliath Frog",collection="Amphibian",rarity=8,description="The goliath frog is the world's largest frog. This African amphibian has a body length of up to 32 cm / 12.6 in. and can weigh up to 3.25 kg / 7.17 lb."});
                conn.Execute(cmdInsertItem, new{name="Archey's Frog",collection="Amphibian",rarity=5,description="One of the world's most primitive species of frog."});
                conn.Execute(cmdInsertItem, new{name="Axolotl",collection="Amphibian",rarity=5,description="Axolotls have the ability to regenerate lost limbs and other body parts."});
                conn.Execute(cmdInsertItem, new{name="Red Eyed Tree Frog",collection="Amphibian",rarity=1,description="The red-eyed tree frog is one of the world's most famous amphibians."});
            
                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 30)
            {
                conn.Execute(cmdInsertItem, new{name="Cat Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Afroedura maripi",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Alsophylax przewalskii",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Bauerius ansorgii",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Asaccus kermanshahensis",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Western Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Swan Islands Croaking Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Slim Velvet Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Leviton's Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Kaspischer Even-fingered Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Yellow Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Spider Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Sadlier's New Caledonian Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Victor's Madagascar Velvet gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Thickhead Rock Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Indian golden gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Chameleon Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Common Giant Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Alexander's Southern Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Adang-Rawi Rock Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Agarwal's dwarf gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Goias Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Yucatan Banded Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Central Uplands clawless geckos",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Comb-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Salt Marsh Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Alder's bow-fingered gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Takou bent-toed gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Taung Wine Hill Bent-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Thathom Bent-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Thochu Bent-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Tioman Island Bent-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Makran Spider Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Warty Rock Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Potahar Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Poum Striped Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Cape Range Stone Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Pilbara Beak-faced Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Speckled Stone Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Fine-faced Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Cha-am leaf-toed gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Wayanad Dravidogecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Kodaikanal Dravidogecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Warty Thick-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Painted leopard gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Symmetrical Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Coquimbo Marked Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Peters' Spotted Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Western Top End Gehyra",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Pacific Stump-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Port Moresby four-clawed gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Southern Kimberley spotted gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Brown's Fringe Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Gigante Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Schlegel's Japanese Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Kwangsi Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Sengchanthavong's Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Thakhek Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Richtersveld Dwarf Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Striped Dwarf Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Annulated Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="South American Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Gollum Leopard Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Sengoku's Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Yamashina's Leopard Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Brazilian Naked-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Emulous leaf-toed gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="White-spotted gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Annobon Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Somali Banded Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Boavista Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Easa's rock gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Grant's Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Bridled house gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Burmese spotted gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Largescale Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Newton's Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Sharpnose Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Persia Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Richardson's Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Northern Somali Leaf-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Meghamalai Rock Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Southern Ghats slender gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Hong Kong Slender Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Longling slender gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Phapant dwarf gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Fat-tail Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Prickly Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Brown Dwarf Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Golden Scaly-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Eua Scaly-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Mourning Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Oriental Scaly-toed Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Slender Chained Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Alligator River Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Philippine Wolf Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Persia Sand Gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Jewelled gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Inland marbled velvet gecko",collection="100Gecs",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Transvaal Gecko",collection="100Gecs",rarity=15,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 31)
            {
                var sqlString = @"CREATE TABLE collection(collectionID INTEGER PRIMARY KEY, name string, cost integer)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"insert into collection (name,cost) select t.collection,50 from (select DISTINCT collection from item) as t";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"alter table item add column collectionID INTEGER";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"update item set collectionID=(select collectionID from collection c where c.Name = item.collection)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 32)
            {
                var sqlString = @"CREATE TABLE collection(collectionID INTEGER PRIMARY KEY, name string, cost integer)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"insert into collection (name,cost) select t.collection,50 from (select DISTINCT collection from item) as t";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"alter table item add column collectionID INTEGER";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"update item set collectionID=(select collectionID from collection c where c.Name = item.collection)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if(version < 33)
            {
                conn.Execute(cmdInsertItem, new{name="D1",collection="Dice",rarity=1,description=""});
                conn.Execute(cmdInsertItem, new{name="D2",collection="Dice",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="D4",collection="Dice",rarity=25,description=""});
                conn.Execute(cmdInsertItem, new{name="D6",collection="Dice",rarity=50,description=""});
                conn.Execute(cmdInsertItem, new{name="D8",collection="Dice",rarity=25,description=""});
                conn.Execute(cmdInsertItem, new{name="D10",collection="Dice",rarity=25,description=""});
                conn.Execute(cmdInsertItem, new{name="D12",collection="Dice",rarity=25,description=""});
                conn.Execute(cmdInsertItem, new{name="D14",collection="Dice",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="D20",collection="Dice",rarity=50,description=""});
                conn.Execute(cmdInsertItem, new{name="D120",collection="Dice",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Milionia Moth",collection="Lepidoptera",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Monarch butterfly",collection="Lepidoptera",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Glasswinged Butterfly",collection="Lepidoptera",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Dragontail Butterflies",collection="Lepidoptera",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Hercules Moth",collection="Lepidoptera",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Emerald Swallowtail",collection="Lepidoptera",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="White Witch",collection="Lepidoptera",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Forest Giant Owl Butterfly",collection="Lepidoptera",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Rosy Maple Moth",collection="Lepidoptera",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Dead Leaf Butterfly",collection="Lepidoptera",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Queen Alexandra's Birdwing",collection="Lepidoptera",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Sunset Moth",collection="Lepidoptera",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="The Basker",collection="Lepidoptera",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Starry Night Cracker",collection="Lepidoptera",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Luna Moth",collection="Lepidoptera",rarity=1,description=""});
                conn.Execute(cmdInsertItem, new{name="Dump Truck",collection="Construction",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Excavator",collection="Construction",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Concrete Mixer Truck",collection="Construction",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Skid Steer Loader",collection="Construction",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Trencher",collection="Construction",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Boom Lift",collection="Construction",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Double Drum Roller",collection="Construction",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Wheel Tractor Scraper",collection="Construction",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Backhoe",collection="Construction",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Feller Buncher",collection="Construction",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Motor Grader",collection="Construction",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Crane",collection="Construction",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Bulldozer",collection="Construction",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Tractor",collection="Construction",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Forklift",collection="Construction",rarity=1,description=""});

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if(version < 34)
            {
                conn.Execute(cmdInsertCollection, new{name="Dice",cost=50});
                conn.Execute(cmdInsertCollection, new{name="Construction",cost=50});
                conn.Execute(cmdInsertCollection, new{name="Lepidoptera",cost=50});

                var sqlString = @"update item set collectionID=(select collectionID from collection c where c.Name = item.collection)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }

            if( version < 35)
            {
                conn.Execute(cmdInsertItem, new{name="Lockheed U-2",collection="Planes",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Boeing 737",collection="Planes",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Supermarine Spitfire",collection="Planes",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Lear 23",collection="Planes",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="P-51 Mustang",collection="Planes",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="SR-71",collection="Planes",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="F4U Corsair",collection="Planes",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Piper J-3 Cub",collection="Planes",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="The Space Shuttle",collection="Planes",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Mitsubishi Zero",collection="Planes",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Messerschmitt Bf 109",collection="Planes",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Boeing's B-29 Superfortress",collection="Planes",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Wright Flyer",collection="Planes",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Northrop Grumman B-2 Spirit",collection="Planes",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="North American XB-70 Valkyrie",collection="Planes",rarity=1,description=""});
                conn.Execute(cmdInsertItem, new{name="SCNCF TGV",collection="Trains",rarity=9,description=""});
                conn.Execute(cmdInsertItem, new{name="Shinkansen",collection="Trains",rarity=1,description=""});
                conn.Execute(cmdInsertItem, new{name="Volkswagen Kombi Splitty",collection="Automobiles",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="DeLorean DMC-12",collection="Automobiles",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Suzuki SJ410",collection="Automobiles",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Bugatti Type 57",collection="Automobiles",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Duesenberg Model J",collection="Automobiles",rarity=15,description=""});
                conn.Execute(cmdInsertItem, new{name="Jaguar XKSS",collection="Automobiles",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="1932 Ford V8",collection="Automobiles",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Pontiac Firebird Trans-Am",collection="Automobiles",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Volvo P1800",collection="Automobiles",rarity=10,description=""});
                conn.Execute(cmdInsertItem, new{name="Toyota AE86",collection="Automobiles",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Porsche Carrera 2.7 RS",collection="Automobiles",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Alfa Romeo Spider",collection="Automobiles",rarity=8,description=""});
                conn.Execute(cmdInsertItem, new{name="Mercedes-Benz G55 AMG",collection="Automobiles",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="Mini Cooper S",collection="Automobiles",rarity=5,description=""});
                conn.Execute(cmdInsertItem, new{name="The Groton",collection="Automobiles",rarity=1,description="AKA Ford Fusion"});

                conn.Execute(cmdInsertCollection, new{name="Planes",cost=50});
                conn.Execute(cmdInsertCollection, new{name="Trains",cost=500});
                conn.Execute(cmdInsertCollection, new{name="Automobiles",cost=50});

                var sqlString = @"update item set collectionID=(select collectionID from collection c where c.Name = item.collection)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if( version < 36)
            {
                var sqlString = @"CREATE TABLE fish(userid INTEGER, fishid INTEGER, weight REAL, length REAL)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if( version < 37)
            {
                var sqlString = @"CREATE TABLE ronkey(keyid INTEGER PRIMARY KEY, userid INTEGER, key STRING, source STRING, used INTEGER )";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if( version < 38)
            {
                var sqlString = @"ALTER TABLE users ADD COLUMN censors integer";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                sqlString = @"UPDATE users set censors =0";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand(string.Format(cmdTextVersionUpdate,++version),conn);
                cmd.ExecuteNonQuery();
            }
            if( version < 39)
            {
                var sqlString = @"CREATE TABLE channelModeration(ChannelId INTEGER PRIMARY KEY, ModerationLevel INTEGER)";
                cmd = new SqliteCommand(sqlString,conn);
                cmd.ExecuteNonQuery();

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
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildWebhooks | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Error
            };
            var interactionConfig = new InteractionServiceConfig(){
                UseCompiledLambda = true,
                WildCardExpression = "*"
            };
            return new ServiceCollection()
                .AddSingleton<ConfigService>()
                .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(clientConfig))
                .AddSingleton<CommandService>(_ => new CommandService(cmdConfig))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(),interactionConfig))
                .AddSingleton<Random>()
                .AddSingleton<FishingService>()
                .AddSingleton<AchievementService>()
                .AddSingleton<AudioService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HangmanService>()
                .AddSingleton<ReactionService>()
                .AddSingleton<BaccaratService>()
                .AddSingleton<CaptchaService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<ImageService>()
                .AddSingleton<KeyService>()
                .AddSingleton<MarkovService>()
                .AddSingleton<StockService>()
                .AddSingleton<BookService>()
                .AddSingleton<GameService>()
                .AddSingleton<WebService>()
                .AddSingleton<RouletteService>()
                .AddSingleton<SlotService>()
                .AddSingleton<RonStockMarketService>()
                .AddSingleton<RonService>()
                .AddSingleton<JellyFinService>()
                .AddSingleton<EconomyService>()
                .AddSingleton<BattleService>()
                .AddSingleton<BlackjackService>()
                .AddSingleton<LancerService>()
                .AddSingleton<AdminService>()
                .BuildServiceProvider();
        }

        private async Task LogAsync(LogMessage log)
        {
            await LoggingService.LogAsync(log.Source,log.Severity,log.Message);
        }
    }
}