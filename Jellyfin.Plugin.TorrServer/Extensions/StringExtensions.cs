using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
#pragma warning disable SA1601

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static partial class StringExtensions
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static string ToSafeFileName(this string? fileName, char replacement = '_', int maxLength = 255)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "file";
        }

        var source = fileName.AsSpan();
        var sb = new StringBuilder(source.Length);

        foreach (var ch in source)
        {
            var invalid = InvalidChars.Any(t => ch == t);
            sb.Append(invalid ? replacement : ch);
        }

        var result = sb.ToString().TrimEnd('.', ' ');

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindowsReservedName(result))
            {
                result = replacement + result;
            }
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            result = "file";
        }

        if (result.Length <= maxLength)
        {
            return result;
        }

        var ext = Path.GetExtension(result);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(result);

        var allowedNameLength = maxLength - ext.Length;
        if (allowedNameLength <= 0)
        {
            return result[..maxLength];
        }

        result = nameWithoutExt[..allowedNameLength] + ext;

        return result;
    }

    private static bool IsWindowsReservedName(string name)
    {
        ReadOnlySpan<string> reserved =
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        ];

        var baseName = Path.GetFileNameWithoutExtension(name);

        foreach (var r in reserved)
        {
            if (string.Equals(baseName, r, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    [GeneratedRegex(@"^M{0,3}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex RomanRegex();

    public static bool IsRomanNumber(this string input) => !string.IsNullOrWhiteSpace(input) && RomanRegex().IsMatch(input);

    public static string RemoveFirstSegment(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var index = input.IndexOfAny(['/', '\\']);
        return index == -1 ? input : input[(index + 1)..];
    }

    [GeneratedRegex(@"(?<=\d)-(?=\d)")]
    private static partial Regex DashBetweenDigitsRegex();

    public static bool IsDashBetweenDigits(this string input) => DashBetweenDigitsRegex().IsMatch(input);
}
