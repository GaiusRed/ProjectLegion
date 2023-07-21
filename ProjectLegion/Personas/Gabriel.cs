using CodeHollow.FeedReader;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ProjectLegion.Personas;

/// <summary>The Gabriel persona generates social media posts from scraped news articles.</summary>
public class Gabriel
{
    public static async Task<List<string>> Invoke(IKernel ai, IProgress<string> progress,
        List<string> RSSFeeds, int NumberOfTitlesPerFeed, int NumberOfPostsToGenerate)
    {
        // 1. Retrieve News Titles and Links from RSS Feeds
        Dictionary<string, string> News = new();
        progress.Report("Found " + RSSFeeds.Count + " RSS Feeds to process");
        foreach (string url in RSSFeeds)
            (await RetrieveNews(url, NumberOfTitlesPerFeed, progress))
                .ToList().ForEach(_ => News.Add(_.Key, _.Value));

        // List Titles for AI processing
        string NewsTitles = string.Empty;
        foreach (KeyValuePair<string, string> _ in News)
            NewsTitles += _.Key + Environment.NewLine;

        // 2. Use AI to determine sentiment
        progress.Report("Applying AI sentiment analysis to " + News.Count.ToString() + " news titles.");
        var RankedNews = await SentimentAnalysisFunc(ai).InvokeAsync(NewsTitles);

        // Sort the results
        var SortedRankedNews = RankedNews.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('|'))
            .Select(parts => (Value: parts[0], Number: int.Parse(parts[1])))
            .ToList();
        SortedRankedNews.Sort((pair1, pair2) => pair2.Number.CompareTo(pair1.Number));

        // 3. Use AI to generate a social media post
        progress.Report("Using AI to generate " + NumberOfPostsToGenerate.ToString() + " social media posts.");
        List<string> r = new();
        var generateSocMedPostFunc = GenerateSocMedPostFunc(ai);
        foreach ((string newsTitle, int rank) in SortedRankedNews.Take(NumberOfPostsToGenerate))
            r.Add((await generateSocMedPostFunc.InvokeAsync(News[newsTitle])).ToString());

        return r;
    }

    private static async Task<Dictionary<string, string>> RetrieveNews(string url, int take, IProgress<string> progress)
    {
        int retryCount = 0;
        Dictionary<string, string> r = new();

        while (true)
        {
            try
            {
                r.Clear();
                progress.Report("Processing " + url);
                foreach (var item in (await FeedReader.ReadAsync(url)).Items.Take(take))
                {
                    progress.Report("Storing " + item.Title);
                    r.Add(item.Title, item.Description);
                }
                break;
            }
            catch (Exception)
            {
                progress.Report("Unable to retrieve " + url);
                if (++retryCount == 5) // Five retries max
                    throw;
                await Task.Delay(TimeSpan.FromSeconds(2));
                progress.Report("Retrying, attempt " + (retryCount + 1));
            }
        }
        return r;
    }

    private static ISKFunction SentimentAnalysisFunc(IKernel ai)
    {
        string skPrompt = @"
From the list of news titles below as input, for each line, rate how sad or happy an average filipino would find the news. The rating should range from zero to one hundred, with 0 for very sad and 100 for very happy. Append a pipe after every line, then the rating.

Here are examples:

INPUT:
In the Midst of Tragedy, A Community Rallies: The Heroic Efforts of the Aberdale Firefighters
Global Warming Effects Diminishing: A Prolonged Fight with Slow Progress

RESULT:
In the Midst of Tragedy, A Community Rallies: The Heroic Efforts of the Aberdale Firefighters|12
Global Warming Effects Diminishing: A Prolonged Fight with Slow Progress|31

INPUT:
Scientific Breakthrough: New Alzheimer's Treatment Shows Promising Results
The Grand Reopening of National Museums Post-Pandemic: A Mixture of Joy and Caution
World Peace Day Celebrated With Global Ceasefire: A Historical Moment of Unity

RESULT:
Scientific Breakthrough: New Alzheimer's Treatment Shows Promising Results|76
The Grand Reopening of National Museums Post-Pandemic: A Mixture of Joy and Caution|53
World Peace Day Celebrated With Global Ceasefire: A Historical Moment of Unity|99

INPUT:
{{$input}}

RESULT:";

        return ai.CreateSemanticFunction(skPrompt, maxTokens: 2048, temperature: 0, topP: 0.5);
    }

    private static ISKFunction GenerateSocMedPostFunc(IKernel ai)
    {
        string skPrompt = @"
You are the best social media manager and you have to make a post in a new social media app called `Threads`. The post has to be happy, cheerful and positive, and does will not violate the usual moderation guidelines that is common in most social media applications. You ALWAYS start the post with `#MagandangBalitaPilipinas! ` which is translated as `Good News, Philippines!`. This new app has a 495 character limit, but you usually limit your posts below the limit. You also do not use hashtags, and use emojis sparingly. Your posts celebrate news articles that is provided as input.

Here are examples:

INPUT:
For the first time in history, the World Peace Day was celebrated with a global ceasefire. The world rejoiced in unity as conflict-ridden zones observed a 24-hour peace period, highlighting the potential for humanity to lay down its arms and choose peace, at least for a day

RESULT:
#MagandangBalitaPilipinas! 🇵🇭 The globe is ringing with the bells of peace today. ✌️ In an unprecedented act of unity, the world observed a 24-hour ceasefire to honor World Peace Day. This historic moment shows us the immense potential we have to choose harmony over discord, if only for a day. May this remind us all to strive for a peaceful world, every day. 🌏❤️✨

INPUT:
Hope for millions worldwide, as the latest Alzheimer's treatment trials reveal groundbreaking success. This significant advancement could revolutionize the way we understand and treat the disease, offering a beacon of hope for patients and their families

RESULT:
#MagandangBalitaPilipinas! 🇵🇭 Hope is on the horizon as a breakthrough in Alzheimer's treatment shows promising results. This leap forward could change our understanding and approach to the disease, shining a bright beacon of optimism for patients and their families worldwide. Let's celebrate this medical milestone and continue to support research for a healthier future! 🩺💡🎉

INPUT:
{{$input}}

RESULT:";

        return ai.CreateSemanticFunction(skPrompt, maxTokens: 1024, temperature: 0, topP: 0.5);
    }
}