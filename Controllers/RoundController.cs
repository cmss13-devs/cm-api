using Byond.TopicSender;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class RoundController(ILogger<TopicClient> logger) : ControllerBase
{
    private readonly ILogger<TopicClient> _logger = logger;

    [HttpGet]
    public IActionResult CurrentRoundInfo()
    {
        return Ok();

    }
}