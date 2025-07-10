using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Watashi.Data;
using Watashi.Models;
using Watashi.ViewModels;

namespace Watashi.Controllers;

[Route("connect/authorize")]
public class AuthorizationController(WatashiDbContext db) : Controller
{
    private readonly WatashiDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> Authorize(
        [FromQuery] string response_type,
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string scope,
        [FromQuery] string state
    )
    {
        Console.WriteLine("client_id: " + client_id);
        Console.WriteLine("redirect_uri: " + redirect_uri);
        var client = await _db
            .Clients.Include(c => c.RedirectUris)
            .FirstOrDefaultAsync(c => c.ClientId == client_id);
        if (client == null)
            return BadRequest("Invalid client_id");

        if (!client.RedirectUris.Any(uri => uri.Uri == redirect_uri))
            return BadRequest("Invalid redirect_uri");

        if (response_type != "code")
            return BadRequest("Unsupported response_type");

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            TempData["ReturnUrl"] = Request.Path + Request.QueryString;
            return RedirectToAction("Login", "User");
        }

        var model = new ConsentViewModel
        {
            ClientId = client_id,
            ClientName = client.ClientName,
            RedirectUri = redirect_uri,
            Scope = scope,
            State = state
        };

        return View("Consent", model);
    }

    [HttpPost]
    public async Task<IActionResult> Authorize([FromForm] ConsentViewModel model)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == model.ClientId);
        if (client == null)
            return BadRequest("Invalid client");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity!.Name);
        if (user == null)
            return Unauthorized();

        var code = new AuthorizationCode
        {
            ClientId = client.Id,
            UserId = user.Id,
            RedirectUri = model.RedirectUri,
            Scope = model.Scope
        };

        _db.AuthorizationCodes.Add(code);
        await _db.SaveChangesAsync();

        var redirect = $"{model.RedirectUri}?code={code.Code}";
        if (!string.IsNullOrEmpty(model.State))
            redirect += $"&state={Uri.EscapeDataString(model.State)}";

        return Redirect(redirect);
    }
}
