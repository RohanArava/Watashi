using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watashi.Models;

public class AuthorizationCode
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Code { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    public string RedirectUri { get; set; } = null!;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

    public bool IsUsed { get; set; } = false;

    public string Scope { get; set; } = null!;

    public string? Nonce { get; set; }
}
