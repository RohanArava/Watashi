namespace Watashi.ViewModels;

public class RecoverySuccessViewModel
{
    public string Username { get; set; } = string.Empty;
    public List<string> NewRecoveryCodes { get; set; } = [];
    public string QrCodeImageUrl { get; set; } = string.Empty;

    public string TotpSecret { get; set; } = string.Empty;
}
