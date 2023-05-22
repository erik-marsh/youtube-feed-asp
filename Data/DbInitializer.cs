using youtube_feed_asp.Enums;
using youtube_feed_asp.Models;
using youtube_feed_asp.Scrapers;

namespace youtube_feed_asp.Data;

public static class DbInitializer
{
    public static void Initialize(VideoContext context)
    {
        InitializeWithOnlyChannels(context);
    }

    public static void InitializeWithOnlyChannels(VideoContext context)
    {
        if (context.Channels.Any()) return;

        var channels = new List<Channel>
        {
            new Channel
            {
                ChannelId = "UCqms1E3UhFpC0RRxZQNF-ag",
                Name = "fuopy",
                LastModified = 0
            },
            new Channel
            {
                ChannelId = "UCSPLhwvj0gBufjDRzSQb3GQ",
                Name = "BobbyBroccoli",
                LastModified = 0,
                Videos = new List<Video>()
            },
            new Channel
            {
                ChannelId = "UCLrno_gfh32wX4I8-6qV4Wg",
                Name = "Running Shine",
                LastModified = 0,
                Videos = new List<Video>()
            },
            new Channel
            {
                ChannelId = "UCBn6YYmhnruGtEtlKxYbQ9A",
                Name = "K Klein",
                LastModified = 0,
                Videos = new List<Video>()
            }
        };

        var test = new Channel
        {
            ChannelId = "UCuvSqzfO_LV_QzHdmEj84SQ",
            Name = "watchlater",
            LastModified = 0,
            Videos = new List<Video>()
        };

        channels.Add(test);

        foreach (var ch in channels)
        {
            var newVideos = RssScraper.UpdateChannelSubscriptions(ch);
            context.Videos.AttachRange(newVideos);
            // context.Entry(ch)
            //     .Property(x => x.LastModified)
            //     .IsModified = true;
        }

        foreach (var vid in test.Videos)
        {
            vid.Type = VideoType.WatchLater;
        }

        context.Channels.AddRange(channels);

        context.SaveChanges();
    }
}