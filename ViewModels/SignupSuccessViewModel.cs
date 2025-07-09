namespace Watashi.ViewModels;

public class SignupSuccessViewModel
{
    public string Username { get; set; } = string.Empty;
    public string TotpSecret { get; set; } = string.Empty;
    public string QrCodeImageUrl { get; set; } = string.Empty;
    public List<string> RecoveryCodes { get; set; } = [];
}
