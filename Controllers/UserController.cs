using CmApi.Classes;
using CmApi.Records;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IDatabase _database;
    private readonly IExternalLogger _externalLogger;

    public UserController(IDatabase database, IExternalLogger externalLogger)
    {
        _database = database;
        _externalLogger = externalLogger;
    }
    
    /// <summary>
    /// Gets a <see cref="Records.Player"/> from the database from their Ckey.
    /// </summary>
    /// <param name="ckey">The Ckey of the user to look up.</param>
    /// <param name="discordId">The Discord ID of the user to look up.</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetUser(string? ckey, string? discordId)
    {
        if (ckey == null && discordId == null)
        {
            return BadRequest();
        }

        Player? user;
        if (discordId == null)
        {
            user = _database.GetPlayer(ckey!);
        }
        else
        {
            var link = _database.GetDiscordLinkByDiscordId(discordId);
            if (link == null)
            {
                return NotFound();
            }

            user = _database.GetPlayer(link.PlayerId);
        }
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
    
    /// <summary>
    /// Gets a user from the database.
    /// </summary>
    /// <param name="id">The ID of the user to look up.</param>
    /// <returns>The <see cref="IActionResult"/> of the request.</returns>
    /// <response code="404">The requested user could not be found.</response>
    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetUser(int id)
    {
        var user = _database.GetPlayer(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    
    [HttpPost]
    [Route("{id:int}/Note")]
    [OAuthFilter]
    public IActionResult AddPlayerNote(int id, AddNoteRequest request)
    {
        var user = User.Identity?.Name;
        if (user == null)
        {
            return Unauthorized();
        }

        var toNote = _database.ShallowPlayerName(id);
        if (string.IsNullOrEmpty(toNote))
        {
            return NotFound();
        }
        
        _externalLogger.LogExternal("Note Added", $"{user} added a note to {toNote}: {request.Message}");
        return Ok(_database.CreateNote(id, user, request.Message, request.Confidential, request.Category));
    }

    [HttpGet]
    [Route("{id:int}/AppliedNotes")]
    public IActionResult CheckAppliedNotes(int id)
    {
        return Ok(_database.GetAppliedPlayerNotes(id));
    }
}