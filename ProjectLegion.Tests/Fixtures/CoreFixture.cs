using Microsoft.Extensions.Configuration;

namespace ProjectLegion.Tests.Fixtures;
public sealed class CoreFixture
{
    private static readonly Lazy<CoreFixture> lazy = new(() => new CoreFixture());

    public static CoreFixture Instance { get { return lazy.Value; } }

    public string? OpenAIModel { get; init; }
    public string? OpenAIApiKey { get; init; }
    public Core Default { get; init; }

    private CoreFixture()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<CoreFixture>().Build();
        OpenAIModel = config["OpenAIModel"] ?? throw new ApplicationException("OpenAI Model not found.");
        OpenAIApiKey = config["OpenAIApiKey"] ?? throw new ApplicationException("OpenAI Api Key not found.");

        // Initialize Default ProjectLegion.Core
        Default = new Core(new Config()
        {
            OpenAIApiKey = OpenAIApiKey ?? throw new ApplicationException("OpenAI Model not found."),
            OpenAIModel = OpenAIModel ?? throw new ApplicationException("OpenAI Api Key not found.")
        });
    }
}
