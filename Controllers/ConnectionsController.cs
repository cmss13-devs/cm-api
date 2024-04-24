using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly IDatabase _database;

    public ConnectionsController(IDatabase database)
    {
        _database = database;
    }
    
    [HttpGet]
    [Route("Ip")]
    public IActionResult GetConnectionIp(string ip)
    {
        var triplets = _database.GetConnectionsByIp(ip);
        if (triplets.Count == 0)
        {
            return NotFound();
        }
        return Ok(new ConnectionHistory(triplets));
    }
    
    [HttpGet]
    [Route("Cid")]
    public IActionResult GetConnectionCid(string cid)
    {
        var triplets = _database.GetConnectionsByCid(cid);
        if (triplets.Count == 0)
        {
            return NotFound();
        }

        return Ok(new ConnectionHistory(triplets));
    }

    [HttpGet]
    [Route("Ckey")]
    public IActionResult GetConnectionCkey(string ckey)
    {
        var triplets = _database.GetConnectionsByCkey(ckey);
        if (triplets.Count == 0)
        {
            return NotFound();
        }

        return Ok(new ConnectionHistory(triplets));
    }

    [HttpGet]
    [Route("FullByAllCid")]
    public IActionResult GetConnectionsWithMatchingCidByCkey(string ckey)
    {
        var triplets = _database.GetConnectionsWithMatchingCidByCkey(ckey);
        if (triplets.Count == 0)
        {
            return NotFound();
        }

        return Ok(new ConnectionHistory(triplets));
    }
    
    [HttpGet]
    [Route("FullByAllIps")]
    public IActionResult GetConnectionsWithMatchingIpByCkey(string ckey)
    {
        var triplets = _database.GetConnectionsWithMatchingIpByCkey(ckey);
        if (triplets.Count == 0)
        {
            return NotFound();
        }

        return Ok(new ConnectionHistory(triplets));
    }
}