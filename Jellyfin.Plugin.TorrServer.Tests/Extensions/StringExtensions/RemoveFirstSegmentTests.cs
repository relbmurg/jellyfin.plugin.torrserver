using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Extensions.StringExtensions;

public class RemoveFirstSegmentTests
{
    [Test]
    public async Task Multiple_Segments_Exists()
    {
        var input = "segment 1/segment 2/tail segment";

        var result = input.RemoveFirstSegment();

        await Assert.That(result).EqualTo("segment 2/tail segment");
    }

    [Test]
    public async Task Multiple_Segments_Back_Slash_Exists()
    {
        var input = "segment 1\\segment 2\\tail segment";

        var result = input.RemoveFirstSegment();

        await Assert.That(result).EqualTo("segment 2\\tail segment");
    }

    [Test]
    public async Task Empty_String()
    {
        var input = string.Empty;

        var result = input.RemoveFirstSegment();

        await Assert.That(result).EqualTo(input);
    }

    [Test]
    public async Task No_Segments()
    {
        var input = "test test test. test";

        var result = input.RemoveFirstSegment();

        await Assert.That(result).EqualTo(input);
    }
}