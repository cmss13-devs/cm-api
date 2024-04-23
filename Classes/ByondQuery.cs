using System.Text.Json.Serialization;

namespace CmApi.Classes;

public class ByondQuery
{
    public string Auth { get; set; }
    public string Query { get; set; }
    public string Source { get; set; }

    public ByondQuery(string auth, string query, string source)
    {
        Auth = auth;
        Query = query;
        Source = source;
    }
}