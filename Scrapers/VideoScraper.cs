using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace youtube_feed_asp.Scrapers;

public static class VideoScraper
{
    public struct Result
    {
        public string VideoId;
        public string ChannelId;
        public string Title;
        public string ShortDescription;
        public long LengthSeconds;
    }

    private readonly static HttpClient httpClient = new();

    // The raw response from a get request to the above url for any given video
    // contains a script tag that a single JSON object named ytInitialPlayerResponse.
    // This object contains a bunch of data and metadata for the video.
    private readonly static Regex findYtInitialPlayerResponse = new(
        @"<script[^>]*>" +  // open script tag, ignore any attributes
        @"\s*var ytInitialPlayerResponse\s*=\s*" +  // this is the variable we are looking for
        @"(.*)" +           // this group will contain the JSON string that we want to parse
        @";</script>",      // and the ending of the script tag
        RegexOptions.Compiled
    );

    public static Result Scrape(string videoId)
    {
        var url = $"https://www.youtube.com/watch?v={videoId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = httpClient.Send(request);
        var textContent = response.Content.ReadAsStringAsync().Result;  // blocking

        var match = findYtInitialPlayerResponse.Match(textContent);

        // TODO: i'm not exactly sure why the group is specified in the regex is the second group
        var json = JsonDocument.Parse(match.Groups[1].Value);
        var videoDetails = json.RootElement.GetProperty("videoDetails");

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