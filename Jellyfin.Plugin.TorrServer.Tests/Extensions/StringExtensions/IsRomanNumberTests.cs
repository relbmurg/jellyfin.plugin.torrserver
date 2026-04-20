using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Extensions.StringExtensions;

public class IsRomanNumberTests
{
    [Test]
    [Arguments("I")]
    [Arguments("III")]
    [Arguments("iii")]
    [Arguments("IV")]
    [Arguments("IX")]
    [Arguments("XIV")]
    [Arguments("XL")]
    [Arguments("XC")]
    [Arguments("CD")]
    [Arguments("CM")]
    [Arguments("MMXXIV")]
    [Arguments("MMMCMXCIX")] // 3999
    public async Task Valid_roman_numbers_return_true(string value)
    {
        await Assert.That(value.IsRomanNumber()).IsTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments(null)]
    [Arguments("IIII")]
    [Arguments("VV")]
    [Arguments("IC")]
    [Arguments("XM")]
    [Arguments("VX")]
    [Arguments("ABC")]
    [Arguments("123")]
    public async Task Invalid_roman_numbers_return_false(string? value)
    {
        await Assert.That(value!.IsRomanNumber()).IsFalse();
    }
}
