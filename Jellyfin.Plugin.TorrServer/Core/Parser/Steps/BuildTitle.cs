using System;
using System.Globalization;
using System.Linq;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class BuildTitle : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        if (!context.Year.HasValue)
        {
            context.Title = string.Join(" ", context.Tokens).Trim();
            return;
        }

        var year = context.Year.Value.ToString(CultureInfo.InvariantCulture);
        context.Title = string.Join(" ", context.Tokens.TakeWhile(x => !x.Equals(year, StringComparison.OrdinalIgnoreCase)));
    }
}
