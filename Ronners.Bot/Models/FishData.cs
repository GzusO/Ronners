using Dapper.Contrib.Extensions;
using Discord;
using System.Text.Json.Serialization;


namespace Ronners.Bot.Models
{
    public class FishData
    {
        public int FishID{get;set;}
        public string Emoji {get;set;}
        public string Name {get;set;}
        public int LengthRolls {get;set;}
        public int LengthMultiplier{get;set;}
        public double WeightConstant {get;set;}
        public double WeightExponent {get;set;}

        public FishData()
        {
            //Required By Dapper
        }

        public FishData(int fishID, string emoji, string name, int lengthRolls, int lengthMultiplier, double weightConstant, double weightExponent)
        {
            FishID = fishID;
            Emoji = emoji;
            Name = name;
            LengthRolls = lengthRolls;
            LengthMultiplier = lengthMultiplier;
            WeightConstant = weightConstant;
            WeightExponent = weightExponent;
        }


    }
}

