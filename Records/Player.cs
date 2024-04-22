namespace CmApi.Records;

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
public record Player( 
    
    // raw from database
    int Id, 
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
    string? FirstJoinDate,
    
    // computed in API
    IEnumerable<PlayerNote>? Notes,
    IEnumerable<PlayerJobBan>? JobBans,
    string? PermabanAdminCkey,
    string? TimeBanAdminCkey
    );