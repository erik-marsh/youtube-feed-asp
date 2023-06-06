using System.Text.RegularExpressions;
using System.Text.Json;

namespace youtube_feed_asp.Scrapers;

/// <summary>
/// Collection of functionality that allows for scraping data from YouTube video pages.
/// </summary>
public static class VideoScraper
{
    /// <summary>
    /// Information that is scrapable from a YouTube video page.
    /// </summary>
    public struct Result
    {
        public string VideoId;
        public string ChannelId;
        public string Title;
        public string ShortDescription;
        public long LengthSeconds;
    }

    private readonly static HttpClient httpClient = new();

    private const string resultGroup = "result";

    /// <summary>
    /// The raw response from a get request to the above url for any given video
    /// contains a script tag that a single JSON object named ytInitialPlayerResponse.
    /// This object contains a bunch of data and metadata for the video that we want to scrape from.
    /// </summary>
    /// <returns>
    /// If the videoId is valid, returns a VideoScraper.Result object.
    /// Otherwise, returns null.
    /// </returns>
    private readonly static Regex findYtInitialPlayerResponse = new($"<script[^>]*>\\s*var ytInitialPlayerResponse\\s*=\\s*(?<{resultGroup}>.*);</script>", RegexOptions.Compiled);

    public static Result? Scrape(string videoId)
    {
        var url = $"https://www.youtube.com/watch?v={videoId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = httpClient.Send(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var textContent = response.Content.ReadAsStringAsync().Result;  // blocking

        var match = findYtInitialPlayerResponse.Match(textContent);

        var json = JsonDocument.Parse(match.Groups[resultGroup].Value);
        var videoDetails = json.RootElement.GetProperty("videoDetails");

        // The compiler likes to complain about possible null values here.
        // If they are null, that means the parser needs to be re-done.
        // If the parser needs to be re-done, the program is broken and should crash anyway.
        // TODO: However, a cleaner, more informative exception could be thrown here.
#pragma warning disable CS8601, CS8604
        return new Result()
        {
            VideoId = videoId,
            ChannelId = videoDetails.GetProperty("channelId").GetString(),
            Title = videoDetails.GetProperty("title").GetString(),
            ShortDescription = videoDetails.GetProperty("shortDescription").GetString(),
            LengthSeconds = long.Parse(videoDetails.GetProperty("lengthSeconds").GetString())
        };
#pragma warning restore CS8601, CS8604
    }
}