using System.Globalization;
using System.Linq;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class ExtractYear() : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        if (context.Tokens.Count == 0)
        {
            return;
        }

        foreach (var token in context.Tokens.Reverse<string>())
        {
            if (int.TryParse(token, NumberStyles.Integer, new NumberFormatInfo(), out var year) && year is >= 1900 and <= 2100)
            {
                context.Year = year;
                return;
            }
        }
    }
}