// channel response page
// there's a massive JSON response in this page containing the info we need
// but its duplicated a million and a half times, because they're all related to channel videos
// that sounds really annoying to parse, so i will try to avoid that

// channel ID can be uniquely found with a search for /<link rel="canonical" href=/
// channel handle can be uniquely found with /<link rel="alternate" media="handheld"/
//     HOWEVER: this is a mobile link, so we need to strip out an initial m. from the url
// channel name can be found with /<meta itemprop="name" content=/

// there's another interesting thing called function serverContract() { ... } that also has all the info we need
// but it's all javascript and i don't really want to parse that either

using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace youtube_feed_asp.Scrapers;

public static class ChannelScraper
{
    public struct Result
    {
        public string ChannelId;
        public string Handle;
        public string Name;
    }

    private readonly static HttpClient httpClient = new();

    private static readonly Regex findChannelId = new("<link\\s+rel=\"canonical\"\\s+href=\"https://www.youtube.com/channel/([^>]*)\">", RegexOptions.Compiled);
    private static readonly Regex findChannelHandle = new("<link\\s+rel=\"alternate\"\\s+media=\"handheld\"\\s+href=\"https://m.youtube.com/(@[^>]*)\">", RegexOptions.Compiled);
    private static readonly Regex findChannelName = new("<meta\\s+itemprop=\"name\"\\s+content=\"([^>]*)\">", RegexOptions.Compiled);

    // The idea here is to retrieve the channel ID from any sort of URL to the channel.
    public static Result Scrape(string channelUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, channelUrl);
        var response = httpClient.Send(request);
        var textContent = response.Content.ReadAsStringAsync().Result;  // blocking

        var matchChannelId = findChannelId.Match(textContent);
        var matchChannelHandle = findChannelHandle.Match(textContent);
        var matchChannelName = findChannelName.Match(textContent);

        return new Result()
        {
            ChannelId = matchChannelId.Groups[1].Value,
            Handle = matchChannelHandle.Groups[1].Value,
            Name = matchChannelName.Groups[1].Value
        };
    }
}