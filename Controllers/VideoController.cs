using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using youtube_feed_asp.Enums;
using youtube_feed_asp.Helpers;
using youtube_feed_asp.Models;
using youtube_feed_asp.Services;

namespace youtube_feed_asp.Controllers;

/// <summary>
/// A struct that defines how a Video object is presented to the client.
/// </summary>
/// <remarks>
/// Since Channel and Video contain a circular dependency,
/// (and the Channel component of Video is marked as [JsonIgnore])
/// this is necessary to include information about the uploader in Video items presented to the client.
/// </remarks>
public struct VideoResponse
{
    public VideoResponse(Video video)
    {
        Id = video.VideoId;
        if (video.Uploader is not null)
        {
            UploaderId = video.Uploader.ChannelId;
            UploaderName = video.Uploader.Name;
        }
        else
        {
            UploaderId = "Unknown channel";
            UploaderName = "Channel name unknown.";
        }
        Title = video.Title;
        Type = video.Type.ToString();

        DateTimeOffset published = DateTimeOffset.FromUnixTimeSeconds(video.TimePublished);
        TimePublished = published.ToString("u", CultureInfo.InvariantCulture);

        DateTimeOffset added = DateTimeOffset.FromUnixTimeSeconds(video.TimeAdded);
        TimeAdded = added.ToString("u", CultureInfo.InvariantCulture);
    }

    public string? Id { get; set; }
    public string? UploaderId { get; set; }
    public string? UploaderName { get; set; }
    public string? Title { get; set; }
    public string? TimePublished { get; set; }
    public string? TimeAdded { get; set; }
    public string? Type { get; set; }
}

public class VideoModel
{
    public VideoModel(List<Video> videos, VideoType type)
    {
        Videos = videos;
        Type = type;
    }

    public List<Video> Videos { get; private set; }
    public VideoType Type { get; private set; }
}

public class ChannelModel
{
    public ChannelModel(List<Channel> channels, VideoType type)
    {
        Channels = channels;
        Type = type;
    }

    public List<Channel> Channels { get; private set; }
    public VideoType Type { get; private set; }
}

[ApiController]
public class VideoController : Controller
{
    private VideoService m_service;

    public VideoController(VideoService service)
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

        VideoType type = (VideoType) parsedVideoType;

        var videos = m_service.VideoQuery(type, "all", SortType.DateDescending);
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

        VideoType type = (VideoType) parsedVideoType;

        var channels = m_service.ChannelQuery(type, "all", SortType.DateDescending);
        if (channels is null)
            return NotFound();

        var model = new ChannelModel(channels, type);
        return View("./ByChannelVideos", model);
    }

    [HttpGet("api/{videoType}/{channelId}/{sortType}")]
    public ActionResult<IEnumerable<VideoResponse>> ApiQuery(string videoType, string channelId, string sortType)
    {
        var videos = m_service.VideoQuery(videoType, channelId, sortType);
        if (videos is null)
            return NotFound();

        // ToList is required because the implicit conversion to an ActionResult must happen on a concrete type
        // ... i think is the reasoning
        return videos.Select(video => new VideoResponse(video)).ToList();
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