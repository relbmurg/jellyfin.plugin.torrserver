using Jellyfin.Plugin.TorrServer.Client;

namespace Jellyfin.Plugin.TorrServer.Infrastructure;

/// <summary>
/// TorrServerClientFactory.
/// </summary>
internal interface ITorrServerClientFactory
{
    /// <summary>
    /// Create instance of ApiClient.
    /// </summary>
    /// <returns>Instance of the <see cref="ApiClient"/>.</returns>
    IApiClient GetClient();
}