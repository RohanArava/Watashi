namespace Watashi.ViewModels;

public class ConsentViewModel
{
    public string ClientId { get; set; } = null!;
    public string? ClientName { get; set; }
    public string RedirectUri { get; set; } = null!;
    public string Scope { get; set; } = null!;
    public string? State { get; set; }
}
