namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal interface IParsingStep
{
    void Parse(ParsingContext context);
}
