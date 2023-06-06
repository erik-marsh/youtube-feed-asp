using System.Text.RegularExpressions;

namespace youtube_feed_asp.Scrapers;

/// <summary>
/// Collection of functionality that allows for scraping data from YouTube channel pages.
/// </summary>
public static class ChannelScraper
{
    /// <summary>
    /// Information that is scrapable from a YouTube channel page.
    /// </summary>
    public struct Result
    {
        public string ChannelId;
        public string Handle;
        public string Name;
    }

    private readonly static HttpClient httpClient = new();

    private const string resultGroup = "result";

    /// <summary>
    /// Searches a YouTube channel page for the channel's base64 ID.
    /// </summary>
    private static readonly Regex findChannelId = new($"<link\\s+rel=\"canonical\"\\s+href=\"https://www.youtube.com/channel/(?<{resultGroup}>[^>]*)\">", RegexOptions.Compiled);

    /// <summary>
    /// Searches a YouTube channel page for the channel's handle.
    /// TODO: This worked at some point but very quickly stopped working.
    /// </summary>
    private static readonly Regex findChannelHandle = new($"<link\\s+rel=\"alternate\"\\s+media=\"handheld\"\\s+href=\"https://m.youtube.com/(?<{resultGroup}>[^>]*)\">", RegexOptions.Compiled);

    /// <summary>
    /// Searches a YouTube channel page for the channel's name.
    /// </summary>
    private static readonly Regex findChannelName = new($"<meta\\s+itemprop=\"name\"\\s+content=\"(?<{resultGroup}>[^>]*)\">", RegexOptions.Compiled);

    /// <summary>
    /// Scrapes the channel ID, channel handle, and channel name from a YouTube channel URL.
    /// </summary>
    /// <param name="channelUrl">Any sort of URL that points to a YouTube channel.</param>
    /// <returns>
    /// Returns a valid ChannelScraper.Result object if the channelUrl exists.
    /// Otherwise, returns null.
    /// </returns>
    public static Result? Scrape(string channelUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, channelUrl);
        var response = httpClient.Send(request);
        if (!response.IsSuccessStatusCode) return null;

        var textContent = response.Content.ReadAsStringAsync().Result;  // blocking

        var matchChannelId = findChannelId.Match(textContent);
        //var matchChannelHandle = findChannelHandle.Match(textContent);
        var matchChannelName = findChannelName.Match(textContent);

        return new Result()
        {
            ChannelId = matchChannelId.Groups[resultGroup].Value,
            Handle = "<no value, not implemented yet>", //matchChannelHandle.Groups[resultGroup].Value,
            Name = matchChannelName.Groups[resultGroup].Value
        };
    }
}