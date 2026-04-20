using System.Collections.Generic;

namespace Jellyfin.Plugin.TorrServer.Core;

internal record ItemMetadata(HashSet<string> Hashes, int Version = 1);
