using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
// ReSharper disable InconsistentNaming

namespace CmApi.Classes;

public class ExternalLogger(IConfiguration configuration) : IExternalLogger
{

    private JsonSerializerOptions _serializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private IConfiguration _configuration = configuration;

    public void LogExternal(string title, string toLog)
    {

        var destination = _configuration.GetSection("ExternalLogging")["Webhook"];
        if (destination == null)
        {
            return;
        }
        using var sender = new HttpClient();
        var embed = new WebhookEmbed
        {
            title = title,
            description = toLog,
            color = 8359053, // darker grey
            author = new EmbedAuthor
            {
                name = "[cmdb]",
                url = "https://db.cm-ss13.com"
            }
        };
        var hook = new WebhookContent
        {
            embeds = new[] { embed },
            username = "[cmdb]"
        };

        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(destination);
        var json = JsonSerializer.Serialize(hook, _serializer);
        request.Content = new StringContent(json);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        sender.Send(request);
    }
}

public interface IExternalLogger
{
    void LogExternal(string title, string toLog);
}

public class WebhookContent
{
    public string content { get; set; }
    public string username { get; set; }
    public string avatar_url { get; set; }
    public bool tts { get; set; }
    public IEnumerable<WebhookEmbed> embeds { get; set; }
}

public class WebhookEmbed
{
    public string? title { get; set; }
    public string? type { get; set; }
    public string? description { get; set; }
    public string? url { get; set; }
    public Timestamp? timestamp { get; set; }
    public int? color { get; set; }
    public EmbedFooter? footer { get; set; }
    public EmbedImage? image { get; set; }
    public EmbedThumbnail? thumbnail { get; set; }
    public EmbedVideo? video { get; set; }
    public EmbedAuthor? author { get; set; }
    public IEnumerable<EmbedField>? fields { get; set; }
}

public class EmbedFooter
{
    public string text { get; set; }
    public string? icon_url { get; set; }
    public string? proxy_icon_url { get; set; }
}

public class EmbedImage
{
    public string url { get; set; }
    public string? proxy_url { get; set; }
    public int? height { get; set; }
    public int? width { get; set; }
}

public class EmbedThumbnail
{
    public string url { get; set; }
    public string? proxy_url { get; set; }
    public int? height { get; set; }
    public int? width { get; set; }
}

public class EmbedVideo
{
    public string? url { get; set; }
    public string? proxy_url { get; set; }
    public int? height { get; set; }
    public int? width { get; set; }
}

public class EmbedAuthor
{
    /// <summary>
    /// The name of the author of this embed.
    /// </summary>
    public required string name { get; set; }
    /// <summary>
    /// The URL that will be linked to.
    /// </summary>
    public string? url { get; set; }
    /// <summary>
    /// This will appear as the profile picture of the user.
    /// </summary>
    public string? icon_url { get; set; }
    /// <summary>
    /// Proxy URL.
    /// </summary>
    public string? proxy_icon_url { get; set; }
}

public class EmbedField
{
    /// <summary>
    /// The key of the field.
    /// </summary>
    public string name { get; set; }
    /// <summary>
    /// The value of the field.
    /// </summary>
    public string value { get; set; }
    /// <summary>
    /// If this should appear in line or not.
    /// </summary>
    public bool? inline { get; set; }
}