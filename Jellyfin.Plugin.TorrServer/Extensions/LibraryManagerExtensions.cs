using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static class LibraryManagerExtensions
{
    public static string[] GetLibraryPaths(this ILibraryManager manager, Guid libraryId)
    {
        if (libraryId == Guid.Empty || manager.GetItemById(libraryId) is not CollectionFolder library)
        {
            return [];
        }

        return library
            .GetLibraryOptions()
            .PathInfos
            .Select(x => x.Path)
            .ToArray();
    }

    public static string[] GetLibrariesPaths(this ILibraryManager manager) =>
        manager.GetVirtualFolders()
            .Where(x => x.CollectionType is CollectionTypeOptions.movies or CollectionTypeOptions.tvshows)
            .SelectMany(x => x.Locations)
            .ToArray();
}
