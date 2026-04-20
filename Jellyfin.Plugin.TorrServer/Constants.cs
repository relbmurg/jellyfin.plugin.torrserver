namespace Jellyfin.Plugin.TorrServer;

/// <summary>
/// Static resources.
/// </summary>
internal static class Constants
{
    public static class TorrServer
    {
        public const string HttpClientName = "torrServerApiClient";
        public const string DefaultBaseUrl = "http://localhost:8090";

        public static class Categories
        {
            public const string Movie = "movie";
            public const string Tv = "tv";
        }
    }
}
