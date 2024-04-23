using System.Text.Json;
using Byond.TopicSender;

namespace CmApi.Classes;

public class Byond(IConfiguration configuration) : IByond
{
    private JsonSerializerOptions _serializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private IConfiguration _configuration = configuration;
    
    private string? _cachedStatus;
    private DateTime? _cacheTime;

    public string? GetServerStatus()
    {
        if (_cacheTime.HasValue)
        {
            var oneMinuteLater = _cacheTime.Value.AddMinutes(1);
            if (oneMinuteLater > DateTime.Now)
            {
                return _cachedStatus;
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
            return null;
        }

        var json = JsonSerializer.Serialize(new ByondQuery(auth, "status", "cm-api"), _serializer);

        var send = sender.SendTopic(host, json, ushort.Parse(port));
        var task = send.AsTask();

        _cachedStatus = task.Result.StringData;
        _cacheTime = DateTime.Now;
        
        return _cachedStatus;
    }
    
}

public interface IByond
{
    string? GetServerStatus();
}