using System.Text.Json;
using Byond.TopicSender;
using CmApi.Classes;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Controllers;

/// <summary>
/// <see cref="ControllerBase"/> for managing Users.
/// </summary>
[ApiController]
[Route("[controller]")]
public class RoundController(IConfiguration configuration) : ControllerBase
{

    private IConfiguration _configuration = configuration;
    private JsonSerializerOptions _serializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private string? _cachedStatus;
    private DateTime? _cacheTime;

    [HttpGet]
    public IActionResult CurrentRoundInfo()
    {

        if (_cacheTime.HasValue)
        {
            var oneMinuteLater = _cacheTime.Value.AddMinutes(1);
            if (oneMinuteLater > DateTime.Now)
            {
                return Ok(_cachedStatus);
            }
        }

        var tenSeconds = TimeSpan.FromSeconds(10);
        
        var sender = new TopicClient(new SocketParameters
        {
            ConnectTimeout = tenSeconds,
            DisconnectTimeout = tenSeconds,
            ReceiveTimeout = tenSeconds,
            SendTimeout = tenSeconds,
        });

        var host = _configuration.GetSection("Game")["Host"];
        var port = _configuration.GetSection("Game")["Port"];
        var auth = _configuration.GetSection("Game")["AuthToken"];
        
        if (auth == null || port == null || host == null)
        {
            return StatusCode(501);
        }

        var json = JsonSerializer.Serialize(new ByondQuery(auth, "status", "cm-api"), _serializer);

        var send = sender.SendTopic(host, json, ushort.Parse(port));
        var task = send.AsTask();

        _cachedStatus = task.Result.StringData;
        _cacheTime = DateTime.Now;
        
        return Ok(_cachedStatus);
    }


}