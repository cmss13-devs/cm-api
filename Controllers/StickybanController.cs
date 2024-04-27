using CmApi.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StickybanController(IDatabase database, IExternalLogger externalLogger) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllBans()
    {
        return Ok(database.GetStickybans());
    }

    [HttpPost]
    [Route("Whitelist")]
    [OAuthFilter]
    public IActionResult WhitelistCkey(string ckey)
    {
        var adminCkey = User.Identity?.Name;
        if (adminCkey == null)
        {
            return Unauthorized();
        }

        var stickybans = database.StickybanWhitelistCkey(ckey);

        if (stickybans <= 0)
        {
            return Ok(stickybans);
        }

        externalLogger.LogExternal("Player Whitelisted", $"{adminCkey} whitelisted {ckey} against all matching stickybans.");
        database.CreateNote(ckey, adminCkey, "User was whitelisted against all stickybans.", true);
        return Ok(stickybans);
    }

    
    [HttpGet]
    [Route("{id:int}/Match/Cid")]
    public IActionResult GetMatchingCids(int id)
    {
        return Ok(database.GetStickybanMatchedCids(id));
    }

    [HttpGet]
    [Route("{id:int}/Match/Ckey")]
    public IActionResult GetMatchingCkey(int id)
    {
        return Ok(database.GetStickybanMatchedCkeys(id));
    }
    
    [HttpGet]
    [Route("{id:int}/Match/Ip")]
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