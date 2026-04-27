using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TorrServer.Client;

internal class ApiClient(HttpClient client, ILogger<ApiClient> logger) : IApiClient
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TorrentItem[]> List(CancellationToken cancellation)
    {
        using var content = JsonContent.Create(new { action = "list" }, options: Options);

        try
        {
            var response = await client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<TorrentItem[]>(cancellation).ConfigureAwait(false);

            return data ?? [];
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get torrents list");
        }

        return [];
    }

    public async Task<Playlist> GetPlaylist(string hash, CancellationToken cancellation)
    {
        try
        {
            var response = await client.GetAsync($"/playlist?hash={hash}", cancellation).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var playlist = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
            return await Playlist.Parse(playlist, cancellation).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get torrent");
        }

        /*
        using var content = JsonContent.Create(new { action = "get", hash = hash }, options: Options);

        const int maxAttempts = 2;
        var delay = TimeSpan.FromSeconds(3);
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var torrent = await response.Content.ReadFromJsonAsync<TorrentItem>(cancellation).ConfigureAwait(false);
                if (torrent?.Files.Length == 0)
                {
                    await Task.Delay(delay, cancellation).ConfigureAwait(false);
                    continue;
                }

                return Playlist.Create(client.BaseAddress!, torrent ?? new TorrentItem());
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to get torrent");
                await Task.Delay(delay, cancellation).ConfigureAwait(false);
            }
        }
        */

        return Playlist.Empty;
    }

    public async Task<bool> Remove(string hash, Category category, CancellationToken cancellation)
    {
        try
        {
            using (var content = JsonContent.Create(new { action = "get", hash = hash }, options: Options))
            {
                var response = await client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var torrent = await response.Content.ReadFromJsonAsync<TorrentItem>(cancellation).ConfigureAwait(false);
                if (torrent == null)
                {
                    logger.LogInformation("Torrent [{Hash}] not found", hash);
                    return false;
                }

                if (torrent.Category != category)
                {
                    logger.LogWarning("The torrent's category has changed. Required {CategoryRequired}, actually {CategoryActually}", category, torrent.Category);
                    return false;
                }
            }

            using (var content = JsonContent.Create(new { action = "rem", hash = hash }, options: Options))
            {
                var response = await client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to remove torrent");
            return false;
        }
    }

    public async Task<ServerConfig> GetConfiguration(CancellationToken cancellation)
    {
        using var content = JsonContent.Create(new { action = "get" }, options: Options);
        ServerConfig? settings = null;
        try
        {
            var response = await client.PostAsync("/settings", content, cancellation).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            settings = await response.Content.ReadFromJsonAsync<ServerConfig>(cancellation).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get server settings - using defaults");
        }

        return settings ?? new ServerConfig();
    }
}
