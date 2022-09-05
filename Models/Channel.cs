using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace youtube_feed_asp.Models;

public class Channel
{
    [Required]
    public string? Id { get; set; }

    [Required]
    public string? Name { get; set; }

    // Unix Timestamp
    [Required]
    public int LastModified { get; set; }
 
    public ICollection<Video>? Videos { get; set; }
}