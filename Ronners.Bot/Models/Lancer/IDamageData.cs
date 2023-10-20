using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public class IDamageData
    {
        [JsonPropertyName("type")]
        public DamageType Type{get;set;}
        [JsonPropertyName("val")]
        public string Value{get;set;}
    }
}