namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class RemoveLeadingBracket : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        if (string.IsNullOrEmpty(context.WorkingName))
        {
            return;
        }

        var result = context.WorkingName;

        // Цикл работает, пока строка начинается с открывающей скобки
        while (result.Length > 0)
        {
            var firstChar = result[0];
            char closingChar;

            // Определяем тип скобки
            if (firstChar == '(')
            {
                closingChar = ')';
            }
            else if (firstChar == '[')
            {
                closingChar = ']';
            }
            else if (firstChar == '{')
            {
                closingChar = '}';
            }
            else
            {
                break; // Если первый символ не скобка — выходим
            }

            // Ищем соответствующую закрывающую скобку
            var depth = 0;
            var closingIndex = -1;

            for (var i = 0; i < result.Length; i++)
            {
                if (result[i] == firstChar)
                {
                    depth++; // Увеличиваем глубину вложенности
                }
                else if (result[i] == closingChar)
                {
                    depth--; // Уменьшаем глубину

                    // Если глубина вернулась к 0, значит, мы нашли закрывающую скобку для самой первой
                    if (depth == 0)
                    {
                        closingIndex = i;
                        break;
                    }
                }
            }

            // Если пара найдена, удаляем эту часть из начала строки
            if (closingIndex != -1)
            {
                result = result[(closingIndex + 1)..];
            }
            else
            {
                // Если открывающая скобка есть, а закрывающей нет — прерываем цикл
                break;
            }
        }

        context.WorkingName = result.TrimStart();
    }
}
