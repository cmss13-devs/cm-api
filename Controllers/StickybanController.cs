using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StickybanController(IDatabase database) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllBans()
    {
        return Ok(database.GetStickybans());
    }

    [HttpGet]
    [Route("Cid")]
    public IActionResult GetMatchingCids(int id)
    {
        return Ok(database.GetStickybanMatchedCids(id));
    }

    [HttpGet]
    [Route("Ckey")]
    public IActionResult GetMatchingCkey(int id)
    {
        return Ok(database.GetStickybanMatchedCkeys(id));
    }
    
    [HttpGet]
    [Route("Ip")]
    public IActionResult GetMatchingIps(int id)
    {
        return Ok(database.GetStickybanMatchedIps(id));
    }
}