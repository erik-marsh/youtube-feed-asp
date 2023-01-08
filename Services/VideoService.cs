using youtube_feed_asp.Models;
using youtube_feed_asp.Data;
using youtube_feed_asp.VideoScraper;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace youtube_feed_asp.Services;

public enum SortType
{
    DateAscending,
    DateDescending,
    AddedAscending,
    AddedDescending,
    Channel,
    None
}

public class VideoService
{
    private const string s_rssBaseUrl = "https://www.youtube.com/feeds/videos.xml?channel_id=";

    private readonly VideoContext m_context;

    public VideoService(VideoContext context)
    {
        m_context = context;
    }


    private static List<Video> SortVideosBy(List<Video> list, SortType sortType)
    {
        switch (sortType)
        {
        case SortType.DateAscending:
            return list.OrderBy(video => video.TimePublished).ToList();
        case SortType.DateDescending:
            return list.OrderByDescending(video => video.TimePublished).ToList();
        case SortType.AddedAscending:
            return list.OrderBy(video => video.TimeAdded).ToList();
        case SortType.AddedDescending:
            return list.OrderByDescending(video => video.TimeAdded).ToList();
        case SortType.Channel:
            return list.OrderBy(video => video.Uploader.ChannelId).ToList();
        case SortType.None:
        default:
            return list;
        }
    }

    public List<Video>? VideoQuery(string videoType, string channelId, string sortType)
    {
        VideoType? parsedVideoType = videoType switch {
            "subscriptions" => VideoType.Subscription,
            "watch-later" => VideoType.WatchLater,
            _ => null
        };

        SortType? parsedSortType = sortType switch {
            "date-ascending" => SortType.DateAscending,
            "date-descending" => SortType.DateDescending,
            "added-ascending" => SortType.AddedAscending,
            "added-descending" => SortType.AddedDescending,
            "channel" => SortType.Channel,
            _ => null 
        };

        Console.WriteLine($"VideoType == {parsedVideoType.ToString()}");
        Console.WriteLine($"SortType == {parsedSortType.ToString()}");

        if (parsedSortType is null || parsedVideoType is null)
            return null;
        
        // TODO: there has to be a better way
        VideoType type = (VideoType) parsedVideoType;
        SortType sort = (SortType) parsedSortType;

        return VideoQuery(type, channelId, sort);
    }

    public List<Video>? VideoQuery(VideoType videoType, string channelId, SortType sortType)
    {
        var videos = new List<Video>();

        if (channelId == "all")
        {
            Console.WriteLine("parsing all channels");
            videos.AddRange(QueryAllChannels(videoType, sortType));
        }
        else
        {
            Console.WriteLine($"attempting to parse regular channel {channelId}");
            try
            {
                videos.AddRange(QueryChannel(channelId, videoType, sortType));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        return videos;
    }

    private List<Video> QueryAllChannels(VideoType videoType, SortType sortType)
    {
        var videos = m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .Where(video => video.Type == videoType)
            .ToList();

        return SortVideosBy(videos, sortType);
    }

    private List<Video> QueryChannel(string channelId, VideoType videoType, SortType sortType)
    {
        var ch = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .SingleOrDefault(channel => channel.ChannelId == channelId);

        if (ch is null)
            throw new ArgumentException($"Channel ID {channelId} not found.");
        
        var filteredVideos = ch.Videos
            .Where(video => video.Type == videoType)
            .ToList();
        return SortVideosBy(filteredVideos, sortType);
    }

    public List<Channel>? ChannelQuery(VideoType videoType, string channelId, SortType sortType)
    {
        var channels = new List<Channel>();
        if (channelId == "all")
        {
            var res = m_context.Channels
                .Include(channel => channel.Videos)
                .AsNoTracking();
            
            channels.AddRange(res);
        }
        else
        {
            var ch = m_context.Channels
                .Include(channel => channel.Videos)
                .AsNoTracking()
                .SingleOrDefault(channel => channel.ChannelId == channelId);
            
            if (ch is null)
                return null;
            
            channels.Add(ch);
        }

        foreach (var ch in channels)
        {
            ch.Videos = ch.Videos.Where(video => video.Type == videoType).ToList();
        }

        return channels;
    }

    /// <summary>
    /// Adds a channel to the database.
    /// </summary>
    /// <remarks>
    /// Does not add any videos uploaded by the channel to the database.
    /// </remarks>
    /// <param name="channelId">A Base64 YouTube canonical channel ID.</param>
    /// <returns>
    /// If the channel already exists in the database
    /// OR the channel ID does not correspond to a channel, returns false.
    /// Otherwise, the channel is added to the database and this function returns true.
    /// </returns>
    public bool SubscribeToChannel(string channelId)
    {
        var ch = m_context.Channels.SingleOrDefault(channel => channel.ChannelId == channelId);
        if (ch is not null)
            return false;

        ch = VideoScraper.VideoScraper.GetChannelFromID(channelId);
        if (ch is null)
            return false;
        
        m_context.Channels.Add(ch);
        m_context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Removes a channel and all of it's associated videos tagged with VideoType.Subscription from the database.
    /// </summary>
    /// <param name="channelId">A Base64 YouTube canonical channel ID.</param>
    /// <returns>
    /// If the channel is not in the database, returns false.
    /// Otherwise, removes the channel and it's videos and returns true.
    /// </returns>
    public bool UnsubscribeFromChannel(string channelId)
    {
        var ch = m_context.Channels
            .Include(channel => channel.Videos)
            .SingleOrDefault(channel => channel.ChannelId == channelId);

        if (ch is null)
            return false;

        // remove all subscriptions from this channel from the database
        var videos = ch.Videos.Where(v => v.Type == VideoType.Subscription);
        m_context.Videos.RemoveRange(videos);
        m_context.Channels.Remove(ch);
        m_context.SaveChanges();
        return true;
    }

    //==============================================================================
    // Video Database Methods
    //==============================================================================

    /// <summary>
    /// Returns every video in the video database.
    /// </summary>
    public IEnumerable<Video> GetAllVideos()
    {
        return m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .ToList();
    }

    /// <summary>
    /// Returns a video in the database with the given YouTube video id,
    /// or null if the video is not in the database.
    /// </summary>
    /// <param name="id">The ID of the YouTube video as given in its URL.</param>
    public Video? GetVideo(string id)
    {
        return m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .SingleOrDefault(video => video.VideoId == id);
    }

    /// <summary>
    /// Gets every video in the database with a given VideoType.
    /// </summary>
    public IEnumerable<Video> GetAllVideosByType(VideoType type)
    {
        return m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .Where(video => video.Type == type);
    }


    // TODO: return bool for success? return the video added?
    /// <summary>
    /// Adds a video to the database.
    /// </summary>
    /// <param name="video">The video object to add to the database.</param>
    /// <remarks>
    /// The type of the video should be included in the Video object itself.
    /// The video is assumed to be linked to the proper Channel object before being added.
    /// </remarks>
    public void AddVideo(Video video)
    {
        m_context.Videos.Add(video);
        m_context.SaveChanges();
    }

    /// <summary>
    /// Adds a list of videos to the database.
    /// </summary>
    /// <param name="videos">The list of videos to add to the database.</param>
    /// <remarks>
    /// The types of the videos should be included in the Video objects themselves.
    /// The videos are assumed to be linked to the proper Channel objects before being added.
    /// </remarks>
    public void AddVideos(List<Video> videos)
    {
        m_context.Videos.AddRange(videos);
        m_context.SaveChanges();
    }

    /// <summary>
    /// Removes a video from the database.
    /// </summary>
    /// <param name="id">The YouTube ID of the video to remove from the database.</param>
    /// <remarks>
    /// If the ID does not correspond to a video in the database, this function does nothing.
    /// </remarks>
    /// <returns>
    /// If the video is not in the database, returns false.
    /// If the video is in the database, deletes the video from the database and returns true.
    /// </returns>
    public bool DeleteVideo(string id)
    {
        // TODO: do i also need to remove the video from its associated channels?
        // yes, this is intentionally an IEnumerable
        // in the documentation for Video, I note that there is weridness
        // with YouTube premieres/etc that make video IDs non-unique
        var videos = m_context.Videos.Where(video => video.VideoId == id);
        if (videos.Count() == 0)
            return false;

        m_context.Videos.RemoveRange(videos);
        m_context.SaveChanges();
        return true;
    }


    //==============================================================================
    // Channel Database Methods
    //==============================================================================

    /// <summary>
    /// Returns a channel from the database.
    /// </summary>
    /// <param name="id">The canonical YouTube ID of the channel (a Base64 string starting with "UC").</param>
    public Channel? GetChannel(string id)
    {
        return m_context.Channels
            .Include(channel => channel.Videos) // TODO: group by subs and watch later
            .AsNoTracking()
            .SingleOrDefault(channel => channel.ChannelId == id);
    }

    // TODO: probably want methods that return this list with and without videos
    /// <summary>
    /// Returns every channel in the database.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Channel> GetAllChannels()
    {
        return m_context.Channels
            .Include(channel => channel.Videos) // TODO: group by subs and watch later
            .AsNoTracking()
            .ToList();
    }

    /// <summary>
    /// Gets a list of every video of a specified type associated with the given channel ID.
    /// </summary>
    /// <param name="id">The canonical YouTube ID of the channel (a Base64 string starting with "UC").</param>
    /// <param name="type">The VideoType of the videos.</param>
    public IEnumerable<Video> GetChannelVideosByType(string id, VideoType type)
    {
        var channel = GetChannel(id);
        if (channel is null || channel.Videos is null)
            return new List<Video>(); // empty list
        
        return channel.Videos.Where(video => video.Type == type);
    }

    // note: channels are assumed to have their video references set up properly prior to adding
    /// <summary>
    /// Adds a channel to the database.
    /// </summary>
    /// <remarks>
    /// The Channel is assumed to already have a list of videos
    /// that were in the database <i>a priori</i> associated with it.
    /// </remarks>
    public void AddChannel(Channel channel)
    {
        m_context.Channels.Add(channel);
        m_context.SaveChanges();
    }

    /// <summary>
    /// Adds a list of channels to the database.
    /// </summary>
    /// <remarks>
    /// The Channels are assumed to already have a list of videos
    /// that were in the database <i>a priori</i> associated with them.
    /// </remarks>
    public void AddChannels(List<Channel> channels)
    {
        m_context.Channels.AddRange(channels);
        m_context.SaveChanges();
    }

    /// <summary>
    /// Deletes a channel from the database and every video associated with it.
    /// </summary>
    /// <param name="id">The canonical YouTube ID of the channel (a Base64 string starting with "UC").</param>
    public void DeleteChannelWithVideos(string id)
    {
        var channel = m_context.Channels
            .Include(channel => channel.Videos)
            .SingleOrDefault(channel => channel.ChannelId == id);
        
        // TODO: what if videos get orphaned (reference a deleted channel)?
        if (channel is null)
            return;

        m_context.Videos.RemoveRange(channel.Videos); // shouldn't be null, so we let it throw if it is
        m_context.Channels.Remove(channel);
    }

    // TODO: need to make sure this doesn't block super hard when waiting for the HTTP request for the feed to resolve
    public void UpdateChannelSubscriptions(Channel ch)
    {
        var newVideos = VideoScraper.VideoScraper.UpdateChannelSubscriptions(ch);
        if (newVideos.Count == 0) return;

        var databaseWriteTimer = new Stopwatch();
        databaseWriteTimer.Start();

        // see https://stackoverflow.com/questions/66017750/ef-core-3-1-re-inserting-existing-navigational-property-when-adding-new-entity
        // if we did AddRange, it would attempt to re-insert the Channel as well (because it was marked as Added)
        // TL;DR there is Microsoft Magic happening in the background that wants to reinsert the channel
        // however, Attach only marks things as Added if their primary key is unset. Otherwise, they are marked as unchanged.
        m_context.Videos.AttachRange(newVideos);

        // BUT!
        // since the channel was marked as unchanged, it would not update the channel's LastModified value.
        // this is bad, since our whole update mechanism depends on this value.
        // fortunately, we can manually set the property as being changed with this line
        // the implementation will perform an UPDATE query and not attempt to reinsert the value
        // relevant SO link: https://stackoverflow.com/questions/3642371/how-to-update-only-one-field-using-entity-framework
        m_context.Entry(ch)
            .Property(x => x.LastModified)
            .IsModified = true;
        m_context.SaveChanges();

        databaseWriteTimer.Stop();
        Console.WriteLine($"Wrote to database in {databaseWriteTimer.ElapsedMilliseconds}ms");
    }
}