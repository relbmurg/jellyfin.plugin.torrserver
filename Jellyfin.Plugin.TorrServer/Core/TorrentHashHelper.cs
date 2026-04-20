using System;
using System.Collections.Generic;
using System.IO;

namespace Jellyfin.Plugin.TorrServer.Core;

internal static class TorrentHashHelper
{
    public static HashSet<string> GetExistedHashes(IEnumerable<string> roots)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false
        };

        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(root, "*", options))
            {
                if (Path.HasExtension(file))
                {
                    continue;
                }

                var info = new FileInfo(file);
                if (info.Length == 0)
                {
                    result.Add(info.Name);
                }
            }
        }

        return result;
    }
}