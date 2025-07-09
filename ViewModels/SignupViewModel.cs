using System.ComponentModel.DataAnnotations;

namespace Watashi.ViewModels;

public class SignupViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}
