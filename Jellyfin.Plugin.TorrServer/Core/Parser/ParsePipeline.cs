using System.Collections.Generic;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal class ParsePipeline : IParsePipeline
{
    private readonly List<PipelineDelegate> _steps = [];

    public IParsePipeline WithStep(PipelineDelegate step)
    {
        _steps.Add(step);
        return this;
    }

    public IParsePipeline WithStep(IParsingStep step)
    {
        _steps.Add(step.Parse);
        return this;
    }

    public ParsingContext Run(ParsingContext context)
    {
        foreach (var step in _steps)
        {
            step(context);
        }

        return context;
    }
}
