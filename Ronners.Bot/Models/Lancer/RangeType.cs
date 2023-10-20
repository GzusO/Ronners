using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum RangeType
    {
        Range,
        Threat,
        Blast,
        Burst,
        Line,
        Cone
    }

    public class RangeTypeConverter : JsonConverter<RangeType>
    {
        public override RangeType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString().Replace(" ","").Replace("/","");
            return Enum.Parse<RangeType>(val);
        }

        public override void Write(Utf8JsonWriter writer, RangeType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}