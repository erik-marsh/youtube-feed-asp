using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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


[ApiController]
[Route("[controller]")]
public class VideoController : Controller
{
    private VideoService m_service;

    public VideoController(VideoService service)
    {
        m_service = service;
    }

    [HttpGet("test")]
    public ActionResult TestQuery()
    {
        var videos = m_service.VideoQuery(VideoType.Subscription, "all", SortType.DateDescending);
        if (videos is null)
            return NotFound();

        return View("./ChronologicalVideos", videos);
    }

    [HttpGet("test2")]
    public ActionResult TestQuery2()
    {
        var channels = m_service.GetAllChannels().ToList();
        if (channels is null)
            return NotFound();

        return View("./ByChannelVideos", channels);
    }

    // TODO: these URLs are getting lengthy
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

    [HttpPost("channels/{channelId}")]
    public ActionResult SubscribeToChannel(string channelId)
    {
        bool succeeded = m_service.SubscribeToChannel(channelId);
        return succeeded ? Ok() : BadRequest();
    }

    [HttpDelete("channels/{channelId}")]
    public ActionResult UnsubscribeFromChannel(string channelId)
    {
        bool succeeded = m_service.UnsubscribeFromChannel(channelId);
        return succeeded ? Ok() : NotFound();
    }

    [HttpPut("channels/{channelId}")]
    public ActionResult UpdateChannelVideos(string channelId)
    {
        if (channelId == "all")
        {
            var channels = m_service.GetAllChannels();
            foreach (var ch in channels)
            {
                m_service.UpdateChannelSubscriptions(ch);
            }
        }
        else
        {
            var ch = m_service.GetChannel(channelId);
            if (ch is null)
                return NotFound();

            m_service.UpdateChannelSubscriptions(ch);
        }

        return Ok();
    }

    [HttpDelete("videos/{videoId}")]
    public ActionResult RemoveVideo(string videoId)
    {
        bool succeeded = m_service.DeleteVideo(videoId);
        return succeeded ? Ok() : NotFound();
    }

    // [HttpGet("videos")]
    // public IEnumerable<VideoResponse> GetAllVideos()
    // {
    //     // when your return a POCO (plain old CLR object) like this, it is wrapped in an ObjectResult implicitly, then serialized
    //     var videos = m_service.GetAllVideos();
    //     var response = new List<VideoResponse>();
    //     foreach (var v in videos)
    //     {
    //         response.Add(new VideoResponse(v));
    //     }
    //     return response;
    // }

    // [HttpGet("videos/{id}")]
    // public ActionResult<VideoResponse> GetVideo(string id)
    // {
    //     var video = m_service.GetVideo(id);
    //     if (video is null)
    //         return NotFound(); // 404 Not Found
    //     return new VideoResponse(video); // implicit 200 OK
    // }

    // [HttpGet("subscriptions")]
    // public IEnumerable<VideoResponse> GetAllSubscriptions()
    // {
    //     var videos = m_service.GetAllVideosByType(VideoType.Subscription);
    //     return videos.Select(video => new VideoResponse(video));
    // }

    // [HttpGet("watch-later")]
    // public IEnumerable<VideoResponse> GetAllWatchLaters()
    // {
    //     var videos = m_service.GetAllVideosByType(VideoType.WatchLater);
    //     return videos.Select(video => new VideoResponse(video));
    // }

    // // TODO: the videos within a channel response do not get turned into VideoResponses,
    // // hence the enums do not appear as text, just an int
    // [HttpGet("channels")]
    // public IEnumerable<Channel> GetAllChannels()
    // {
    //     return m_service.GetAllChannels();
    // }

    // [HttpPut("channels")]
    // public ActionResult UpdateAllChannels()
    // {
    //     var channels = m_service.GetAllChannels();// as List<Channel>;
    //     foreach (var ch in channels)
    //     {
    //         m_service.UpdateChannelSubscriptions(ch);
    //     }
    //     return Ok();
    // }

    // [HttpPut("channels/{id}")]
    // public ActionResult UpdateChannel(Channel ch)
    // {
    //     // TODO: need to distinguish between channels that exist and channels that don't exists with a 200 and 404 respectively
    //     m_service.UpdateChannelSubscriptions(ch);
    //     return Ok();
    // }

    // [HttpGet("channels/{id}")]
    // public ActionResult<Channel> GetChannel(string id)
    // {
    //     var channel = m_service.GetChannel(id);
    //     if (channel is null)
    //         return NotFound();
        
    //     return channel;
    // }

    // [HttpGet("channels/{id}/subscriptions")]
    // public IEnumerable<VideoResponse> GetChannelSubscriptions(string id)
    // {
    //     return m_service.GetChannelVideosByType(id, VideoType.Subscription)
    //         .Select(video => new VideoResponse(video));
    // }

    // [HttpGet("channels/{id}/watch-later")]
    // public IEnumerable<VideoResponse> GetChannelWatchLaters(string id)
    // {
    //     return m_service.GetChannelVideosByType(id, VideoType.WatchLater)
    //         .Select(video => new VideoResponse(video));
    // }
}