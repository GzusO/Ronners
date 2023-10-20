using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer;
public enum MountType
{
    Main,
    Heavy,
    AuxAux,
    Aux,
    MainAux,
    Flex,
    Integrated
}

public class MountTypeConverter : JsonConverter<MountType>
{
    public override MountType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var val = reader.GetString();
        val = val.Replace(" ","").Replace("/","");
        return Enum.Parse<MountType>(val);
    }

    public override void Write(Utf8JsonWriter writer, MountType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}