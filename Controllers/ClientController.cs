using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Watashi.Data;
using Watashi.Models;

namespace Watashi.Controllers;

[ApiController]
[Route("connect/register")]
public class ClientController(WatashiDbContext db) : ControllerBase
{
    private readonly WatashiDbContext _db = db;

    private async Task<Client?> AuthenticateClientAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            return null;

        var apiKey = apiKeyHeader.ToString();

        Console.WriteLine($"Authenticating client with API key: {apiKey}");

        if (string.IsNullOrEmpty(apiKey))
            return null;

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ApiKey == apiKey);
        Console.WriteLine($"Client found: {client?.ClientName ?? "None"}");

        return client;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] Client client)
    {
        if (client.RedirectUris == null || client.RedirectUris.Count == 0)
            return BadRequest("redirect_uris is required.");

        if (client.TokenEndpointAuthMethod != "none")
            client.ClientSecret = Guid.NewGuid().ToString("N");

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = client.Id },
            new
            {
                client.ClientId,
                client.ClientSecret,
                client.ApiKey,
                client.ClientName,
                client.ClientUri,
                client.LogoUri,
                client.Scope,
                RedirectUris = client.RedirectUris.Select(r => r.Uri),
                GrantTypes = client.GrantTypes.Select(g => g.Value),
                ResponseTypes = client.ResponseTypes.Select(r => r.Value)
            }
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _db
            .Clients.Select(c => new
            {
                c.ClientId,
                c.ClientName,
                c.ClientUri,
                c.LogoUri
            })
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = id.ToString("N");
        var client = await AuthenticateClientAsync();
        if (client == null || client.ClientId != clientId)
        {
            return Unauthorized();
        }

        var fullClient = await _db
            .Clients.Include(c => c.RedirectUris)
            .Include(c => c.GrantTypes)
            .Include(c => c.ResponseTypes)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (fullClient == null)
            return NotFound();

        return Ok(
            new
            {
                fullClient.ClientId,
                fullClient.ClientSecret,
                fullClient.ApiKey,
                fullClient.ClientName,
                fullClient.ClientUri,
                fullClient.LogoUri,
                fullClient.Scope,
                RedirectUris = fullClient.RedirectUris.Select(r => r.Uri),
                GrantTypes = fullClient.GrantTypes.Select(g => g.Value),
                ResponseTypes = fullClient.ResponseTypes.Select(r => r.Value)
            }
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Client updated)
    {
        var clientId = id.ToString("N");
        var client = await AuthenticateClientAsync();
        if (client == null || client.ClientId != clientId)
            return Unauthorized();

        var existing = await _db
            .Clients.Include(c => c.RedirectUris)
            .Include(c => c.GrantTypes)
            .Include(c => c.ResponseTypes)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (existing == null)
            return NotFound();

        _db.RedirectUris.RemoveRange(existing.RedirectUris);
        _db.GrantTypes.RemoveRange(existing.GrantTypes);
        _db.ResponseTypes.RemoveRange(existing.ResponseTypes);

        existing.ClientName = updated.ClientName;
        existing.ClientUri = updated.ClientUri;
        existing.LogoUri = updated.LogoUri;
        existing.Scope = updated.Scope;
        existing.Jwks = updated.Jwks;
        existing.JwksUri = updated.JwksUri;
        existing.TokenEndpointAuthMethod = updated.TokenEndpointAuthMethod;

        existing.RedirectUris = updated.RedirectUris;
        existing.GrantTypes = updated.GrantTypes;
        existing.ResponseTypes = updated.ResponseTypes;

        await _db.SaveChangesAsync();
        return Ok(
            new
            {
                existing.ClientId,
                existing.ClientSecret,
                existing.ApiKey,
                existing.ClientName,
                existing.ClientUri,
                existing.LogoUri,
                existing.Scope,
                RedirectUris = existing.RedirectUris.Select(r => r.Uri),
                GrantTypes = existing.GrantTypes.Select(g => g.Value),
                ResponseTypes = existing.ResponseTypes.Select(r => r.Value)
            }
        );
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = id.ToString("N");
        var clientCheck = await AuthenticateClientAsync();
        if (clientCheck == null || clientCheck.ClientId != clientId)
            return Unauthorized();

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
        if (client == null)
            return NotFound();

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
