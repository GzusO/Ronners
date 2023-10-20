using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum ActivationType
    {
        Free,
        Protocol,
        Quick,
        Full,
        Invade,
        FullTech,
        QuickTech,
        Reaction,
        Other
    }

    public class ActivationTypeConverter : JsonConverter<ActivationType>
    {
        public override ActivationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString();
            val = val.Replace(" ","").Replace("/","");
            return Enum.Parse<ActivationType>(val);
        }

        public override void Write(Utf8JsonWriter writer, ActivationType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}