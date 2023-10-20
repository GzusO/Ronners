using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public class IActionData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("activation")]
    public ActivationType Activation { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("cost")]
    public int? Cost { get; set; }

    [JsonPropertyName("pilot")]
    public bool? Pilot { get; set; }

    [JsonPropertyName("synergy_locations")]
    public string[] SynergyLocations { get; set; }

    [JsonPropertyName("tech_attack")]
    public bool? TechAttack { get; set; }

    [JsonPropertyName("log")]
    public string[] Log { get; set; }
}