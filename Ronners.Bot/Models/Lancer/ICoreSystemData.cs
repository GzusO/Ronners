using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;

public class ICoreSystemData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("active_name")]
    public string ActiveName { get; set; }

    [JsonPropertyName("active_effect")]
    public string ActiveEffect { get; set; }

    [JsonPropertyName("activation")]
    public ActivationType Activation { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("deactivation")]
    public ActivationType Deactivation { get; set; }

    [JsonPropertyName("use")]
    public string Use { get; set; }

    [JsonPropertyName("active_actions")]
    public List<IActionData> ActiveActions { get; set; }

    [JsonPropertyName("active_bonuses")]
    public List<IBonusData> ActiveBonuses { get; set; }

    [JsonPropertyName("active_synergies")]
    public List<ISynergyData> ActiveSynergies { get; set; }

    [JsonPropertyName("passive_name")]
    public string PassiveName { get; set; }

    [JsonPropertyName("passive_effect")]
    public string PassiveEffect { get; set; }

    [JsonPropertyName("passive_actions")]
    public List<IActionData> PassiveActions { get; set; }

    [JsonPropertyName("passive_bonuses")]
    public List<IBonusData> PassiveBonuses { get; set; }

    [JsonPropertyName("passive_synergies")]
    public List<ISynergyData> PassiveSynergies { get; set; }

    [JsonPropertyName("deployables")]
    public List<IDeployableData> Deployables { get; set; }

    [JsonPropertyName("counters")]
    public List<ICounterData> Counters { get; set; }

    [JsonPropertyName("integrated")]
    public List<string> Integrated { get; set; }

    [JsonPropertyName("special_equipment")]
    public List<string> SpecialEquipment { get; set; }

    [JsonPropertyName("tags")]
    public List<ITagData> Tags { get; set; }
}
