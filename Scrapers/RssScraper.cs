using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace youtube_feed_asp.Scrapers;

/// <summary>
/// Collection of functionality that allows for scraping data from YouTube channel RSS feeds.
/// </summary>
public static class RssScraper
{
    public struct Result
    {
        public string VideoId;
        public string Title;
        public int TimePublished;
    }

    private const string s_rssBaseUrl = "https://www.youtube.com/feeds/videos.xml?channel_id=";

    /// <summary>
    /// Retrieves information about the latest 15 videos from a YouTube channel.
    /// </summary>
    /// <param name="channelId">The channel ID to fetch new videos from.</param>
    /// <returns>A list of RSS results, ordered from oldest to newest.</returns>
    public static List<Result> Scrape(string channelId)
    {
        var feed = GetChannelRSSFeed(channelId);
        var feedVideos = feed.Items.ToList();
        var result = new List<Result>();

        // reverse iteration because the videos are ordered newest to oldest
        // and we want to incrementally update the LastModified field
        for (int i = feedVideos.Count - 1; i >= 0; i--)
        {
            var video = feedVideos[i];
            var videoInfo = new Result()
            {
                VideoId = GetVideoId(video),
                Title = video.Title.Text,
                TimePublished = (int)video.PublishDate.ToUnixTimeSeconds()
            };
            result.Add(videoInfo);
        }

        return result;
    }

    /// <summary>
    /// Fetches the RSS feed associated with a certain channel and returns it.
    /// </summary>
    public static SyndicationFeed GetChannelRSSFeed(string channelId)
    {
        string rssUrl = s_rssBaseUrl + channelId;
        var reader = XmlReader.Create(rssUrl);
        var feed = SyndicationFeed.Load(reader);
        reader.Close();

        return feed;
    }

    /// <summary>
    /// Extracts a Base64 YouTube video ID from a video entry in a YouTube RSS feed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// May throw this exception if the SyndicationItem has no videoId extension element.
    /// This is a parsing error, which should be very visible to the end user:
    /// if the parser doesn't work, then the application is useless.
    /// </exception>
    private static string GetVideoId(SyndicationItem item)
    {
        var extensionObject = item.ElementExtensions.Single(x => x.OuterName == "videoId");
        return extensionObject.GetObject<XElement>().Value;
    }
}