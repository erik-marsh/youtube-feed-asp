using youtube_feed_asp.Enums;

namespace youtube_feed_asp.Helpers;

public static class Parsing
{
    /// <summary>
    /// Parses a video type from a string.
    /// </summary>
    /// <returns>
    /// The corresponding video type if it exists.
    /// Returns null otherwise.
    /// </returns>
    public static VideoType? ParseVideoType(string videoType) => videoType switch
    {
        "subscriptions" => VideoType.Subscription,
        "watch-later" => VideoType.WatchLater,
        _ => null
    };

    /// <summary>
    /// Parses a sort type from a string.
    /// </summary>
    /// <returns>
    /// The corresponding sort type if it exists.
    /// Returns null otherwise.
    /// </returns>
    public static SortType? ParseSortType(string sortType) => sortType switch
    {
        "date-ascending" => SortType.DateAscending,
        "date-descending" => SortType.DateDescending,
        "added-ascending" => SortType.AddedAscending,
        "added-descending" => SortType.AddedDescending,
        "channel" => SortType.Channel,
        _ => null
    };
}