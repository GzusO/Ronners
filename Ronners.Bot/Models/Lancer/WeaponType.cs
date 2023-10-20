using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum WeaponType
    {
        Rifle,
        Cannon,
        Launcher,
        CQB,
        Nexus,
        Melee
    }

    public class WeaponTypeConverter : JsonConverter<WeaponType>
    {
        public override WeaponType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString().Replace(" ","").Replace("/","");
            return Enum.Parse<WeaponType>(val);
        }

        public override void Write(Utf8JsonWriter writer, WeaponType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}