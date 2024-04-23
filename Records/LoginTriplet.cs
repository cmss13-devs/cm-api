namespace CmApi.Records;

public record LoginTriplet(
    int Id,
    string Ckey,
    int Ip1,
    int Ip2,
    int Ip3,
    int Ip4,
    string LastKnownCid,
    DateTime LoginDate
    );