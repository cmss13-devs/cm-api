namespace CmApi.Records;

public record PlayerNote(
    int Id,
    int PlayerId,
    int AdminId,
    string? Text,
    string Date,
    bool IsBan,
    long? BanTime,
    bool IsConfidential,
    string AdminRank,
    int? NoteCategory,
    int? RoundId,
    
    // computed in API
    string? NotedPlayerCkey,
    string? NotingAdminCkey
    );