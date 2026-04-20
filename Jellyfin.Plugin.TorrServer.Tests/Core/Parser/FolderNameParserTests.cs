using Jellyfin.Plugin.TorrServer.Core.Parser;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Core.Parser;

public class FolderNameParserTests
{
    private FolderNameParser FolderNameParser => new();

    [Test]
    [Arguments("Terminator-3.Rise.of.the.Machines_1080p.2003.D.BDRip.mkv", "Terminator-3 Rise of the Machines (2003)", 2003)]
    [Arguments("Смертельная ярость / Мертвая жара / Мертвый полицейский / Dead Heat (Марк Голдблатт / Mark Goldblatt) [1988, США, боевик, ужасы, фантастика, комедия, BDRip 720p] 3x AVO(Михалев, Горчаков, Володарский) + MVO (НТВ) + VO + Original (eng)", "Смертельная ярость (1988)", 1988)]
    [Arguments("Убежище / Shelter (2026) WEB-DL [H.265/2160p] [4K, HDR10+, 10-bit] [EN / RU, EN Sub]", "Убежище (2026)", 2026)]
    [Arguments("Shelter (2026) WEB-DL [H.265/2160p] [4K, HDR10+, 10-bit] [EN / RU, EN Sub]", "Shelter (2026)", 2026)]
    [Arguments("Казнить нельзя помиловать / Mercy / 2026 / 2 x ПМ, ЛМ, СТ / WEB-DL (1080p)", "Казнить нельзя помиловать (2026)", 2026)]
    [Arguments("Mercy / 2026 / 2 x ПМ, ЛМ, СТ / WEB-DL (1080p)", "Mercy (2026)", 2026)]
    [Arguments("Д'Артаньян и три мушкетёра [1979, музыкальный, приключения, экранизация, HDTV to FHD 1080p AI]", "Д'Артаньян и три мушкетёра (1979)", 1979)]
    [Arguments("[NUM] Вавилон 5 / Babylon 5 / S5E1-22 of 22 (Дж. Майкл Стражински, Майкл Виджер и др.) [1998, США, Фантастика, боевик, драма, приключения, WEB-DL 1080p] MVO (S6) + Original + Sub (Rus, Eng)", "Вавилон 5 (1998)", 1998)]
    [Arguments("Хоббит: Нежданное путешествие / The Hobbit: An Unexpected Journey [2012, США, Новая Зеландия, фэнтези, приключения, BDRip 1080p] [Расширенная версия / Extended Cut] Dub + 2x AVO + Original Eng + Sub (Rus, Ukr, Eng)", "Хоббит. Нежданное путешествие (2012)", 2012)]
    [Arguments("Звездный путь: Сборник Оригинальных фильмов I-X / Star Trek: Original motion picture collection [1979-2002, США, фантастика, приключения, BDRip 1080p]", "Звездный путь. Сборник Оригинальных фильмов I-X", null)]
    [Arguments("Звездный путь: Сборник Оригинальных фильмов I-X / Star Trek: Original motion picture collection [1979-2002, США, фантастика, приключения, BDRip 1080p]", "Звездный путь. Сборник Оригинальных фильмов I-X", null)]
    [Arguments("Star Trek I-XI 1080p BluRay DTS AC3 DL x264-HDC BOZX", "Star Trek I-XI", null)]
    [Arguments("Хроники Риддика (Режиссёрская версия) The Chronicles of Riddick (Director's cut) 2004 DUB, MVO, AVO (Живов, Пучков, Гаврилов), Sub BDRip 1080p", "Хроники Риддика (2004)", 2004)]
    [Arguments("Гарри Поттер и Дары Смерти: Часть II / Harry Potter and the Deathly Hallows: Part 2 [2011, Великобритания, США, фэнтези, приключения, семейный, BDRip 1080p] Dub + 2x AVO + 3x VO + Original (Eng) + Sub (Rus, Eng)", "Гарри Поттер и Дары Смерти. Часть II (2011)", 2011)]
    [Arguments("Большая перемена [1972-1973, комедия, социальная мелодрама, экранизация, DVDRip]", "Большая перемена", null)]
    public async Task Parse_Should_Parse_Title_And_Year_Correctly(string input, string expectedTitle, int? expectedYear)
    {
        var result = FolderNameParser.Parse(input);

        await Assert.That(result.Year).IsEqualTo(expectedYear);
        await Assert.That(result.FolderName()).IsEqualTo(expectedTitle);
    }
}
