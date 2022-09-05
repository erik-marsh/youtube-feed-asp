using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace youtube_feed_asp.Models;

public enum VideoType
{
    Subscription = 0,
    WatchLater = 1
}

public class Video
{
    [Required] // redundant but still
    public string? Id { get; set; }

    [Required]
    [JsonIgnore]
    public Channel? Uploader { get; set; }

    [Required]
    public string? Title { get; set; }

    // Unix Timestamp
    [Required]
    public int TimePublished { get; set; }

    // Unix Timestamp
    [Required]
    public int TimeAdded { get; set; }

    [Column(TypeName = "varchar(30)")]
    public VideoType? Type { get; set; }
}