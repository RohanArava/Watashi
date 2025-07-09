using System.ComponentModel.DataAnnotations;
using Watashi.Models.UserInfo;

namespace Watashi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string TotpSecret { get; set; } = string.Empty;

    public ICollection<RecoveryCode> RecoveryCodes { get; set; } = [];

    [MaxLength(128)]
    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
