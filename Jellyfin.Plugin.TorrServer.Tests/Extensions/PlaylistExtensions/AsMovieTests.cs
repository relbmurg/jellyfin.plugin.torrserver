using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Extensions.PlaylistExtensions;

public class AsMovieTests
{
    [Test]
    public async Task AsMovie_Single_Entry()
    {
        var playlist = await Playlist.Parse("""
                                            #EXTM3U
                                            #EXTINF:0,Title.1.mkv
                                            http://localhost:8090/stream/Title.1.mkv?link=54b8b13e-bb01-4e8c-8181-95ce9c8b730c&index=1&play
                                            """, CancellationToken.None);
        
        playlist.AsMovie("skipped");

        await Assert.That(playlist.Entries).IsEquivalentTo(playlist.Entries);
    }

    [Test]
    public async Task AsMovie_Multipart_With_Prefix()
    {
        var playlist = await Playlist.Parse("""
                                            #EXTM3U
                                            #EXTINF:0,Prefix_1.avi
                                            http://localhost:8090/stream/Prefix_1.avi?link=1a913a7f-8d39-4a95-9865-41c281257905&index=1&play
                                            #EXTINF:0,Prefix_02.avi
                                            http://localhost:8090/stream/Prefix_02.avi?link=b16c5053-87d5-405b-a71f-d1cf8326ceb0&index=1&play
                                            #EXTINF:0,Prefix_3.avi
                                            http://localhost:8090/stream/Prefix_3.avi?link=c287564f-fd7d-4bd5-b1bb-4a772c385452&index=3&play
                                            """, CancellationToken.None);

        playlist.AsMovie("Title");

        await Assert.That(playlist.Entries.Count).IsEqualTo(3);
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 1.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 2.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 3.avi"));
    }

    [Test]
    public async Task AsMovie_Multipart_With_Prefix_And_Suffix()
    {
        var playlist = await Playlist.Parse("""
                                            #EXTM3U
                                            #EXTINF:0,Prefix_1.avi
                                            http://localhost:8090/stream/Prefix_1.avi?link=1a913a7f-8d39-4a95-9865-41c281257905&index=1&play
                                            #EXTINF:0,Prefix_02.avi
                                            http://localhost:8090/stream/Prefix_02.avi?link=b16c5053-87d5-405b-a71f-d1cf8326ceb0&index=1&play
                                            #EXTINF:0,Prefix_3.avi
                                            http://localhost:8090/stream/Prefix_3.avi?link=c287564f-fd7d-4bd5-b1bb-4a772c385452&index=3&play
                                            """, CancellationToken.None);

        playlist.AsMovie("Title");

        await Assert.That(playlist.Entries.Count).IsEqualTo(3);
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 1.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 2.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 3.avi"));
    }

    [Test]
    public async Task AsMovie_Multipart_With_Number_Prefix()
    {
        var playlist = await Playlist.Parse("""
                                            #EXTM3U
                                            #EXTINF:0,2012.1.avi
                                            http://localhost:8090/stream/2012.1.avi?link=1a913a7f-8d39-4a95-9865-41c281257905&index=1&play
                                            #EXTINF:0,2012.2.avi
                                            http://localhost:8090/stream/2012.2.avi?link=b16c5053-87d5-405b-a71f-d1cf8326ceb0&index=1&play
                                            """, CancellationToken.None);

        playlist.AsMovie("Title");

        await Assert.That(playlist.Entries.Count).IsEqualTo(2);
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 1.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 2.avi"));
    }

    [Test]
    public async Task AsMovie_Multipart_Without_Prefix()
    {
        var playlist = await Playlist.Parse("""
                                            #EXTM3U
                                            #EXTINF:0,1.avi
                                            http://localhost:8090/stream/1.avi?link=1a913a7f-8d39-4a95-9865-41c281257905&index=1&play
                                            #EXTINF:0,2.avi
                                            http://localhost:8090/stream/2.avi?link=b16c5053-87d5-405b-a71f-d1cf8326ceb0&index=1&play
                                            """, CancellationToken.None);

        playlist.AsMovie("Title");

        await Assert.That(playlist.Entries.Count).IsEqualTo(2);
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 1.avi"));
        await Assert.That(playlist.Entries).Contains(x => x.Title.Equals("Title.part 2.avi"));
    }
}
