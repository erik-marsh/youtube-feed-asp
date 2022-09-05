using youtube_feed_asp.Models;
using youtube_feed_asp.Data;
using Microsoft.EntityFrameworkCore;

namespace youtube_feed_asp.Services;

public class VideoService
{
    private readonly VideoContext m_context;

    public VideoService(VideoContext context)
    {
        m_context = context;
    }

    public IEnumerable<Video> GetAllVideos()
    {
        return m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .ToList();
    }

    public IEnumerable<Channel> GetAllChannels()
    {
        return m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .ToList();
    }
}