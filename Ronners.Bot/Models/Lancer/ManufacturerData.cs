using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;

public class ManufacturerData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("logo")]
    public string Logo { get; set; }
    
    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; }
    
    [JsonPropertyName("light")]
    public string Light { get; set; }
    
    [JsonPropertyName("dark")]
    public string Dark { get; set; }
    
    [JsonPropertyName("quote")]
    public string Quote { get; set; }
}