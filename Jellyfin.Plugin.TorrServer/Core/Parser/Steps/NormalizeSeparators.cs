using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class NormalizeSeparators : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        context.WorkingName = Regex.Replace(context.WorkingName, @"([^\p{L}\p{N}])\1+", "$1");
    }
}
