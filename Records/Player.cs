namespace CmApi.Records;

/// <summary>
/// Represents a BYOND database user.
/// </summary>
public record Player
{
    /// <summary>
    /// Represents a BYOND database user.
    /// </summary>
    /// <param name="Id">The ID in the database.</param>
    /// <param name="Ckey">The CKEY of the user.</param>
    /// <param name="LastLogin">The date the user last logged in.</param>
    /// <param name="IsPermabanned">If the user is permabanned.</param>
    /// <param name="PermabanReason">If permabanned, the reason for being.</param>
    /// <param name="PermabanDate">If permabanned, the date this was applied.</param>
    /// <param name="PermabanAdminId">If permabanned, the ID of the admin that applied this ban.</param>
    /// <param name="IsTimeBanned">If the user is temporarily banned.</param>
    /// <param name="TimeBanReason">If tempbanned, the reason for being.</param>
    /// <param name="TimeBanAdminId">If tempbanned, the ID of the admin that applied this ban.</param>
    /// <param name="TimeBanDate">If tempbanned, the date this was applied.</param>
    /// <param name="LastKnownIp">The last IP this user connected on.</param>
    /// <param name="LastKnownCid">The last CID this user connected with.</param>
    /// <param name="TimeBanExpiration">When this user's temporary ban should expire, in BYONDtime.</param>
    /// <param name="MigratedNotes">If this user's notes have been migrated from .sav files.</param>
    /// <param name="MigratedJobBans">If this user's jobbans have been migrated from .sav files.</param>
    /// <param name="StickybanWhitelisted">If this user should be exempted from stickybans.</param>
    /// <param name="DiscordLinkId">The presently in use Discord link.</param>
    /// <param name="WhitelistStatus">The whitelists held by this user.</param>
    /// <param name="ByondAccountAge">When this user first joined BYOND.</param>
    /// <param name="FirstJoinDate">When this user first joined CM.</param>
    public Player(int Id, 
        string Ckey, 
        string LastLogin, 
        bool IsPermabanned, 
        string? PermabanReason, 
        string? PermabanDate, 
        bool IsTimeBanned, 
        string? TimeBanReason, 
        int? TimeBanAdminId, 
        string? TimeBanDate,
        string LastKnownIp,
        string LastKnownCid,
        long? TimeBanExpiration,
        bool MigratedNotes,
        bool MigratedBans,
        bool MigratedJobBans,
        int? PermabanAdminId,
        bool StickybanWhitelisted,
        int? DiscordLinkId,
        string? WhitelistStatus,
        string? ByondAccountAge,
        string? FirstJoinDate
    )
    {
        this.Id = Id;
        this.Ckey = Ckey;
        this.LastLogin = LastLogin;
        this.IsPermabanned = IsPermabanned;
        this.PermabanReason = PermabanReason;
        this.PermabanDate = PermabanDate;
        this.IsTimeBanned = IsTimeBanned;
        this.TimeBanReason = TimeBanReason;
        this.TimeBanAdminId = TimeBanAdminId;
        this.TimeBanDate = TimeBanDate;
        this.LastKnownIp = LastKnownIp;
        this.LastKnownCid = LastKnownCid;
        this.TimeBanExpiration = TimeBanExpiration;
        this.MigratedNotes = MigratedNotes;
        this.MigratedBans = MigratedBans;
        this.MigratedJobBans = MigratedJobBans;
        this.PermabanAdminId = PermabanAdminId;
        this.StickybanWhitelisted = StickybanWhitelisted;
        this.DiscordLinkId = DiscordLinkId;
        this.WhitelistStatus = WhitelistStatus;
        this.ByondAccountAge = ByondAccountAge;
        this.FirstJoinDate = FirstJoinDate;
    }

    /// <summary>The ID in the database.</summary>
    public int Id { get; init; }

    /// <summary>The CKEY of the user.</summary>
    public string Ckey { get; init; }

    /// <summary>The date the user last logged in.</summary>
    public string LastLogin { get; init; }

    /// <summary>If the user is permabanned.</summary>
    public bool IsPermabanned { get; init; }

    /// <summary>If permabanned, the reason for being.</summary>
    public string? PermabanReason { get; init; }

    /// <summary>If permabanned, the date this was applied.</summary>
    public string? PermabanDate { get; init; }

    /// <summary>If the user is temporarily banned.</summary>
    public bool IsTimeBanned { get; init; }

    /// <summary>If tempbanned, the reason for being.</summary>
    public string? TimeBanReason { get; init; }

    /// <summary>If tempbanned, the ID of the admin that applied this ban.</summary>
    public int? TimeBanAdminId { get; init; }

    /// <summary>If tempbanned, the date this was applied.</summary>
    public string? TimeBanDate { get; init; }

    /// <summary>The last IP this user connected on.</summary>
    public string LastKnownIp { get; init; }

    /// <summary>The last CID this user connected with.</summary>
    public string LastKnownCid { get; init; }

    /// <summary>When this user's temporary ban should expire, in BYONDtime.</summary>
    public long? TimeBanExpiration { get; init; }

    /// <summary>If this user's notes have been migrated from .sav files.</summary>
    public bool MigratedNotes { get; init; }

    public bool MigratedBans { get; init; }

    /// <summary>If this user's jobbans have been migrated from .sav files.</summary>
    public bool MigratedJobBans { get; init; }

    /// <summary>If permabanned, the ID of the admin that applied this ban.</summary>
    public int? PermabanAdminId { get; init; }

    /// <summary>If this user should be exempted from stickybans.</summary>
    public bool StickybanWhitelisted { get; init; }

    /// <summary>The presently in use Discord link.</summary>
    public int? DiscordLinkId { get; init; }

    /// <summary>The whitelists held by this user.</summary>
    public string? WhitelistStatus { get; init; }

    /// <summary>When this user first joined BYOND.</summary>
    public string? ByondAccountAge { get; init; }

    /// <summary>When this user first joined CM.</summary>
    public string? FirstJoinDate { get; init; }

    public IEnumerable<PlayerNote>? Notes { get; set; }
    public IEnumerable<PlayerJobBan>? JobBans { get; set; }
    public string? PermabanAdminCkey { get; set; }
    public string? TimeBanAdminCkey { get; set; }
    public string? DiscordId { get; set; }
}