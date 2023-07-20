using Microsoft.SemanticKernel;
namespace ProjectLegion;

public class Core
{
    private readonly Config _config;
    public Core(Config config)
    {
        _config = config;
    }

    public async Task<string> Test()
    {
        var kernel = new KernelBuilder()
            .WithOpenAITextCompletionService(
                _config.OpenAIModel,
                _config.OpenAIApiKey)
            .Build();

        var prompt = @"{{$input}}

One line TLDR with the fewest words.";

        var summarize = kernel.CreateSemanticFunction(prompt);

        string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

        string text2 = @"
1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
2. The acceleration of an object depends on the mass of the object and the amount of force applied.
3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

        return await summarize.InvokeAsync(text1) + "\n" + await summarize.InvokeAsync(text2);
    }
}
