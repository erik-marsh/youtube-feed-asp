using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace youtube_feed_asp.Models;

public class Channel
{
    /// <summary>
    /// Record ID (to ensure uniqueness).
    /// </summary>
    /// <remarks>
    /// The ASP.net EF Core implementation will auto-increment this value by default.
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// The Base64 canonical identifier of the YouTube channel.
    /// </summary>
    /// <remarks>
    /// Every YouTube channel has a canonical ID, but some channels have custom URLs that do not use this ID.
    /// It still exists, however, and is hidden within the HTML source of the channel page.
    /// </remarks>
    [Required]
    public string? ChannelId { get; set; }

    /// <summary>
    /// The name of the channel.
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// Unix timestamp representing the last time the channel uploaded a video.
    /// </summary>
    [Required]
    public int LastModified { get; set; }
 
    /// <summary>
    /// A list of videos associated with the channel.
    /// </summary>
    public ICollection<Video>? Videos { get; set; }
}