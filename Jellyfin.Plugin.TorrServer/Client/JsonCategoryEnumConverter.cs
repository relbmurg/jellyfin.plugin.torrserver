using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Jellyfin.Plugin.TorrServer.Constants.TorrServer.Categories;

namespace Jellyfin.Plugin.TorrServer.Client;

internal sealed class JsonCategoryEnumConverter : JsonConverter<Category>
{
    public override Category Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var category = reader.GetString()!;
        if (string.IsNullOrWhiteSpace(category))
        {
            return Category.Empty;
        }

        return Enum.TryParse<Category>(category, true, out var result) ? result : Category.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, Category value, JsonSerializerOptions options)
    {
        var result = value switch
        {
            Category.Movie => Movie,
            Category.Tv => Tv,
            _ => string.Empty
        };
        writer.WriteStringValue(result);
    }
}
