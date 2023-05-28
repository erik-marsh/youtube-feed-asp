using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using youtube_feed_asp.Enums;

namespace youtube_feed_asp.Models;

public class Video
{
    /// <summary>
    /// Record ID (to ensure uniqueness).
    /// </summary>
    /// <remarks>
    /// <para>The ASP.net EF Core implementation will auto-increment this value by default.</para>
    /// <para>In a previous version of this software, I used the video ID as the primary key.
    /// This, in theory, would work, but I eventually ran into situations where the video ID was not unique.
    /// I'm not entirely sure why this is the case, but I suspect it has something to do with the videos being
    /// edited after an initial upload or a conflict with YouTube's premiere system.
    /// I don't think reuploads would cause it, since they would probably have a new video ID.
    /// Regardless, this field will probably prove to be more necessary over time,
    /// since (as far as I can tell) the RSS feed system YouTube provides is deprecated.</para>
    /// </remarks>
    [Key, Required]
    public int Id { get; set; }

    /// <summary>
    /// The Base64 ID for the video.
    /// </summary>
    [Required]
    public string VideoId { get; set; } = "";

    /// <summary>
    /// The channel that the video was uploaded by.
    /// </summary>
    [Required]
    [JsonIgnore]
    public Channel Uploader { get; set; } = new();

    /// <summary>
    /// The title of the video.
    /// </summary>
    /// <remarks>
    /// This will not track changes in the title of the video.
    /// </remarks>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>
    /// Unix timestamp representing the time that the video was uploaded.
    /// </summary>
    [Required]
    public long TimePublished { get; set; }

    /// <summary>
    /// Unix timestamp representing the time that the video was added to this database.
    /// </summary>
    [Required]
    public long TimeAdded { get; set; }

    /// <summary>
    /// The type of the video, used to differentiate between different video feeds provided by this software.
    /// </summary>
    [Required, Column(TypeName = "varchar(30)")]
    public VideoType Type { get; set; }

    /// <summary>
    /// The duration of the video, in seconds.
    /// </summary>
    [Required]
    public long LengthSeconds { get; set; }

    /// <summary>
    /// Returns the URL that points to the video.
    /// </summary>
    [NotMapped]
    public string Url => $"https://www.youtube.com/watch?v={VideoId}";

    [NotMapped]
    public string ThumbnailUrl => $"https://i.ytimg.com/vi/{VideoId}/maxresdefault.jpg";

    /// <summary>
    /// Returns a human-readable form of the upload time.
    /// </summary>
    public string GetReadablePublishingTime()
    {
        var published = DateTimeOffset.FromUnixTimeSeconds(TimePublished);
        return published.ToString("h:mm tt M/d/yyyy");
    }

    /// <summary>
    /// Returns a human-readable form of the added time.
    /// </summary>
    public string GetReadableAddedTime()
    {
        var added = DateTimeOffset.FromUnixTimeSeconds(TimeAdded);
        return added.ToString("h:mm tt M/d/yyyy");
    }
}