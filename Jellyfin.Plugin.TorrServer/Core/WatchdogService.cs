using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Client;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TorrServer.Core;

internal sealed class WatchdogService(
    IPluginConfigurationProvider configuration,
    ILibraryManager libraryManager,
    IServiceScopeFactory factory,
    ILogger<WatchdogService> logger) : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private string _metadataKey = string.Empty;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _metadataKey = $"{Plugin.Instance!.Id:N}";

        libraryManager.ItemAdded += LibraryManagerOnItemAdded;
        libraryManager.ItemRemoved += LibraryManagerOnItemRemoved;
        return Task.FromResult(Task.CompletedTask);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        libraryManager.ItemRemoved -= LibraryManagerOnItemRemoved;
        return Task.CompletedTask;
    }

    private async void LibraryManagerOnItemAdded(object? sender, ItemChangeEventArgs e)
    {
        try
        {
            var hashes = TorrentHashHelper.GetExistedHashes([e.Item.ContainingFolderPath]);
            if (hashes.Count == 0)
            {
                return;
            }

            var json = JsonSerializer.Serialize(new ItemMetadata(hashes), JsonOptions);
            e.Item.SetProviderId(_metadataKey, json);
            await libraryManager.UpdateItemAsync(e.Item, e.Parent, ItemUpdateType.MetadataEdit, CancellationToken.None).ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            logger.LogError(exception, "Failed to add TorrServer metadata");
        }
    }

    private async void LibraryManagerOnItemRemoved(object? sender, ItemChangeEventArgs e)
    {
        try
        {
            if (!configuration.Get().RemoveTorrent)
            {
                return;
            }

            if (!e.Item.ProviderIds.TryGetValue(_metadataKey, out var json))
            {
                return;
            }

            var hashes = JsonSerializer.Deserialize<ItemMetadata>(json, JsonOptions)?.Hashes ?? [];
            if (hashes.Count == 0)
            {
                return;
            }

            using var scope = factory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IApiClient>();
            foreach (var hash in hashes)
            {
                if (!await client.Remove(hash, CancellationToken.None).ConfigureAwait(false))
                {
                    logger.LogWarning("Failed to remove torrent with {Hash}", hash);
                }
            }

            logger.LogInformation("Item removed from TorrServer: {Item}", e.Item.Name);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            logger.LogError(exception, "Failed to remove torrent");
        }
    }
}