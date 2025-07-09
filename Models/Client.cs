using System.ComponentModel.DataAnnotations;
using Watashi.Models.ClientInfo;

namespace Watashi.Models;

public class Client
{
    [Key]
    public Guid Id { get; set; }
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    public string ClientId { get; set; } = Guid.NewGuid().ToString("N");

    public string? ClientSecret { get; set; }

    public string? ClientName { get; set; }
    public string? ClientUri { get; set; }
    public string? LogoUri { get; set; }

    public string TokenEndpointAuthMethod { get; set; } = "client_secret_basic";

    public string? Scope { get; set; }

    public string? JwksUri { get; set; }
    public string? Jwks { get; set; }

    public string? TosUri { get; set; }

    public string? PolicyUri { get; set; }

    public ICollection<RedirectUri> RedirectUris { get; set; } = [];
    public ICollection<GrantType> GrantTypes { get; set; } = [];
    public ICollection<ResponseType> ResponseTypes { get; set; } = [];
}
