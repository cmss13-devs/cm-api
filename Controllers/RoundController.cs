using System.Net;
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

        DoThing();
        
        return Ok();
    }

    private async void DoThing()
    {

        var tenSeconds = TimeSpan.FromSeconds(10);
        
        var sender = new TopicClient(new SocketParameters
        {
            ConnectTimeout = tenSeconds,
            DisconnectTimeout = tenSeconds,
            ReceiveTimeout = tenSeconds,
            SendTimeout = tenSeconds,
        });

        var ip = IPAddress.Parse("127.0.0.1");
        
        var send = await sender.SendTopic(ip, "status", 1400);
        
        Console.WriteLine(send.StringData);
        
    }
}