using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum DamageType
    {
        Kinetic,
        Explosive,
        Energy,
        Burn,
        Variable
    }

    public class DamageTypeConverter : JsonConverter<DamageType>
    {
        public override DamageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString().Replace(" ","").Replace("/","");
            return Enum.Parse<DamageType>(val);
        }

        public override void Write(Utf8JsonWriter writer, DamageType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}