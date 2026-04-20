using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jellyfin.Extensions.Json.Converters;
using JetBrains.Annotations;
using MediaBrowser.Model.Plugins;
using static Jellyfin.Plugin.TorrServer.Constants.TorrServer;

namespace Jellyfin.Plugin.TorrServer.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
[UsedImplicitly]
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets TorrServer URL.
    /// </summary>
    public string ServerUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Gets or sets TorrServer user.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets TorrServer user password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets movies library ID.
    /// </summary>
    [JsonConverter(typeof(CustomGuidJsonConverter))]
    public Guid MoviesLibraryId { get; set; }

    /// <summary>
    /// Gets or sets movies library location.
    /// </summary>
    public string MoviesLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets TorrServer tv shows library ID.
    /// </summary>
    [JsonConverter(typeof(CustomGuidJsonConverter))]
    public Guid TvShowsLibraryId { get; set; }

    /// <summary>
    /// Gets or sets tv shows library location.
    /// </summary>
    public string TvShowsLocation { get; set; } = string.Empty;

    internal class CustomGuidJsonConverter : JsonGuidConverter
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Guid.TryParse(reader.GetString(), out var result) ? result : Guid.Empty;
    }
}
