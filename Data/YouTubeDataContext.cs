using Microsoft.EntityFrameworkCore;
using youtube_feed_asp.Models;

namespace youtube_feed_asp.Data;

public class YouTubeDataContext : DbContext
{
    public YouTubeDataContext(DbContextOptions<YouTubeDataContext> options)
        : base(options)
    {
    }

    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Channel> Channels => Set<Channel>();
}