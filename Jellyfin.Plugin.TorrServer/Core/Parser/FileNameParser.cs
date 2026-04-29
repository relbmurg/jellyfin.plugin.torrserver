using System;
using System.Collections.Generic;
using Jellyfin.Plugin.TorrServer.Core.Parser.Steps;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal sealed class FileNameParser : Parser
{
    private List<char> Delimiters { get; } = [' ', '_', '[', ']', '(', ')', '.'];

    protected override IParsePipeline CreatePipeline()
    {
        return new ParsePipeline()
            .WithStep(new RemoveExtension())
            .WithStep(new RemoveLeadingBracket())
            .WithStep(new NormalizeSeparators())
            .WithStep(Tokenize)
            .WithStep(new ExtractTechnicalTags())
            .WithStep(new ExtractVersionTags())
            .WithStep(new ExtractEpisodeTag())
            .WithStep(new ExtractYear())
            .WithStep(new RemoveReleaseGroup())
            .WithStep(new BuildTitle())
            .WithStep(new SanitizeTitle([':'], '.'));
    }

    private void Tokenize(ParsingContext context)
    {
        if (string.IsNullOrEmpty(context.WorkingName))
        {
            return;
        }

        var split = context.WorkingName.Split(Delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in split)
        {
            if (token.Contains('-', StringComparison.OrdinalIgnoreCase)
                && !ExtractTechnicalTags.TechnicalSet.Contains(token)
                && !token.IsDashBetweenDigits())
            {
                context.Tokens.AddRange(token.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                continue;
            }

            context.Tokens.Add(token);
        }
    }
}
