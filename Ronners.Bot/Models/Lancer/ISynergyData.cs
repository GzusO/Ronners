using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public class ISynergyData
{
    [JsonPropertyName("locations")]
    public List<string> Locations { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("weapon_types")]
    public List<WeaponType> WeaponTypes { get; set; }

    [JsonPropertyName("system_types")]
    public List<SystemType> SystemTypes { get; set; }

    [JsonPropertyName("weapon_sizes")]
    public List<WeaponSize> WeaponSizes { get; set; }
}