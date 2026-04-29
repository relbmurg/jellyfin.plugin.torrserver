using System.Globalization;
using System.Text.RegularExpressions;
#pragma warning disable SA1601

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal partial class ParseYear(bool trim) : IParsingStep
{
    // (?<!-\s*) - Негативный lookbehind: перед годом НЕ должно быть дефиса (с возможными пробелами)
    // \b(19|20)\d{2}\b - Сам год (1900-2099)
    // (?!\s*-) - Негативный lookahead: после года НЕ должно быть дефиса (с возможными пробелами)
    [GeneratedRegex(@"(?<!-\s*)\b(19|20)\d{2}\b(?!\s*-)")]
    private static partial Regex YearRegex();

    public void Parse(ParsingContext context)
    {
        if (string.IsNullOrEmpty(context.WorkingName))
        {
            return;
        }

        var input = context.WorkingName;

        // Находим все совпадения
        var matches = YearRegex().Matches(input);

        if (matches.Count == 0)
        {
            return;
        }

        // Берем последнее совпадение
        var lastMatch = matches[^1];
        var extractedYear = lastMatch.Value;

        // Определяем границы удаления
        var removeStartIndex = lastMatch.Index;
        var removeLength = lastMatch.Length;

        // Проверяем наличие скобок непосредственно вокруг года
        // Индекс открывающей скобки должен быть > 0
        // Индекс закрывающей скобки должен быть < длины строки
        var hasOpenParen = removeStartIndex > 0 && input[removeStartIndex - 1] == '(';
        var hasCloseParen = removeStartIndex + removeLength < input.Length && input[removeStartIndex + removeLength] == ')';

        if (hasOpenParen && hasCloseParen)
        {
            // Если год в скобках, расширяем диапазон удаления на 2 символа (сами скобки)
            removeStartIndex -= 1;
            removeLength += 2;
        }

        context.Year = int.Parse(extractedYear, NumberFormatInfo.InvariantInfo);
        var yearIndex = context.Tokens.IndexOf(extractedYear);
        if (yearIndex != -1)
        {
            context.Tokens.RemoveRange(yearIndex, context.Tokens.Count - yearIndex);
        }

        if (!trim)
        {
            return;
        }

        // Удаляем часть строки
        // Предполагаем, что название идет до года
        context.WorkingName = input[..removeStartIndex].Trim();
    }
}