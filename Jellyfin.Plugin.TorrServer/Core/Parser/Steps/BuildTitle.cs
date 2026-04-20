namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class BuildTitle : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        context.Title = string.Join(" ", context.Tokens).Trim();
    }
}
