using System.Runtime.InteropServices;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Extensions.StringExtensions;

public class ToSafeFileNameTests
{
    [Test]
    public async Task Should_Replace_Invalid_Characters()
    {
        var input = "file<>:\"/\\|?*.txt";

        var result = input.ToSafeFileName('_');

        await Assert.That(result).DoesNotContain('<');
        await Assert.That(result).DoesNotContain('>');
        await Assert.That(result).DoesNotContain(':');
        await Assert.That(result).DoesNotContain('"');
        await Assert.That(result).DoesNotContain('/');
        await Assert.That(result).DoesNotContain('\\');
        await Assert.That(result).DoesNotContain('|');
        await Assert.That(result).DoesNotContain('?');
        await Assert.That(result).DoesNotContain('*');
    }

    [Test]
    public async Task Should_Trim_Ending_Dot_And_Space()
    {
        var input = "file.txt. ";

        var result = input.ToSafeFileName();

        await Assert.That(result.EndsWith(".")).IsFalse();
        await Assert.That(result.EndsWith(" ")).IsFalse();
    }

    [Test]
    public async Task Should_Handle_Null_Or_Empty()
    {
        string? input = null;

        var result = input.ToSafeFileName();

        await Assert.That(result).IsEqualTo("file");
    }

    [Test]
    public async Task Should_Limit_Length_And_Preserve_Extension()
    {
        var longName = new string('a', 300) + ".txt";

        var result = longName.ToSafeFileName(maxLength: 255);

        await Assert.That(result.Length).IsLessThanOrEqualTo(255);
        await Assert.That(Path.GetExtension(result)).IsEqualTo(".txt");
    }

    [Test]
    public async Task Should_Replace_Windows_Reserved_Name()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        var input = "CON.txt";

        var result = input.ToSafeFileName();

        await Assert.That(result.StartsWith("_")).IsTrue();
    }

    [Test]
    public async Task Should_Not_Modify_Valid_Name()
    {
        var input = "normal-file_name.txt";

        var result = input.ToSafeFileName();

        await Assert.That(result).IsEqualTo(input);
    }
}
