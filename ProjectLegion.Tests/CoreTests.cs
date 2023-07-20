namespace ProjectLegion.Tests;

public class CoreTests
{
    private ITestOutputHelper Log { get; init; }
    public CoreTests(ITestOutputHelper outputHelper)
    {
        Log = outputHelper;
    }

    [Fact]
    public async void CoreTest()
    {
        // Arrange
        string TestOutput;

        // Act
        TestOutput = await Fixtures.CoreFixture.CoreDefault.TestOpenAIConnection();
        Log.WriteLine(TestOutput);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(TestOutput));
    }
}
