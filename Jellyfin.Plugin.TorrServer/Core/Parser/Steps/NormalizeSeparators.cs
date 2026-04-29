using System.Text.RegularExpressions;
#pragma warning disable SA1601

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal partial class NormalizeSeparators : IParsingStep
{
    [GeneratedRegex(@"([^\p{L}\p{N}])\1+")]
    private static partial Regex SameCharacters();

    public void Parse(ParsingContext context)
    {
        context.WorkingName = SameCharacters().Replace(context.WorkingName, "$1");
    }
}
