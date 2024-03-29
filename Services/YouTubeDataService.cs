using youtube_feed_asp.Models;
using youtube_feed_asp.Data;
using youtube_feed_asp.Enums;
using youtube_feed_asp.Helpers;
using youtube_feed_asp.Scrapers;
using Microsoft.EntityFrameworkCore;

namespace youtube_feed_asp.Services;

public class YouTubeDataService
{
    private readonly YouTubeDataContext m_context;

    public YouTubeDataService(YouTubeDataContext context)
    {
        m_context = context;
    }

    #region Helper Methods
    private static List<Video> SortVideosBy(List<Video> list, SortType sortType)
    {
        return sortType switch
        {
            SortType.DateAscending => list.OrderBy(video => video.TimePublished).ToList(),
            SortType.DateDescending => list.OrderByDescending(video => video.TimePublished).ToList(),
            SortType.AddedAscending => list.OrderBy(video => video.TimeAdded).ToList(),
            SortType.AddedDescending => list.OrderByDescending(video => video.TimeAdded).ToList(),
            SortType.Channel => list.OrderBy(video => video.Uploader.ChannelId).ToList(),
            _ => list,
        };
    }
    #endregion

    //==============================================================================
    // Database CRUD Operations
    //==============================================================================

    // TODO: a read method for a single video would be nice to have

    #region C: Subscribe to Channel
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
        {
            Console.WriteLine($"Unable to subscribe to channel {channelId}: the channel is already subscribed to.");
            return false;
        }

        var parseResult = ChannelScraper.Scrape($"https://www.youtube.com/channel/{channelId}");
        // TODO: this existence check should likely be the first check in the function
        if (parseResult is null)
            return false;

        var res = (ChannelScraper.Result)parseResult;
        ch = new Channel
        {
            ChannelId = channelId,
            Name = res.Name,
            LastModified = 0,
            Videos = new List<Video>()
        };

        m_context.Channels.Add(ch);
        m_context.SaveChanges();

        Console.WriteLine($"Successfully subscribed to channel {channelId}.");
        return true;
    }
    #endregion

    #region R: Get Channels
    public List<Channel> GetAllChannels(VideoType videoType, SortType sortType)
    {
        var channels = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .ToList();

        foreach (var ch in channels)
        {
            ch.Videos = ch.Videos
                .Where(video => video.Type == videoType)
                .ToList();
        }

        return channels;
    }

    public Channel? GetChannel(VideoType videoType, string channelId, SortType sortType)
    {
        var ch = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .SingleOrDefault(channel => channel.ChannelId == channelId);

        if (ch is not null)
            ch.Videos = ch.Videos.Where(video => video.Type == videoType).ToList();

        return ch;
    }
    #endregion

    #region R: Get Videos
    public List<Video> GetAllVideos(VideoType videoType, SortType sortType)
    {
        var res = m_context.Videos
            .Include(video => video.Uploader)
            .AsNoTracking()
            .Where(video => video.Type == videoType)
            .ToList();
        return SortVideosBy(res, sortType);
    }

    public List<Video>? GetChannelVideos(VideoType videoType, string channelId, SortType sortType)
    {
        var ch = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .SingleOrDefault(channel => channel.ChannelId == channelId);

        if (ch is null)
        {
            Console.WriteLine($"Channel ID {channelId} not found.");
            return null;
        }

        var filteredVideos = ch.Videos
            .Where(video => video.Type == videoType)
            .ToList();
        return SortVideosBy(filteredVideos, sortType);
    }
    #endregion

    #region U: Update Channel Subscriptions
    public bool UpdateAllChannelSubscriptions()
    {
        IEnumerable<Channel> allChannels = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking();

        foreach (var ch in allChannels)
            UpdateChannelSubscriptions(ch);
        return true;
    }

    public bool UpdateChannelSubscriptions(string channelId)
    {
        var ch = m_context.Channels
            .Include(channel => channel.Videos)
            .AsNoTracking()
            .SingleOrDefault(channel => channel.ChannelId == channelId);

        if (ch is null)
        {
            Console.WriteLine($"Could not update subscriptions for channel {channelId} because it does not exist in the database.");
            return false;
        }

        UpdateChannelSubscriptions(ch);
        Console.WriteLine($"Successfully updated subscriptions for channel {channelId}.");
        return true;
    }

    // TODO: need to make sure this doesn't block super hard when waiting for the HTTP requests
    private void UpdateChannelSubscriptions(Channel ch)
    {
        // update subscriptions from the RSS feed
        List<RssScraper.Result> rssVideos = RssScraper.Scrape(ch.ChannelId);

        // discard videos that we have already parsed in the past
        Console.WriteLine($"Last modified at {ch.LastModified}");
        rssVideos = rssVideos.Where(rssVideo => rssVideo.TimePublished > ch.LastModified).ToList();

        var newVideos = new List<Video>();

        foreach (var rssResponse in rssVideos)
        {
            Console.WriteLine($"    uploaded={rssResponse.TimePublished}");
            // TODO: needs better null checking
            var videoResponse = VideoScraper.Scrape(rssResponse.VideoId);

            // TODO: document the meaning of a video length of -1 seconds
            newVideos.Add(new Video()
            {
                VideoId = rssResponse.VideoId,
                Uploader = ch,
                Title = rssResponse.Title,
                TimePublished = rssResponse.TimePublished,
                TimeAdded = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Type = VideoType.Subscription,
                LengthSeconds = videoResponse?.LengthSeconds ?? -1
            });

            // rssVideos should be in the correct order for the assignment to be correct,
            // but here's a sanity check anyway
            if (ch.LastModified < rssResponse.TimePublished)
                ch.LastModified = rssResponse.TimePublished;
        }

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
    }
    #endregion

    #region D: Unsubscribe from Channel
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
        {
            Console.WriteLine($"Unable to unsubscribe from channel {channelId}: the channel does not exist in the database.");
            return false;
        }

        // remove all subscriptions from this channel from the database
        var videos = ch.Videos.Where(v => v.Type == VideoType.Subscription);
        m_context.Videos.RemoveRange(videos);
        m_context.Channels.Remove(ch);
        m_context.SaveChanges();

        Console.WriteLine($"Successfully unsubscribed from channel {channelId} and removed all subscriptions related to the channel.");
        return true;
    }
    #endregion

    #region D: Delete Video
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
        if (!videos.Any())
        {
            Console.WriteLine($"Could not delete video {id} from the database because it does not exist in the database.");
            return false;
        }

        m_context.Videos.RemoveRange(videos);
        m_context.SaveChanges();

        Console.WriteLine($"Successfully deleted video {id} from the database.");
        return true;
    }
    #endregion
}