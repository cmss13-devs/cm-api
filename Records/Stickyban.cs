namespace CmApi.Records;

/// <summary>
/// Represents a stickyban from the database, applied to a user and prevents matching
/// users (with the same CKEY, IP, or CID) from connecting, and banning them, too.
/// </summary>
public record Stickyban
{
    /// <summary>
    /// The ID of the ban in the database.
    /// </summary>
    public required int Id { get; set; }
    /// <summary>
    /// The name of the banned user, or bancode.
    /// </summary>
    public required string Identifier { get; set; }
    /// <summary>
    /// The reason shown to the user.
    /// </summary>
    public required string Reason { get; set; }
    /// <summary>
    /// The internal reason, not shown to the user.
    /// </summary>
    public required string Message { get; set; }
    /// <summary>
    /// When this was placed.
    /// </summary>
    public required string Date { get; set; }

    /// <summary>
    /// If this stickyban is active.
    /// </summary>
    public required bool Active { get; set; }
    
    /// <summary>
    /// The ID of the admin that placed the ban.
    /// </summary>
    public long? AdminId { get; set; }
    
    // computed
    
    /// <summary>
    /// The CKEY of the admin that placed the ban.
    /// </summary>
    public string? AdminCkey { get; set; }
}

/// <summary>
/// Represents a match from a stickyban to an impacted CID
/// </summary>
public record StickybanMatchedCid
{
    /// <summary>
    /// The ID of the match
    /// </summary>
    public required int Id { get; set; }
    /// <summary>
    /// The CID this match is for
    /// </summary>
    public required string Cid { get; set; }
    /// <summary>
    /// The stickyban this match links to
    /// </summary>
    public required int LinkedStickyban { get; set; }
}

/// <summary>
/// Represents a match from a stickyban to an impacted CKEY
/// </summary>
public record StickybanMatchedCkey
{
    /// <summary>
    /// The ID of the match
    /// </summary>
    public required int Id { get; set; }
    /// <summary>
    /// The CKEY this match is for
    /// </summary>
    public required string Ckey { get; set; }
    /// <summary>
    /// The stickyban this match links to
    /// </summary>
    public required int LinkedStickyban { get; set; }
    /// <summary>
    /// If this match is a whitelist, allowing the user to connect if otherwise their CID/IP would prevent it.
    /// </summary>
    public required bool Whitelisted { get; set; }
}

/// <summary>
/// Represents a match from a stickyban to an impacted IP
/// </summary>
public record StickybanMatchedIp
{
    /// <summary>
    /// The ID of the match
    /// </summary>
    public required int Id { get; set; }
    /// <summary>
    /// The IP this match is for
    /// </summary>
    public required string Ip { get; set; }
    /// <summary>
    /// The stickyban this match links to
    /// </summary>
    public required int LinkedStickyban { get; set; }
}