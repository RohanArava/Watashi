using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Watashi.Data;
using Watashi.Dtos;
using Watashi.Services;

namespace Watashi.Controllers;

[ApiController]
[Route("connect/token")]
public class TokenController(
    WatashiDbContext db,
    IConfiguration config,
    RsaKeyService rsaKeyService
) : ControllerBase
{
    private readonly WatashiDbContext _db = db;
    private readonly RsaKeyService _rsaKeyService = rsaKeyService;
    private readonly IConfiguration _config = config;

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (request.GrantType != "authorization_code")
            return BadRequest(new { error = "unsupported_grant_type" });

        var (clientId, clientSecret) = ParseBasicAuthHeader();
        if (clientId == null)
            return Unauthorized();

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
        if (client == null || client.ClientSecret != clientSecret)
            return Unauthorized();

        var code = await _db
            .AuthorizationCodes.Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Code == request.Code);

        Console.WriteLine(
            $"Code: {request.Code}, RedirectUri: {request.RedirectUri}, ClientId: {client.Id}"
        );
        Console.WriteLine(
            $"Found code: {code?.Code}, RedirectUri: {code?.RedirectUri}, ClientId: {code?.ClientId}"
        );

        if (code == null || code.RedirectUri != request.RedirectUri || code.ClientId != client.Id)
            return BadRequest(new { error = "invalid_grant" });

        if (code.ExpiresAt < DateTime.UtcNow || code.IsUsed)
            return BadRequest(new { error = "expired_grant" });

        if (code.User == null)
            return BadRequest(
                new
                {
                    error = "invalid_grant",
                    error_description = "User not found for the authorization code."
                }
            );

        _db.AuthorizationCodes.Remove(code);
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, code.User.Username),
            new("sub", code.User.Username),
        };

        var now = DateTime.UtcNow;
        var rsaKey = _rsaKeyService.GetKey();
        var creds = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(10),
            signingCredentials: creds
        );

        var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);

        var result = new Dictionary<string, object>
        {
            ["access_token"] = accessTokenStr,
            ["token_type"] = "Bearer",
            ["expires_in"] = 600
        };

        if (code.Scope?.Contains("openid") == true)
        {
            var idTokenClaims = new List<Claim>(claims);
            if (!string.IsNullOrEmpty(code.Nonce))
                idTokenClaims.Add(new Claim("nonce", code.Nonce));

            var idToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: client.ClientId,
                claims: idTokenClaims,
                notBefore: now,
                expires: now.AddMinutes(10),
                signingCredentials: creds
            );

            result["id_token"] = new JwtSecurityTokenHandler().WriteToken(idToken);
        }

        return Ok(result);
    }

    private (string? clientId, string? clientSecret) ParseBasicAuthHeader()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return (null, null);

        if (!header.ToString().StartsWith("Basic "))
            return (null, null);

        var encoded = header.ToString()["Basic ".Length..];
        var decodedBytes = Convert.FromBase64String(encoded);
        var decoded = Encoding.UTF8.GetString(decodedBytes);
        var parts = decoded.Split(':', 2);

        if (parts.Length != 2)
            return (null, null);

        return (parts[0], parts[1]);
    }
}
