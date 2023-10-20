using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public class ICounterData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("default_value")]
    public int? DefaultValue { get; set; }

    [JsonPropertyName("min")]
    public int? MinValue { get; set; }

    [JsonPropertyName("max")]
    public int? MaxValue { get; set; }
}