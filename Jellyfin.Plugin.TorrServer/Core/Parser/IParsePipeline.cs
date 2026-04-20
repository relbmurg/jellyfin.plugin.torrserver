namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal delegate void PipelineDelegate(ParsingContext context);

internal interface IParsePipeline
{
    IParsePipeline WithStep(PipelineDelegate step);

    IParsePipeline WithStep(IParsingStep step);

    ParsingContext Run(ParsingContext context);
}
