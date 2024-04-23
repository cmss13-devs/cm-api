using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class RoundController(IByond byond) : ControllerBase
{
    [HttpGet]
    public IActionResult CurrentRoundInfo()
    {
        var status = byond.GetServerStatus();
        if (status == null)
        {
            return StatusCode(501);
        }

        return Ok(status);
    }


}