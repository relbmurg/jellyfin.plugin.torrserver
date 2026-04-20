using Jellyfin.Plugin.TorrServer.Core.Parser;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Tests.Core.Parser;

public class FileNameParserTests
{
    private FileNameParser FileNameParser => new();

    [Test]
    [Arguments("Gladiator.2000.1080p.BluRay.x264.mkv", "Gladiator", 2000)]
    [Arguments("Матрица.1999.BDRip.720p.mkv", "Матрица", 1999)]
    [Arguments("Dune.Part.Two.2024.Extended.4K.mkv", "Dune Part Two", 2024)]
    [Arguments("Blade.Runner.1982.Directors.Cut.1080p.BluRay.x264.mkv", "Blade Runner", 1982)]
    [Arguments("[FGT] The.Matrix.1999.1080p.BluRay.x264-FGT.mkv", "The Matrix", 1999)]
    [Arguments("(SPARKS) Interstellar.2014.1080p.BluRay.x264-SPARKS.mkv", "Interstellar", 2014)]
    [Arguments("The Matrix 1080p BluRay x264.mkv", "The Matrix", null)]
    [Arguments("x264.720p.WEB-DL.Interstellar.2014..mkv", "Interstellar", 2014)]
    [Arguments("No.Year.Movie.1080p.mkv", "No Year Movie", null)]
    [Arguments("The.Hitchhiker's.Guide.to.the.Galaxy.2005.1080p.BluRay.5xRus.3xEng.HDCLUB-Skazhutin.mkv", "The Hitchhiker's Guide to the Galaxy", 2005)]
    public async Task Parse_Should_Parse_Title_And_Year_Correctly(string input, string expectedTitle, int? expectedYear)
    {
        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo(expectedTitle);
        await Assert.That(result.Year).IsEqualTo(expectedYear);
    }

    [Test]
    public async Task Parse_Should_Be_Idempotent_For_StrmFileName()
    {
        var input = "Blade.Runner.1982.Directors.Cut.1080p.BluRay.x264.mkv";

        var first = FileNameParser.Parse(input);

        var second = FileNameParser.Parse(first.StrmFileName());

        await Assert.That(second.Title).IsEqualTo(first.Title);
        await Assert.That(second.Year).IsEqualTo(first.Year);

        await Assert.That(second.VersionTags)
            .IsEquivalentTo(first.VersionTags);

        await Assert.That(second.TechnicalTags)
            .IsEquivalentTo(first.TechnicalTags);
    }

    [Test]
    public async Task Parse_Should_Parse_Standard_Movie_With_Version_And_Technical_Tags()
    {
        var input = "Blade.Runner.1982.Directors.Cut.1080p.BluRay.x264-GROUP.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Blade Runner");
        await Assert.That(result.Year).IsEqualTo(1982);

        await Assert.That(result.VersionTags).Contains("Director's Cut");
        await Assert.That(result.TechnicalTags).Contains("1080p");
        await Assert.That(result.TechnicalTags).Contains("BluRay");
        await Assert.That(result.TechnicalTags).Contains("x264");

        await Assert.That(result.FolderName).IsEqualTo("Blade Runner (1982)");
        await Assert.That(result.StrmFileName).IsEqualTo("Blade Runner (1982) [Director's Cut] [1080p BluRay x264].strm");
    }

    [Test]
    public async Task Parse_Should_Take_Last_Valid_Year()
    {
        var input = "1917.2019.1080p.BluRay.x264.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("1917");
        await Assert.That(result.Year).IsEqualTo(2019);
    }

    [Test]
    public async Task Parse_Should_Parse_Extended_4K_WebDl()
    {
        var input = "Dune.Part.Two.2024.Extended.4K.WEB-DL.x265.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Dune Part Two");
        await Assert.That(result.Year).IsEqualTo(2024);

        await Assert.That(result.VersionTags).Contains("Extended");
        await Assert.That(result.TechnicalTags).Contains("4K");
        await Assert.That(result.TechnicalTags).Contains("WEB-DL");
        await Assert.That(result.TechnicalTags).Contains("x265");
    }

    [Test]
    public async Task Parse_Should_Handle_Russian_Titles()
    {
        var input = "Матрица.1999.BDRip.1080p.x264.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Матрица");
        await Assert.That(result.Year).IsEqualTo(1999);

        await Assert.That(result.TechnicalTags).Contains("BDRip");
        await Assert.That(result.TechnicalTags).Contains("1080p");
    }

    [Test]
    public async Task Parse_Should_Work_Without_Year()
    {
        var input = "My.Movie.1080p.BluRay.x264.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("My Movie");
        await Assert.That(result.Year).IsNull();

        await Assert.That(result.FolderName).IsEqualTo("My Movie");
    }

    [Test]
    public async Task Parse_Should_Produce_Different_Strm_For_Different_Versions()
    {
        var input1 = "Alien.1979.Directors.Cut.1080p.BluRay.x264.mkv";
        var input2 = "Alien.1979.Theatrical.1080p.BluRay.x264.mkv";

        var r1 = FileNameParser.Parse(input1);
        var r2 = FileNameParser.Parse(input2);

        await Assert.That(r1.FolderName).IsEqualTo(r2.FolderName());
        await Assert.That(r1.StrmFileName).IsNotEqualTo(r2.StrmFileName());
    }

    [Test]
    public async Task Parse_Should_Preserve_Technical_Tag_Order()
    {
        var input = "Movie.2023.2160p.REMUX.HEVC.Atmos.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.TechnicalTags.Count).IsEqualTo(4);

        await Assert.That(result.TechnicalTags[0]).IsEqualTo("2160p");
        await Assert.That(result.TechnicalTags[1]).IsEqualTo("REMUX");
        await Assert.That(result.TechnicalTags[2]).IsEqualTo("HEVC");
        await Assert.That(result.TechnicalTags[3]).IsEqualTo("Atmos");
    }

    [Test]
    public async Task Parse_Should_Handle_Noise_And_Multiple_Separators()
    {
        var input = "[GROUP]---The.Matrix.(1999)---1080p___BluRay---x264.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("The Matrix");
        await Assert.That(result.Year).IsEqualTo(1999);

        await Assert.That(result.TechnicalTags)
            .Contains("1080p")
            .And.Contains("BluRay")
            .And.Contains("x264");
    }

    [Test]
    public async Task Parse_Should_Not_Treat_Resolution_As_Year()
    {
        var input = "Movie.720p.1080p.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Year).IsNull();
    }

    [Test]
    public async Task Parse_Should_Generate_Different_Strm_For_Different_Technical_Tags()
    {
        var input1 = "Avatar.2009.1080p.BluRay.x264.mkv";
        var input2 = "Avatar.2009.2160p.REMUX.HEVC.mkv";

        var r1 = FileNameParser.Parse(input1);
        var r2 = FileNameParser.Parse(input2);

        await Assert.That(r1.FolderName).IsEqualTo("Avatar (2009)");
        await Assert.That(r2.FolderName).IsEqualTo("Avatar (2009)");

        await Assert.That(r1.StrmFileName).IsNotEqualTo(r2.StrmFileName());
    }

    [Test]
    public async Task Parse_Should_Remove_Dash_Release_Group()
    {
        var input = "Blade.Runner.1982.1080p.BluRay.x264-GROUP.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Blade Runner");
    }

    [Test]
    public async Task Parse_Should_Remove_Bracket_Release_Group_From_Start()
    {
        var input = "[GROUP] Blade.Runner.1982.1080p.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Blade Runner");
    }

    [Test]
    public async Task Parse_Should_Not_Remove_Normal_Last_Word()
    {
        var input = "The.Last.Samurai.2003.1080p.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("The Last Samurai");
    }

    [Test]
    public async Task Parse_Should_Not_Remove_Dash_In_Digits()
    {
        var input = "Battlestar Galactica 00-01.mkv";

        var result = FileNameParser.Parse(input);

        await Assert.That(result.Title).IsEqualTo("Battlestar Galactica 00-01");
    }

}
