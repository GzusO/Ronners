using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public class IBonusData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("val")]
    public object Value { get; set; }

    [JsonPropertyName("damage_types")]
    public List<DamageType> DamageTypes { get; set; }

    [JsonPropertyName("range_types")]
    public List<RangeType> RangeTypes { get; set; }

    [JsonPropertyName("weapon_types")]
    public WeaponType[] WeaponTypes { get; set; }

    [JsonPropertyName("weapon_sizes")]
    public WeaponSize[] WeaponSizes { get; set; }

    [JsonPropertyName("overwrite")]
    public bool Overwrite { get; set; }

    [JsonPropertyName("replace")]
    public bool Replace { get; set; }
}