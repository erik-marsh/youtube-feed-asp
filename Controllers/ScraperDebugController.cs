using Microsoft.AspNetCore.Mvc;
using youtube_feed_asp.Scrapers;
using youtube_feed_asp.Models;
// using System.Net.Http;
// using System.Text.RegularExpressions;
// using System.Text.Json;

namespace youtube_feed_asp.Controllers;

// The parser modules should generally be able to fetch
// each of the data fields in the Channel and Video models.
// A lot of this can be done in the RSS feed data.
// That is...
// Video:
//     Video ID         => RSS
//     Uploader         => RSS, Implicit
//     Title            => RSS
//     Time published   => RSS
// Channel:
//     Channel ID       => Channel page (somewhere)
//     Name             => RSS
//     Videos           => Most recent 15, RSS

// The different use case is if we want to add individual videos to something (i.e. watch later)
// we now have to scour thorugh the page source (because the video might now be in the RSS feed)

[ApiController]
public class ScraperDebugController : Controller
{
    [HttpGet("get-channel-rss/{channelId}")]
    public ActionResult<IEnumerable<VideoResponse>> GetVideosFromChannelRSSFeed(string channelId)
    {
        var feed = RssScraper.GetChannelRSSFeed(channelId);
        var feedVideos = feed.Items.ToList();
        var videos = new List<Video>();
        for (int i = feedVideos.Count - 1; i >= 0; i--)
        {
            videos.Add(new Video()
            {
                VideoId = RssScraper.GetVideoId(feedVideos[i]),
                Uploader = new Channel(),
                Title = feedVideos[i].Title.Text,
                TimePublished = feedVideos[i].PublishDate.ToUnixTimeSeconds(),
                TimeAdded = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Type = Enums.VideoType.Subscription
            });
        }

        return videos.ConvertAll(video => new VideoResponse(video));
    }

    [HttpGet("get-video-details/{videoId}")]
    public ActionResult<VideoScraper.Result> GetVideoDetails(string videoId) => VideoScraper.Scrape(videoId);

    // NOTE: expects an encoded URI as the channelUrl parameter
    // there is a javascript method for this so its no big deal
    [HttpGet("get-channel-details/{channelUrl}")]
    public ActionResult<ChannelScraper.Result> GetChannelDetails(string channelUrl) => ChannelScraper.Scrape(System.Web.HttpUtility.UrlDecode(channelUrl));
}