using System.ComponentModel.DataAnnotations;

namespace Watashi.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty; // TOTP or recovery
}
