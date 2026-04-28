namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class Reconstruct : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        context.WorkingName = string.Join(' ', context.Tokens);
    }
}
