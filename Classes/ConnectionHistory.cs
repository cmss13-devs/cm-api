using CmApi.Records;

namespace CmApi.Classes;

public class ConnectionHistory
{
    public List<LoginTriplet>? Triplets { get; set; }
    
    public List<string>? AllCkeys { get; set; }
    public List<string>? AllCids { get; set; }
    public List<string>? AllIps { get; set; }
    
    public ConnectionHistory(List<LoginTriplet>? triplets = null, List<string>? ckeys = null, List<string>? cids = null, List<string>? ips = null)
    {
        if (triplets == null)
        {
            AllCkeys = ckeys;
            AllCids = cids;
            AllIps = ips;
        }
        else
        {
            Triplets = triplets;

            var uniqueCkeys = new List<string>();
            var uniqueCids = new List<string>();
            var uniqueIps = new List<string>();
            foreach (var triplet in triplets)
            {
                if (!uniqueCkeys.Contains(triplet.Ckey))
                {
                    uniqueCkeys.Add(triplet.Ckey);
                }
            
                if (!uniqueCids.Contains(triplet.LastKnownCid))
                {
                    uniqueCids.Add(triplet.LastKnownCid);
                }

                var computedIp = $"{triplet.Ip1}.{triplet.Ip2}.{triplet.Ip3}.{triplet.Ip4}";
                if (!uniqueIps.Contains(computedIp))
                {
                    uniqueIps.Add(computedIp);
                }

            }

            AllCkeys = uniqueCkeys;
            AllCids = uniqueCids;
            AllIps = uniqueIps;
        }
    }
}