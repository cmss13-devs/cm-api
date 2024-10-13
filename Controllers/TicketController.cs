using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TicketController(IDatabase database) : ControllerBase
{
    [HttpGet]
    [Route("{roundId:int}")]
    public IActionResult TicketsForRoundId(int roundId)
    {
        var tickets = database.GetTicketsByRoundId(roundId);
        if (tickets.Count == 0)
        {
            return NotFound();
        }

        return Ok(tickets);
    }


}