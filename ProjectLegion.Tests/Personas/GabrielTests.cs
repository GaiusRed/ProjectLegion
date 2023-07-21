using ProjectLegion.Personas;
using ProjectLegion.Tests.Fixtures;

namespace ProjectLegion.Tests;

public class GabrielTests
{
    private readonly ITestOutputHelper Log;

    public GabrielTests(ITestOutputHelper outputHelper)
    {
        Log = outputHelper;
    }

    [Fact]
    public async void Invoke_10TitlesFrom5Feeds_Expect5Posts()
    {
        // Arrange
        List<string> RSSFeeds = new(){
            "https://www.philstar.com/rss/headlines",
            "https://data.gmanetwork.com/gno/rss/news/feed.xml",
            "https://www.inquirer.net/fullfeed/",
            "https://www.eaglenews.ph/feed/",
            "https://philnews.ph/feed/"
        };
        int NumberOfTitlesPerFeed = 10;
        int NumberOfPostsToGenerate = 5;
        Progress<string> progress = new(message => { Log.WriteLine(message); });

        // Act
        List<string> r = await Gabriel.Invoke(CoreFixture.CoreDefault.Kernel, progress,
            RSSFeeds, NumberOfTitlesPerFeed, NumberOfPostsToGenerate);
        r.ForEach(_ => Log.WriteLine(_));

        // Assert
        Assert.Equal(NumberOfPostsToGenerate, r.Count);
    }
}
