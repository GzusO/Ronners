using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Net.Http;
using System.IO;
using Ronners.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Ronners.Loot;
using Dapper;

namespace Ronners.Bot
{
    class Program
    {
        private string dbFileName ="ronners.db";
        private DiscordSocketClient _discord;
        private GameService gameService;

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
                services.GetRequiredService<CommandService>().Log += LogAsync;
                services.GetRequiredService<GameService>().connection = connection;
                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await _discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
                await _discord.StartAsync();
                
                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                await services.GetRequiredService<HangmanService>().InitializeAsync();
                await services.GetRequiredService<ReactionService>().InitializeAsync();
                await services.GetRequiredService<CaptchaService>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task ReadyAsync()
        {
            await _discord.SetGameAsync("Ronners!");
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
            cmd = new SqliteCommand(String.Format(cmdTextVersionUpdate,version),conn);
            cmd.ExecuteNonQuery();
        }

        private ServiceProvider ConfigureServices()
        {
            var cmdConfig = new CommandServiceConfig(){
                IgnoreExtraArgs = true
                };
            var clientConfig = new DiscordSocketConfig(){
                MessageCacheSize = 1000
            };
            return new ServiceCollection()
                .AddSingleton<ConfigService>()
                .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(clientConfig))
                .AddSingleton<CommandService>(_ => new CommandService(cmdConfig))
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
                .AddSingleton<StockService>()
                .AddSingleton<GameService>()
                .AddSingleton<WebService>()
                .AddSingleton<RouletteService>()
                .AddSingleton<SlotService>()
                .AddSingleton<LootGenerator>(_ => new LootGenerator("TestData/"))
                .BuildServiceProvider();
        }

        private async Task LogAsync(LogMessage log)
        {
            await LoggingService.LogAsync(log.Source,log.Severity,log.Message);
        }
    }
}