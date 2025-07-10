using Microsoft.AspNetCore.Mvc;
using Watashi.Services;

namespace Watashi.Controllers;

[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController(RsaKeyService rsaKeyService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var jwk = rsaKeyService.GetPublicJwk();
        return Ok(new { keys = new[] { jwk } });
    }
}
