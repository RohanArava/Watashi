using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watashi.Models.ClientInfo;

public class RedirectUri
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Uri { get; set; } = null!;

    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public Client? Client { get; set; }
}
