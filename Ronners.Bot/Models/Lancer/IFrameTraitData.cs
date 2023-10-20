using System.Text.Json.Serialization;
using System.Collections.Generic;
namespace Ronners.Bot.Models.Lancer;

public class IFrameTraitData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("use")]
    public string Use { get; set; }

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