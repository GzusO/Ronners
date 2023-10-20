using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.Audio;
using System.Collections.Concurrent;
using System;
using System.IO;
using Ronners.Bot.Extensions;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Ronners.Bot.Models.JellyFin;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Ronners.Bot.Models;
using System.ComponentModel;

namespace Ronners.Bot.Services
{
    public class FishingService
    {

        private readonly Random _rand;
        private string FishFile;
        private Dictionary<int, FishData> FishTypes;

        private List<string> ValidFish = new List<string>{":fish:",":tropical_fish:",":blowfish:",":shark:",":shrimp:"};

        public async Task InitializeAsync(string fishFile)
        {
            FishFile = fishFile;
            var json = string.Empty;

            if(!File.Exists(fishFile))
            {
                json = JsonSerializer.Serialize(DefaultFishData(),new JsonSerializerOptions(){WriteIndented =true});
                File.WriteAllText(fishFile,json,new UTF8Encoding(false));
            }

            json = File.ReadAllText(fishFile,new UTF8Encoding(false));
            var fish = JsonSerializer.Deserialize<List<FishData>>(json);

            FishTypes = fish.ToDictionary(x=> x.FishID);
            ValidFish = fish.Select(x=> x.Emoji).ToList();
        }

        private List<FishData> DefaultFishData()
        {
            return new List<FishData>
            {
                new FishData(1, ":fish:", "Fish", 3, 27, .000005337, 3.180),
            };
        }


        public FishingService(Random rand)
        {
            _rand = rand;
        }


        public bool MessageContainsFish(string message)
        {
            return ValidFish.Any(x=> message.Contains(x));
        }

        public List<Fish> CatchFish(string message, out string newMessage)
        {
            newMessage = message;
            var fishList = new List<Fish>();

            foreach (var fish in FishTypes.Values)
            {
                var fishIndex = newMessage.IndexOf(fish.Emoji);
                while(fishIndex != -1)
                {
                    fishList.Add(GenerateFish(fish.FishID));
                    newMessage = newMessage.Remove(fishIndex,fish.Emoji.Length);
                    fishIndex = newMessage.IndexOf(fish.Emoji);
                }
            }

            return fishList;
        }

        private Fish GenerateFish(int fishID)
        {
            var fish = FishTypes[fishID];

            double length = 1;
            int i = fish.LengthRolls;
            while(i>0)
            {
                i--;
                length+= _rand.NextDouble();
            }

            length*=fish.LengthMultiplier;

            double weight = fish.WeightConstant*Math.Pow(length,fish.WeightExponent);

            return new Fish(fishID,Math.Round(weight,2),Math.Round(length,2));

        }

        public string Fishify(string message)
        {
            var builder = new StringBuilder(message);

            var spaces = message.AllIndexesOf(" ");
            var randomSpaceIndex = _rand.Next(spaces.Count());
            var randomFishIndex = _rand.Next(ValidFish.Count);
            builder.Insert(spaces.ElementAt(randomSpaceIndex),ValidFish[randomFishIndex]);

            return builder.ToString();
        }

    }
}