using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Jellyfin.Plugin.TorrServer.Constants.TorrServer;

namespace Jellyfin.Plugin.TorrServer.Client;

internal class ApiClient(IHttpClientFactory factory, IPluginConfigurationProvider config) : IApiClient
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private HttpClient Client
    {
        get
        {
            var cfg = config.Get();
            var client = factory.CreateClient(HttpClientName);
            client.BaseAddress = Uri.TryCreate(cfg.ServerUrl, UriKind.Absolute, out var uri)
                ? uri
                : new Uri(DefaultBaseUrl);

            if (!string.IsNullOrWhiteSpace(cfg.Username))
            {
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{cfg.Username}:{cfg.Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            }

            return client;
        }
    }

    public async Task<TorrentItem[]> List(CancellationToken cancellation)
    {
        using var content = JsonContent.Create(new { action = "list" }, options: _options);

        var response = await Client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<TorrentItem[]>(cancellation).ConfigureAwait(false);

        return data ?? [];
    }

    public async Task<Playlist> GetPlaylist(string hash, CancellationToken cancellation)
    {
        /*
        var response = await Client.GetAsync($"/playlist?hash={hash}", cancellation).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var playlist = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
        var result = await Playlist.Parse(playlist, cancellation).ConfigureAwait(false);
        */

        using var content = JsonContent.Create(new { action = "get", hash = hash }, options: _options);

        var response = await Client.PostAsync("/torrents", content, cancellation).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var torrent = await response.Content.ReadFromJsonAsync<TorrentItem>(cancellation).ConfigureAwait(false);

        return Playlist.Create(Client.BaseAddress!, torrent ?? new TorrentItem());
    }
}
