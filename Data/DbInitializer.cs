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

        //UCSPLhwvj0gBufjDRzSQb3GQ|BobbyBroccoli|2022-07-22 12:00:51+00:00
        var channel2 = new Channel {
            Id = "UCSPLhwvj0gBufjDRzSQb3GQ",
            Name = "BobbyBroccoli",
            LastModified = 123476372,
            Videos = new List<Video>() {

            }
        };

        var channel3 = new Channel {
            Id = "UCLrno_gfh32wX4I8-6qV4Wg",
            Name = "Running Shine",
            LastModified = 1234763722,
            Videos = new List<Video>()
        };

        var video30 = new Video {
            Id = "LSKdQhrcbuw",
            Uploader = channel3,
            Title = "Kirby 64: The Crystal Shards Review",
            TimePublished = 1928374,
            TimeAdded = 92187398,
            Type = VideoType.Subscription
        };

        channel3.Videos.Add(video30);

        //UCBn6YYmhnruGtEtlKxYbQ9A|K Klein|2022-08-26 10:00:01+00:00
        var channel4 = new Channel {
            Id = "UCBn6YYmhnruGtEtlKxYbQ9A",
            Name = "K Klein",
            LastModified = 1234722672,
            Videos = new List<Video>() {

            }
        };

        video.Uploader = channel;

        channel.Videos = new List<Video>();
        channel.Videos.Add(video);
        channel.Videos.Add(video2);

        context.Videos.Add(video);
        context.Videos.Add(video2);
        context.Videos.Add(video30);

        context.Channels.Add(channel);
        context.Channels.Add(channel2);
        context.Channels.Add(channel3);
        context.Channels.Add(channel4);
        context.SaveChanges();
    }
}