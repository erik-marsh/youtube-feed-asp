using youtube_feed_asp.Enums;
using youtube_feed_asp.Models;
using System.Diagnostics;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace youtube_feed_asp.VideoScraper;

public static class VideoScraper
{
    private const string s_rssBaseUrl = "https://www.youtube.com/feeds/videos.xml?channel_id=";

    public static List<Video> UpdateChannelSubscriptions(Channel ch)
    {
        var httpRequestTimer = new Stopwatch();
        var feedParsingTimer = new Stopwatch();

        if (ch is null)
        {
            Console.WriteLine($"Attempted to update channel that does not exist in the database. Abort.");
            return new List<Video>();
        }

        httpRequestTimer.Start();
        var feed = GetChannelRSSFeed(ch.ChannelId);
        httpRequestTimer.Stop();
        Console.WriteLine($"Retrieved RSS feed in {httpRequestTimer.ElapsedMilliseconds}ms");

        var feedVideos = feed.Items.ToList();
        var newVideos = new List<Video>();

        Console.WriteLine($"Updating channel: {feed.Title.Text} ({ch.ChannelId}) (last modified at {ch.LastModified})");


        feedParsingTimer.Start();
        // reverse iteration because the videos are ordered newest to oldest
        // and we want to incrementally update the LastModified field
        for (int i = feedVideos.Count - 1; i >= 0; i--)
        {
            var videoPublished = feedVideos[i].PublishDate.ToUnixTimeSeconds();

            if (videoPublished > ch.LastModified)
            {
                ch.LastModified = (int)videoPublished;

                var v = new Video() {
                    VideoId = GetVideoId(feedVideos[i]),
                    Uploader = ch,
                    Title = feedVideos[i].Title.Text,
                    TimePublished = (int)videoPublished,
                    TimeAdded = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Type = VideoType.Subscription
                };

                newVideos.Add(v);
            }
        }

        feedParsingTimer.Stop();
        Console.WriteLine($"Parsed feed in {feedParsingTimer.ElapsedMilliseconds}ms");

        Console.WriteLine($"{newVideos.Count} new videos added.");

        return newVideos;
    }

    // TODO: this is no longer a VideoScraper, more like an RSS scraper
    /// <summary>
    /// Constructs a Channel object from a channel ID.
    /// </summary>
    /// <param name="channelId">A Base64 YouTube canonical channel ID.</param>
    /// <returns>
    /// If the channel ID points to a valid YouTube channel, returns the corresponding Channel object.
    /// Otherwise, returns null.
    /// </returns>
    public static Channel? GetChannelFromID(string channelId)
    {
        // TODO: move this to GetChannelRSSFeed
        try
        {
            var feed = GetChannelRSSFeed(channelId);
            return new Channel {
                ChannelId = channelId,
                Name = feed.Title.Text,
                LastModified = 0,
                Videos = new List<Video>()
            };
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($"{ex.Message} at {ex.Source}");
            return null;
        }
    }

    /// <summary>
    /// Fetches the RSS feed associated with a certain channel and returns it.
    /// </summary>
    private static SyndicationFeed GetChannelRSSFeed(string channelId)
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
    /// I think it's better for the application to crash here, since this is a critical error.
    /// Not having access to YouTube video IDs renders this entire application useless.
    /// </exception>
    private static string GetVideoId(SyndicationItem item)
    {
        var extensionObject = item.ElementExtensions.Single(x => x.OuterName == "videoId");
        return extensionObject.GetObject<XElement>().Value;
    }

}