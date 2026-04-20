namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal abstract class Parser
{
    public ParseResult Parse(string input)
    {
        var context = CreatePipeline().Run(new ParsingContext(input));

        return new ParseResult
        {
            Year = context.Year,
            VersionTags = context.VersionTags,
            TechnicalTags = context.TechnicalTags,
            Title = context.Title
        };
    }

    protected abstract IParsePipeline CreatePipeline();
}
