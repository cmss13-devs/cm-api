using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController(IByond byond) : ControllerBase
{
    [HttpGet]
    [OAuthFilter]
    public IActionResult IsAuthorized()
    {
        Request.Headers.TryGetValue("X-Forwarded-Preferred-Username", out var value);
        return Ok(value.First());
    }


}