using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public class IDeployableData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; }

    [JsonPropertyName("activation")]
    public ActivationType? Activation { get; set; }

    [JsonPropertyName("deactivation")]
    public ActivationType? Deactivation { get; set; }

    [JsonPropertyName("recall")]
    public ActivationType? Recall { get; set; }

    [JsonPropertyName("redeploy")]
    public ActivationType? Redeploy { get; set; }

    [JsonPropertyName("instances")]
    public int? Instances { get; set; }

    [JsonPropertyName("cost")]
    public int? Cost { get; set; }

    [JsonPropertyName("armor")]
    public int? Armor { get; set; }

    [JsonPropertyName("hp"),JsonConverter(typeof(AutoNumberToStringConverter))]
    public string? HP { get; set; }

    [JsonPropertyName("evasion")]
    public int? Evasion { get; set; }

    [JsonPropertyName("edef")]
    public int? EDef { get; set; }

    [JsonPropertyName("heatcap")]
    public int? HeatCap { get; set; }

    [JsonPropertyName("repcap")]
    public int? RepCap { get; set; }

    [JsonPropertyName("sensor_range")]
    public int? SensorRange { get; set; }

    [JsonPropertyName("tech_attack")]
    public int? TechAttack { get; set; }

    [JsonPropertyName("save")]
    public int? Save { get; set; }

    [JsonPropertyName("speed")]
    public int? Speed { get; set; }

    [JsonPropertyName("pilot")]
    public bool? Pilot { get; set; }

    [JsonPropertyName("mech")]
    public bool? Mech { get; set; }

    [JsonPropertyName("actions")]
    public List<IActionData> Actions { get; set; }

    [JsonPropertyName("bonuses")]
    public List<IBonusData> Bonuses { get; set; }

    [JsonPropertyName("synergies")]
    public List<ISynergyData> Synergies { get; set; }

    [JsonPropertyName("counters")]
    public List<ICounterData> Counters { get; set; }

    [JsonPropertyName("tags")]
    public List<ITagData> Tags { get; set; }
}