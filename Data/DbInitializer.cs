using youtube_feed_asp.Models;
using youtube_feed_asp.VideoScraper;

namespace youtube_feed_asp.Data;

public static class DbInitializer
{
    public static void Initialize(VideoContext context)
    {
        InitializeWithOnlyChannels(context);
    }

    public static void InitializeWithOnlyChannels(VideoContext context)
    {
        var channels = new List<Channel>();

        channels.Add(new Channel {
            ChannelId = "UCqms1E3UhFpC0RRxZQNF-ag",
            Name = "fuopy",
            LastModified = 0
        });

        channels.Add(new Channel {
            ChannelId = "UCSPLhwvj0gBufjDRzSQb3GQ",
            Name = "BobbyBroccoli",
            LastModified = 0,
            Videos = new List<Video>()
        });

        channels.Add(new Channel {
            ChannelId = "UCLrno_gfh32wX4I8-6qV4Wg",
            Name = "Running Shine",
            LastModified = 0,
            Videos = new List<Video>()
        });

        channels.Add(new Channel {
            ChannelId = "UCBn6YYmhnruGtEtlKxYbQ9A",
            Name = "K Klein",
            LastModified = 0,
            Videos = new List<Video>()
        });

        var test = new Channel {
            ChannelId = "UCuvSqzfO_LV_QzHdmEj84SQ",
            Name = "watchlater",
            LastModified = 0,
            Videos = new List<Video>()
        };

        channels.Add(test);

        context.Channels.AddRange(channels);

        foreach (var ch in channels)
        {
            var newVideos = VideoScraper.VideoScraper.UpdateChannelSubscriptions(ch);
            context.Videos.AttachRange(newVideos);
            context.Entry(ch)
                .Property(x => x.LastModified)
                .IsModified = true;
        }

        foreach (var vid in test.Videos)
        {
            vid.Type = VideoType.WatchLater;
        }

        context.SaveChanges();
    }
}