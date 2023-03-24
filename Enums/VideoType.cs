namespace youtube_feed_asp.Enums;

/// <summary>
/// Differentiates between different categories of videos.
/// This avoids having to use separate tables to differentiate the categories.
/// </summary>
public enum VideoType
{
    Subscription = 0,
    WatchLater = 1
}