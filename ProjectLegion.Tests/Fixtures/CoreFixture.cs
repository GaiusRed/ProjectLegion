using Microsoft.Extensions.Configuration;

namespace ProjectLegion.Tests.Fixtures;
public sealed class CoreFixture
{
    private static readonly Lazy<CoreFixture> lazyInstance = new(() => new CoreFixture());

    private static readonly Lazy<Config> lazyConfigDefault = new(() => new Config()
    {
        OpenAIApiKey = Instance.OpenAIApiKey ?? throw new ApplicationException("OpenAI Model not found."),
        OpenAIModel = Instance.OpenAIModel ?? throw new ApplicationException("OpenAI Api Key not found.")
    });

    private static readonly Lazy<Core> lazyCoreDefault = new(() => new Core(ConfigDefault));

    public static CoreFixture Instance { get { return lazyInstance.Value; } }
    public static Config ConfigDefault { get { return lazyConfigDefault.Value; } }
    public static Core CoreDefault { get { return lazyCoreDefault.Value; } }

    public string? OpenAIModel { get; init; }
    public string? OpenAIApiKey { get; init; }

    private CoreFixture()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<CoreFixture>().Build();
        OpenAIModel = config["OpenAIModel"] ?? throw new ApplicationException("OpenAI Model not found.");
        OpenAIApiKey = config["OpenAIApiKey"] ?? throw new ApplicationException("OpenAI Api Key not found.");
    }
}
