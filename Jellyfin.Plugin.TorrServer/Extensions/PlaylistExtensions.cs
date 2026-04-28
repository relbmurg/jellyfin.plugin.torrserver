using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jellyfin.Plugin.TorrServer.Client;

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static class PlaylistExtensions
{
    private static readonly char[] NotAllowedMultipartStartChars = ['.', ' ', '_'];

    public static void AsMovie(this Playlist playlist, string movieTitle)
    {
        var entries = playlist.Entries;
        if (entries.Count == 1)
        {
            return;
        }

        var prefix = entries.Select(x => x.Title).GetPrefix();

        // multipart movie
        foreach (var entry in entries)
        {
            var title = entry.Title.AsSpan(prefix.Length);
            var number = ExtractLeadingNumber(title);
            if (number.Length < 0)
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(title[number.Length..]);
            var ext = Path.GetExtension(title);

            var builder = new StringBuilder(movieTitle);
            builder.Append(name);
            if (NotAllowedMultipartStartChars.Contains(builder[^1]))
            {
                builder.Remove(builder.Length - 1, 1);
            }

            builder.Append(".part ");
            builder.Append(number.Number);
            builder.Append(ext);

            entry.Title = builder.ToString();
            entry.SkipParsing = true;
        }
    }

    private static string GetCommonPrefix(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return string.Empty;
        }

        var minLength = Math.Min(s1.Length, s2.Length);
        var i = 0;

        while (i < minLength && s1[i] == s2[i])
        {
            i++;
        }

        return s1[..i];
    }

    private static string GetPrefix(this IEnumerable<string> input)
    {
        return input.Aggregate(GetCommonPrefix);
    }

    private static NumberString ExtractLeadingNumber(ReadOnlySpan<char> input)
    {
        var i = 0;
        while (i < input.Length && char.IsDigit(input[i]))
        {
            i++;
        }

        return int.TryParse(input[..i], out var n)
            ? new NumberString(n, i)
            : new NumberString(-1, 0);
    }

    private readonly record struct NumberString(int Number, int Length);
}