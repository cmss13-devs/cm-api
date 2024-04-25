using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IDatabase _database;

    public UserController(IDatabase database)
    {
        _database = database;
    }
    
    /// <summary>
    /// Gets a user from the database.
    /// </summary>
    /// <param name="id">The ID of the user to look up.</param>
    /// <returns>The <see cref="IActionResult"/> of the request.</returns>
    /// <response code="404">The requested user could not be found.</response>
    [HttpGet]
    public IActionResult GetUser(int id)
    {
        var user = _database.GetPlayer(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>
    /// Gets a <see cref="Records.Player"/> from the database from their Ckey.
    /// </summary>
    /// <param name="ckey">The Ckey of the user to look up.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("Ckey")]
    public IActionResult GetUserCkey(string ckey)
    {
        var user = _database.GetPlayer(ckey);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Route("Note")]
    [OAuthFilter]
    public IActionResult AddPlayerNote(AddNoteRequest request)
    {
        var user = Request.HttpContext.User.Identity?.Name;
        if (user == null)
        {
            return Unauthorized();
        }
        
        return Ok(_database.CreateNote(request.Ckey, user, request.Message, request.Confidential, request.Category));
    }
}