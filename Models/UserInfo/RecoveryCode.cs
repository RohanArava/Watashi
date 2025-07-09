using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watashi.Models.UserInfo;

public class RecoveryCode
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Code { get; set; } = string.Empty;

    public bool IsUsed { get; set; } = false;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
