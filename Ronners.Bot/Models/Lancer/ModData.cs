using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;

public class ModData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("source")]
    public string Source { get; set; }
    
    [JsonPropertyName("license")]
    public string License { get; set; }
    
    [JsonPropertyName("license_id")]
    public string LicenseId { get; set; }
    
    [JsonPropertyName("license_level")]
    public int LicenseLevel { get; set; }
    
    [JsonPropertyName("sp")]
    public int? Sp { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("effect")]
    public string Effect { get; set; }
    
    [JsonPropertyName("tags")]
    public List<ITagData> Tags { get; set; }
    
    [JsonPropertyName("allowed_types")]
    public List<WeaponType> AllowedTypes { get; set; }
    
    [JsonPropertyName("allowed_sizes")]
    public List<WeaponSize> AllowedSizes { get; set; }
    
    [JsonPropertyName("restricted_types")]
    public List<WeaponType> RestrictedTypes { get; set; }
    
    [JsonPropertyName("restricted_sizes")]
    public List<WeaponSize> RestrictedSizes { get; set; }
    
    [JsonPropertyName("added_tags")]
    public List<ITagData> AddedTags { get; set; }
    
    [JsonPropertyName("added_damage")]
    public List<IDamageData> AddedDamage { get; set; }
    
    [JsonPropertyName("added_range")]
    public List<IRangeData> AddedRange { get; set; }
    
    [JsonPropertyName("actions")]
    public List<IActionData> Actions { get; set; }
    
    [JsonPropertyName("bonuses")]
    public List<IBonusData> Bonuses { get; set; }
    
    [JsonPropertyName("synergies")]
    public List<ISynergyData> Synergies { get; set; }
    
    [JsonPropertyName("deployables")]
    public List<IDeployableData> Deployables { get; set; }
    
    [JsonPropertyName("counters")]
    public List<ICounterData> Counters { get; set; }
    
    [JsonPropertyName("integrated")]
    public List<string> Integrated { get; set; }
    
    [JsonPropertyName("special_equipment")]
    public List<string> SpecialEquipment { get; set; }
}