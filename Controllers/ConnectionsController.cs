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
        var triplets = _database.GetConnections(ip);
        if (triplets.Count == 0)
        {
            return NotFound();
        }
        return Ok(triplets);
    }
    
    [HttpGet]
    [Route("Cid")]
    public IActionResult GetConnectionCid(int cid)
    {
        var triplets = _database.GetConnections(cid);
        if (triplets.Count == 0)
        {
            return NotFound();
        }

        return Ok(triplets);
    }
}