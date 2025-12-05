using AuthenticationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

[ApiController]
[Route(".well-known")]
public class WellKnownController : ControllerBase
{
    private readonly IJwksProvider _jwksProvider;

    public WellKnownController(IJwksProvider jwksProvider)
    {
        _jwksProvider = jwksProvider;
    }

    [HttpGet("jwks.json")]
    [AllowAnonymous]
    public IActionResult GetJwks()
    {
        var snapshot = _jwksProvider.GetSnapshot();
        return Ok(new
        {
            keys = snapshot.Keys
        });
    }
}
