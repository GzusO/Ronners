using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace Ronners.Bot.Models.Lancer;

public class FrameData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("license_level")]
    public int LicenseLevel { get; set; }

    [JsonPropertyName("license_id")]
    public string LicenseId { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mechtype")]
    public List<string> MechType { get; set; }

    [JsonPropertyName("specialty")]
    public object Specialty { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("mounts")]
    public List<MountType> Mounts { get; set; }

    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    [JsonPropertyName("traits")]
    public List<IFrameTraitData> Traits { get; set; }

    [JsonPropertyName("core_system")]
    public ICoreSystemData CoreSystem { get; set; }

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("y_pos")]
    public double? YPos { get; set; }
}

public class Stats
{
    [JsonPropertyName("size")]
    public decimal Size { get; set; }

    [JsonPropertyName("structure")]
    public int Structure { get; set; }

    [JsonPropertyName("stress")]
    public int Stress { get; set; }

    [JsonPropertyName("armor")]
    public int Armor { get; set; }

    [JsonPropertyName("hp"),JsonConverter(typeof(AutoNumberToStringConverter))]
    public string HP { get; set; }

    [JsonPropertyName("evasion")]
    public int Evasion { get; set; }

    [JsonPropertyName("edef")]
    public int EDef { get; set; }

    [JsonPropertyName("heatcap")]
    public int HeatCap { get; set; }

    [JsonPropertyName("repcap")]
    public int RepCap { get; set; }

    [JsonPropertyName("sensor_range")]
    public int SensorRange { get; set; }

    [JsonPropertyName("tech_attack")]
    public int TechAttack { get; set; }

    [JsonPropertyName("save")]
    public int Save { get; set; }

    [JsonPropertyName("speed")]
    public int Speed { get; set; }

    [JsonPropertyName("sp")]
    public int SP { get; set; }
}

public class AutoNumberToStringConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Number) {
                return reader.TryGetInt64(out long l) ?
                    l.ToString():
                    reader.GetDouble().ToString();
            }
            if(reader.TokenType == JsonTokenType.String) {
                return reader.GetString();
            }
            using(JsonDocument document = JsonDocument.ParseValue(ref reader)){
                return document.RootElement.Clone().ToString();
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }



