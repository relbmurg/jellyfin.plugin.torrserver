namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal abstract class Parser
{
    public ParseResult Parse(string input)
    {
        var context = CreatePipeline().Run(new ParsingContext(input));

        return new ParseResult(context);
    }

    protected abstract IParsePipeline CreatePipeline();
}
