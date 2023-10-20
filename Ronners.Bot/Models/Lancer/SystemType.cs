using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum SystemType
    {
        AI,
        Deployable,
        Drone,
        FlightSystem,
        Shield,
        System,
        Tech
    }

    public class SystemTypeConverter : JsonConverter<SystemType>
    {
        public override SystemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString().Replace(" ","").Replace("/","");
            return Enum.Parse<SystemType>(val);
        }

        public override void Write(Utf8JsonWriter writer, SystemType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}