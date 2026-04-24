using System;
using System.Collections.Generic;
using System.IO;

namespace Jellyfin.Plugin.TorrServer.Core;

internal static class TorrentHashHelper
{
    public static HashSet<string> GetExistedHashes(IEnumerable<string> roots, bool recursive = true)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false
        };

        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(root, "*.hash", options))
            {
                result.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        return result;
    }

    public static string GetHashFileName(this string hash) => $"{hash}.hash";
}