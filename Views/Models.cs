// TODO: this file is poorly named

using youtube_feed_asp.Models;
using youtube_feed_asp.Enums;

namespace youtube_feed_asp.Views;

public class VideoModel
{
    public VideoModel(List<Video> videos, VideoType type)
    {
        Videos = videos;
        Type = type;
    }

    public List<Video> Videos { get; }
    public VideoType Type { get; }
}

public class ChannelModel
{
    public ChannelModel(List<Channel> channels, VideoType type)
    {
        Channels = channels;
        Type = type;
    }

    public List<Channel> Channels { get; }
    public VideoType Type { get; }
}