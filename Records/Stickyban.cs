namespace CmApi.Records;

public record Stickyban(
    int Id,
    string Identifier,
    string Reason,
    string Message,
    string Date,
    bool Active,
    long? AdminId,
    
    // computed
    string? AdminCkey
    );
    
public record StickybanMatchedCid(
    int Id,
    string Cid,
    int LinkedStickyban
    );
    
public record StickybanMatchedCkey(
    int Id,
    string Ckey,
    int LinkedStickyban,
    bool Whitelisted
);

public record StickybanMatchedIp(
    int Id,
    string Ip,
    int LinkedStickyban
);
    