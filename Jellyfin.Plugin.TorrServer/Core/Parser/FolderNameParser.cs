using System;
using System.Globalization;
using System.Linq;
using Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal sealed class FolderNameParser : Parser
{
    private char[] Delimiters { get; } = ['.', '_', ' '];

    private char[] TranslatedNameDelimiters { get; } = ['/', '(', '['];

    protected override IParsePipeline CreatePipeline()
    {
        return new ParsePipeline()
            .WithStep(new RemoveExtension())
            .WithStep(new RemoveLeadingBracket())
            .WithStep(new NormalizeSeparators())
            .WithStep(new ParseYear(true))
            .WithStep(TrimTitleInfo)
            .WithStep(Tokenize)
            .WithStep(new ExtractTechnicalTags())
            .WithStep(new ExtractVersionTags())
            .WithStep(new BuildTitle())
            .WithStep(new SanitizeTitle([':'], '.'));
    }

    private void TrimTitleInfo(ParsingContext context)
    {
        var input = context.WorkingName.Split(TranslatedNameDelimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        var index = ExtractTechnicalTags.TechnicalSet
            .Select(tag => input.IndexOf(tag, StringComparison.OrdinalIgnoreCase))
            .Where(x => x != -1)
            .DefaultIfEmpty(-1)
            .Min(x => x);

        input = index >= 0 ? input[..index] : input;

        context.WorkingName = input;
    }

    private void Tokenize(ParsingContext context)
    {
        if (string.IsNullOrEmpty(context.WorkingName))
        {
            return;
        }

        var split = context.WorkingName.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
        context.Tokens.AddRange(split);
    }
}
