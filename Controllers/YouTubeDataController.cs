using Microsoft.AspNetCore.Mvc;
using youtube_feed_asp.Enums;
using youtube_feed_asp.Helpers;
using youtube_feed_asp.Models;
using youtube_feed_asp.Services;
using youtube_feed_asp.Views;

namespace youtube_feed_asp.Controllers;

[ApiController]
public class YouTubeDataController : Controller
{
    private readonly YouTubeDataService m_service;

    public YouTubeDataController(YouTubeDataService service)
    {
        m_service = service;
    }

    [HttpGet("/")]
    public ActionResult Index() => Redirect("views/subscriptions/by-date");

    [HttpGet("views/{videoType}/by-date")]
    public ActionResult VideoPageChronological(string videoType)
    {
        VideoType? parsedVideoType = Parsing.ParseVideoType(videoType);

        if (parsedVideoType is null)
            return NotFound();

        VideoType type = (VideoType)parsedVideoType;

        var videos = m_service.GetAllVideos(type, SortType.DateDescending);
        if (videos is null)
            return NotFound();

        var model = new VideoModel(videos, type);
        return View("./ChronologicalVideos", model);
    }

    [HttpGet("views/{videoType}/by-channel")]
    public ActionResult VideoPageByChannel(string videoType)
    {
        VideoType? parsedVideoType = Parsing.ParseVideoType(videoType);

        if (parsedVideoType is null)
            return NotFound();

        VideoType type = (VideoType)parsedVideoType;

        var channels = m_service.GetAllChannels(type, SortType.DateDescending);
        if (channels is null)
            return NotFound();

        var model = new ChannelModel(channels, type);
        return View("./ByChannelVideos", model);
    }

    [HttpGet("api/{videoType}/{channelId}/{sortType}")]
    public ActionResult<IEnumerable<Video.Serialized>> ApiQuery(string videoType, string channelId, string sortType)
    {
        VideoType? parsedVideoType = Parsing.ParseVideoType(videoType);
        SortType? parsedSortType = Parsing.ParseSortType(sortType);

        if (parsedSortType is null || parsedVideoType is null)
        {
            Console.WriteLine("Invalid sort type or video type.");
            return NotFound();
        }

        VideoType type = (VideoType)parsedVideoType;
        SortType sort = (SortType)parsedSortType;

        var videos = m_service.GetChannelVideos(type, channelId, sort);
        if (videos is null)
            return NotFound();

        return videos.ConvertAll(video => video.Serialize());
    }

    [HttpPost("api/channels/{channelId}")]
    public ActionResult SubscribeToChannel(string channelId)
    {
        bool succeeded = m_service.SubscribeToChannel(channelId);
        return succeeded ? Ok() : BadRequest();
    }

    [HttpDelete("api/channels/{channelId}")]
    public ActionResult UnsubscribeFromChannel(string channelId)
    {
        bool succeeded = m_service.UnsubscribeFromChannel(channelId);
        return succeeded ? Ok() : NotFound();
    }

    [HttpPut("api/channels/{channelId}")]
    public ActionResult UpdateChannelVideos(string channelId)
    {
        if (channelId == "all")
        {
            m_service.UpdateAllChannelSubscriptions();
            return Ok();
        }
        else
        {
            bool succeeded = m_service.UpdateChannelSubscriptions(channelId);
            return succeeded ? Ok() : NotFound();
        }
    }

    [HttpDelete("api/videos/{videoId}")]
    public ActionResult RemoveVideo(string videoId)
    {
        bool succeeded = m_service.DeleteVideo(videoId);
        return succeeded ? Ok() : NotFound();
    }
}