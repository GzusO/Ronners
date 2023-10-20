using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public enum WeaponSize
    {
        Aux,
        Main,
        Heavy,
        SuperHeavy,
        any
    }

    public class WeaponSizeConverter : JsonConverter<WeaponSize>
    {
        public override WeaponSize Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = reader.GetString().Replace(" ","").Replace("/","");
            return Enum.Parse<WeaponSize>(val);
        }

        public override void Write(Utf8JsonWriter writer, WeaponSize value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}