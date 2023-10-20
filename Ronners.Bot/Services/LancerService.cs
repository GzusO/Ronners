using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ronners.Bot.Models;
using Ronners.Bot.Models.Lancer;

namespace Ronners.Bot.Services
{
    public class LancerService
    {
        public IEnumerable<FrameData> Frames{get;set;}
        public string LCPDirectory{get;set;}
        public JsonSerializerOptions options {get;set;}

        public async Task InitializeAsync(string directory)
        {
            LCPDirectory = Path.Combine(Directory.GetCurrentDirectory(),directory);

            RegisterCustomConverters();

            Frames = LoadFrames(LCPDirectory);
        }

        private void RegisterCustomConverters()
        {
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new ActivationTypeConverter(),
                    new MountTypeConverter(),
                    new SystemTypeConverter(),
                    new WeaponSizeConverter(),
                    new WeaponTypeConverter(),
                    new DamageTypeConverter(),
                    new RangeTypeConverter()
                },
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
        }

        private IEnumerable<FrameData> LoadFrames(string lCPDirectory)
        {
            var json = string.Empty;
            var file = Path.Combine(lCPDirectory,"frames.json");

            if(!File.Exists(file))
            {
                return new List<FrameData>();
            }

            json = File.ReadAllText(file,new UTF8Encoding(false));
            return JsonSerializer.Deserialize<IEnumerable<FrameData>>(json,options);
        }
    }
}