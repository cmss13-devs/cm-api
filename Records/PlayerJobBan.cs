namespace CmApi.Records;

public record PlayerJobBan(
    int Id,
    int PlayerId,
    int? AdminId,
    string Text,
    string? Date,
    int? BanTime,
    long? Expiration,
    string Role,
    
    // computed in API
    string? BanningAdminCkey
    );