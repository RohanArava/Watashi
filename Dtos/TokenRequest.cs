using Microsoft.AspNetCore.Mvc;

namespace Watashi.Dtos;

public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; set; } = string.Empty;

    [FromForm(Name = "code")]
    public string Code { get; set; } = string.Empty;

    [FromForm(Name = "redirect_uri")]
    public string RedirectUri { get; set; } = string.Empty;

    [FromForm(Name = "client_id")]
    public string? ClientId { get; set; } // optional with Basic auth
}
