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
public class VideoController : ControllerBase
{
    private VideoService m_service;

    public VideoController(VideoService service)
    {
        m_service = service;
    }

    // TODO: these URLs are getting lengthy
    [HttpGet("api/{videoType}/{channelId}/{sortType}")]
    public ActionResult<IEnumerable<VideoResponse>> VideoQuery(string videoType, string channelId, string sortType)
    {
        VideoType? parsedVideoType = videoType switch {
            "subscriptions" => VideoType.Subscription,
            "watch-later" => VideoType.WatchLater,
            _ => null
        };

        SortType? parsedSortType = sortType switch {
            "date" => SortType.Date,
            "channel" => SortType.Channel,
            _ => null 
        };

        Console.WriteLine($"VideoType == {parsedVideoType.ToString()}");
        Console.WriteLine($"SortType == {parsedSortType.ToString()}");

        if (parsedSortType is null || parsedVideoType is null)
            return NotFound();
        
        // TODO: there has to be a better way
        VideoType type = (VideoType) parsedVideoType;
        SortType sort = (SortType) parsedSortType;

        var videos = new List<Video>();

        if (channelId == "all")
        {
            Console.WriteLine("parsing all channels");
            videos.AddRange(m_service.QueryAllChannels(type, sort));
        }
        else
        {
            Console.WriteLine($"attempting to parse regular channel {channelId}");
            try
            {
                videos.AddRange(m_service.QueryChannel(channelId, type, sort));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }

        // ToList is required because the implicit conversion to an ActionResult must happen on a concrete type
        // ... i think is the reasoning
        return videos.Select(video => new VideoResponse(video)).ToList();
    }

    [HttpGet("videos")]
    public IEnumerable<VideoResponse> GetAllVideos()
    {
        // when your return a POCO (plain old CLR object) like this, it is wrapped in an ObjectResult implicitly, then serialized
        var videos = m_service.GetAllVideos();
        var response = new List<VideoResponse>();
        foreach (var v in videos)
        {
            response.Add(new VideoResponse(v));
        }
        return response;
    }

    [HttpGet("videos/{id}")]
    public ActionResult<VideoResponse> GetVideo(string id)
    {
        var video = m_service.GetVideo(id);
        if (video is null)
            return NotFound(); // 404 Not Found
        return new VideoResponse(video); // implicit 200 OK
    }

    [HttpGet("subscriptions")]
    public IEnumerable<VideoResponse> GetAllSubscriptions()
    {
        var videos = m_service.GetAllVideosByType(VideoType.Subscription);
        return videos.Select(video => new VideoResponse(video));
    }

    [HttpGet("watch-later")]
    public IEnumerable<VideoResponse> GetAllWatchLaters()
    {
        var videos = m_service.GetAllVideosByType(VideoType.WatchLater);
        return videos.Select(video => new VideoResponse(video));
    }

    // TODO: the videos within a channel response do not get turned into VideoResponses,
    // hence the enums do not appear as text, just an int
    [HttpGet("channels")]
    public IEnumerable<Channel> GetAllChannels()
    {
        return m_service.GetAllChannels();
    }

    [HttpPut("channels")]
    public ActionResult UpdateAllChannels()
    {
        var channels = m_service.GetAllChannels();// as List<Channel>;
        foreach (var ch in channels)
        {
            m_service.UpdateChannelSubscriptions(ch);
        }
        return Ok();
    }

    [HttpPut("channels/{id}")]
    public ActionResult UpdateChannel(Channel ch)
    {
        // TODO: need to distinguish between channels that exist and channels that don't exists with a 200 and 404 respectively
        m_service.UpdateChannelSubscriptions(ch);
        return Ok();
    }

    [HttpGet("channels/{id}")]
    public ActionResult<Channel> GetChannel(string id)
    {
        var channel = m_service.GetChannel(id);
        if (channel is null)
            return NotFound();
        
        return channel;
    }

    [HttpGet("channels/{id}/subscriptions")]
    public IEnumerable<VideoResponse> GetChannelSubscriptions(string id)
    {
        return m_service.GetChannelVideosByType(id, VideoType.Subscription)
            .Select(video => new VideoResponse(video));
    }

    [HttpGet("channels/{id}/watch-later")]
    public IEnumerable<VideoResponse> GetChannelWatchLaters(string id)
    {
        return m_service.GetChannelVideosByType(id, VideoType.WatchLater)
            .Select(video => new VideoResponse(video));
    }
}