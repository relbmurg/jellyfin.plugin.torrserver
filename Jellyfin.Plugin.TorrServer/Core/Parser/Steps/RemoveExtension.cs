namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class RemoveExtension : IParsingStep
{
    private const int MaxExtensionLength = 4;

    public void Parse(ParsingContext context)
    {
        var index = context.WorkingName.LastIndexOf('.');
        var length = context.WorkingName.Length - 1 - index;
        if (length <= MaxExtensionLength)
        {
            context.WorkingName = context.WorkingName.Remove(index, length + 1);
        }
    }
}
