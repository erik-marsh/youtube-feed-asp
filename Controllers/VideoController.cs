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
        Id = video.Id;
        if (video.Uploader is not null)
        {
            UploaderId = video.Uploader.Id;
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

    [HttpGet("channels")]
    public IEnumerable<Channel> GetAllChannels()
    {
        return m_service.GetAllChannels();
    }
}