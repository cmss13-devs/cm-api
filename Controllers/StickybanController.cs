using CmApi.Classes;
using Microsoft.AspNetCore.Authorization;
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
    [Route("Match/Cid")]
    public IActionResult GetMatchingCids(int id)
    {
        return Ok(database.GetStickybanMatchedCids(id));
    }

    [HttpPost]
    [Route("Whitelist")]
    [OAuthFilter]
    public IActionResult WhitelistCkey(string ckey)
    {
        if (User.Identity?.Name == null)
        {
            return Unauthorized();
        }
        database.CreateNote(ckey, User.Identity.Name, "User was whitelisted against all stickybans.", true);
        return Ok(database.StickybanWhitelistCkey(ckey));
    }

    [HttpGet]
    [Route("Match/Ckey")]
    public IActionResult GetMatchingCkey(int id)
    {
        return Ok(database.GetStickybanMatchedCkeys(id));
    }
    
    [HttpGet]
    [Route("Match/Ip")]
    public IActionResult GetMatchingIps(int id)
    {
        return Ok(database.GetStickybanMatchedIps(id));
    }

    [HttpGet]
    [Route("Cid")]
    public IActionResult GetStickyByCid(string cid)
    {
        return Ok(database.GetStickybanWithMatchingCid(cid));
    }
    
    [HttpGet]
    [Route("Ckey")]
    public IActionResult GetStickyByCkey(string ckey)
    {
        return Ok(database.GetStickybanWithMatchingCkey(ckey));
    }
    
    [HttpGet]
    [Route("Ip")]
    public IActionResult GetStickyByIp(string ip)
    {
        return Ok(database.GetStickybanWithMatchingIp(ip));
    }
}