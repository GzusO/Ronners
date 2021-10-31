

using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Ronners.Bot.Services;
using System.Collections.Generic;
using Ronners.Bot.Models;

namespace Ronners.Bot
{
    public class ConfigService
    {
        public static readonly string ConfigPath = "config.json";
        public static BotConfig Config{get;set;}

        public async Task InitializeAsync()
        {
            var json = string.Empty;

            if(!File.Exists(ConfigPath))
            {
                json = JsonSerializer.Serialize(GenerateBaseConfig(),new JsonSerializerOptions(){WriteIndented =true});
                File.WriteAllText(ConfigPath,json,new UTF8Encoding(false));
                await LoggingService.LogAsync("bot",Discord.LogSeverity.Warning,"Creating default Config.json, please configure and then restart Ronners.");
                await Task.Delay(-1);
            }

            json = File.ReadAllText(ConfigPath,new UTF8Encoding(false));
            Config = JsonSerializer.Deserialize<BotConfig>(json);
        }

        private static BotConfig GenerateBaseConfig() => new BotConfig
        {
            DiscordKey = "Discord KEy Here",
            IEXKey ="INSERT KEY HERE",
            CaptchaFolder= "Captcha",
            ImgFolder = "Img",
            AudioFolder ="Audio",
            CommandPrefix = '!',
            WhitelistedChannel = new List<ulong>(){1234,5678},
            DatabaseConnection ="Data Source=sample.db;",
            StockJson = "ronStock.json"
        };

    }
}