using youtube_feed_asp.Models;

namespace youtube_feed_asp.Data;

public static class DbInitializer
{
    public static void Initialize(VideoContext context)
    {
        if (context.Videos.Any() && context.Channels.Any())
            return;

        var video = new Video {
            Id = "3C9zpM4NaHQ",
            Title = "Realtime Spoken Encryption using Toki Pona (Toki Pona VR)",
            TimePublished = 100,
            TimeAdded = 1000000,
            Type = VideoType.WatchLater
        };

        var video2 = new Video {
            Id = "3kjsdfhglkjshdflkjgC9zpM4NaHQ",
            Title = "fake youtube video",
            TimePublished = 10000,
            TimeAdded = 1000000,
            Type = VideoType.Subscription
        };

        var channel = new Channel {
            Id = "fuopy",
            Name = "fuopy",
            LastModified = 1000000
        };

        video.Uploader = channel;

        channel.Videos = new List<Video>();
        channel.Videos.Add(video);
        channel.Videos.Add(video2);

        context.Videos.Add(video);
        context.SaveChanges();
    }
}