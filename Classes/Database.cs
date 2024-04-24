using CmApi.Records;
using MySql.Data.MySqlClient;

namespace CmApi.Classes;

public class Database(IConfiguration configuration) : IDatabase
{
    /// <summary>
    /// Gets a user based on the ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="Player"/>or null</returns>
    public Player? GetPlayer(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM players WHERE id = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquirePlayer(sqlCommand);
    }

    public Player? GetPlayer(string ckey)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM players WHERE ckey = @ckey";
        sqlCommand.Parameters.AddWithValue("@ckey", ckey);

        return AcquirePlayer(sqlCommand);
    }
    
    
    private Player? AcquirePlayer(MySqlCommand sqlCommand)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            sqlCommand.Connection = sqlConnection;

            Player? user;
            
            using (var dataReader = sqlCommand.ExecuteReader())
            {

                if (!dataReader.HasRows)
                {
                    return null;
                }
                
                dataReader.Read();

                var gottenId = dataReader.GetInt32("id");

                // if our player got made, but not actually by a player existing (ie, a note lookup)
                if (dataReader.IsDBNull(dataReader.GetOrdinal("last_login")))
                {
                    return null;
                }

                var notes = GetPlayerNotes(gottenId);
                var bans = GetPlayerJobBans(gottenId);

                
                string? permabanningAdmin = null;
                
                var isPermabanned = dataReader.GetBoolean("is_permabanned");
                var permabanAdminId = GetInt32NullSafe(dataReader, "permaban_admin_id");
                if (isPermabanned && permabanAdminId.HasValue)
                {
                    permabanningAdmin = ShallowPlayerName(permabanAdminId.Value);
                }

                string? timeBanAdmin = null;

                var isTimebanned = dataReader.GetBoolean("is_time_banned");
                var timebanAdminId = GetInt32NullSafe(dataReader, "time_ban_admin_id");
                if (isTimebanned && timebanAdminId.HasValue)
                {
                    timeBanAdmin = ShallowPlayerName(timebanAdminId.Value);
                }

                var discordId = GetDiscordLink(gottenId)?.DiscordId;

                user = new Player(
                    Id: gottenId,
                    Ckey: dataReader.GetString("ckey"),
                    LastLogin: dataReader.GetString("last_login"),
                    IsPermabanned: isPermabanned,
                    PermabanReason: GetStringNullSafe(dataReader, "permaban_reason"),
                    PermabanDate: GetStringNullSafe(dataReader, "permaban_date"),
                    IsTimeBanned: isTimebanned,
                    TimeBanReason: GetStringNullSafe(dataReader, "time_ban_reason"),
                    TimeBanAdminId: GetInt32NullSafe(dataReader, "time_ban_admin_id"),
                    TimeBanDate: GetStringNullSafe(dataReader, "time_ban_date"),
                    LastKnownIp: dataReader.GetString("last_known_ip"),
                    LastKnownCid: dataReader.GetString("last_known_cid"),
                    TimeBanExpiration: GetInt64NullSafe(dataReader, "time_ban_expiration"),
                    MigratedNotes: dataReader.GetBoolean("migrated_notes"),
                    MigratedBans: dataReader.GetBoolean("migrated_bans"),
                    MigratedJobBans: dataReader.GetBoolean("migrated_jobbans"),
                    PermabanAdminId: permabanAdminId,
                    StickybanWhitelisted: GetBoolNullSafe(dataReader, "stickyban_whitelisted"),
                    DiscordLinkId: GetInt32NullSafe(dataReader, "discord_link_id"),
                    WhitelistStatus: GetStringNullSafe(dataReader, "whitelist_status"),
                    ByondAccountAge: GetStringNullSafe(dataReader, "byond_account_age"),
                    FirstJoinDate: GetStringNullSafe(dataReader, "first_join_date"),
                    Notes: notes,
                    JobBans: bans,
                    PermabanAdminCkey: permabanningAdmin,
                    TimeBanAdminCkey: timeBanAdmin,
                    DiscordId: discordId
                );

            }
            sqlConnection.Close();

            return user;

        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return null;
    }

    public IEnumerable<PlayerNote>? GetPlayerNotes(int id)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"SELECT * FROM player_notes WHERE player_id = @id";
            sqlCommand.Parameters.AddWithValue("@id", id);

            var notes = new List<PlayerNote>();

            using (var sqlReader = sqlCommand.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var notingAdminId = sqlReader.GetInt32("admin_id");
                    var notingAdmin = ShallowPlayerName(notingAdminId);
                    
                    var note = new PlayerNote(
                        Id: sqlReader.GetInt32("id"),
                        PlayerId: sqlReader.GetInt32("player_id"),
                        AdminId: notingAdminId,
                        Text: sqlReader.GetString("text"),
                        Date: sqlReader.GetString("date"),
                        IsBan: sqlReader.GetBoolean("is_ban"),
                        BanTime: GetInt64NullSafe(sqlReader, "ban_time"),
                        IsConfidential: sqlReader.GetBoolean("is_confidential"),
                        AdminRank: sqlReader.GetString("admin_rank"),
                        NoteCategory: GetInt32NullSafe(sqlReader, "note_category"),
                        RoundId: GetInt32NullSafe(sqlReader, "round_id"),
                        NotingAdminCkey: notingAdmin
                    );

                    notes.Add(note);
                }
                
                sqlConnection.Close();
            }

            return notes;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return null;
    }

    public IEnumerable<PlayerJobBan>? GetPlayerJobBans(int id)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"SELECT * FROM player_job_bans WHERE player_id = @id";
            sqlCommand.Parameters.AddWithValue("@id", id);

            var jobBans = new List<PlayerJobBan>();

            using var sqlReader = sqlCommand.ExecuteReader();
            while (sqlReader.Read())
            {

                var banningAdminId = sqlReader.GetInt32("admin_id");
                var banningAdmin = ShallowPlayerName(banningAdminId);
                    
                var ban = new PlayerJobBan(
                    Id: sqlReader.GetInt32("id"),
                    PlayerId: sqlReader.GetInt32("player_id"),
                    AdminId: banningAdminId,
                    Text: sqlReader.GetString("text"),
                    Date: sqlReader.GetString("date"),
                    BanTime: GetInt32NullSafe(sqlReader, "ban_time"),
                    Expiration: GetInt64NullSafe(sqlReader, "expiration"),
                    Role: sqlReader.GetString("role"),
                    BanningAdminCkey: banningAdmin
                );
                    
                jobBans.Add(ban);
            }
                
            sqlConnection.Close();

            return jobBans;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return null;
    }

    private string? ShallowPlayerName(int id)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"SELECT ckey FROM players WHERE id = @id";
            sqlCommand.Parameters.AddWithValue("@id", id);

            using (var sqlReader = sqlCommand.ExecuteReader())
            {
                sqlReader.Read();
                var ckey = sqlReader.GetString("ckey");
                sqlConnection.Close();
                return ckey;
            }
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return null;
    }

    private DiscordLink? GetDiscordLink(int id)
    {
        {
            try
            {
                var sqlConnection = GetConnection();
                sqlConnection.Open();

                var sqlCommand = new MySqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = @"SELECT * FROM discord_links WHERE player_id = @id";
                sqlCommand.Parameters.AddWithValue("@id", id);

                DiscordLink? link;

                using (var sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();

                    link = new DiscordLink(
                        Id: sqlReader.GetInt32("id"),
                        DiscordId: sqlReader.GetInt64("discord_id"),
                        PlayerId: sqlReader.GetInt32("player_id")
                    );
                
                    sqlConnection.Close();
                    
                }

                return link;

            }
            catch (MySqlException exception)
            {
                Console.Error.WriteLine(exception.ToString());
            }

            return null;
        }
    }

    public List<LoginTriplet> GetConnectionsByCid(string cid)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM login_triplets WHERE last_known_cid = @cid";
        sqlCommand.Parameters.AddWithValue("@cid", cid);

        return AcquireTriplets(sqlCommand);
    }
    
    public List<LoginTriplet> GetConnectionsByIp(string ip)
    {

        var split = ip.Split(".");

        if (split.Length != 4)
        {
            return new List<LoginTriplet>();
        }
        
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM login_triplets WHERE ip1 = @ip1 AND ip2 = @ip2 AND ip3 = @ip3 AND ip4 = @ip4";
        sqlCommand.Parameters.AddWithValue("@ip1", split[0]);
        sqlCommand.Parameters.AddWithValue("@ip2", split[1]);
        sqlCommand.Parameters.AddWithValue("@ip3", split[2]);
        sqlCommand.Parameters.AddWithValue("@ip4", split[3]);

        return AcquireTriplets(sqlCommand);
    }
    
    public List<LoginTriplet> GetConnectionsByCkey(string ckey)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM login_triplets WHERE ckey = @ckey";
        sqlCommand.Parameters.AddWithValue("@ckey", ckey);

        return AcquireTriplets(sqlCommand);
    }
    
    public List<LoginTriplet> GetConnectionsWithMatchingIpByCkey(string ckey)
    {
        var allConnections = GetConnectionsByCkey(ckey);

        var uniqueIps = new List<string>();
        foreach (var triplet in allConnections)
        {
            var computedIp = $"{triplet.Ip1}.{triplet.Ip2}.{triplet.Ip3}.{triplet.Ip4}";
            if (uniqueIps.Contains(computedIp))
            {
                continue;
            }
            
            uniqueIps.Add(computedIp);
        }

        if (uniqueIps.Count == 0)
        {
            return [];
        }

        var newTriplets = new List<LoginTriplet>();
        var addedIds = new List<int>();
        foreach (var ip in uniqueIps)
        {
            var newTrips = GetConnectionsByIp(ip);

            foreach (var triplet in newTrips)
            {
                if (addedIds.Contains(triplet.Id))
                {
                    continue;
                }
                
                addedIds.Add(triplet.Id);
                newTriplets.Add(triplet);
            }
        }

        return newTriplets;
    }
    
    public List<LoginTriplet> GetConnectionsWithMatchingCidByCkey(string ckey)
    {
        var allConnections = GetConnectionsByCkey(ckey);

        var uniqueCids = new List<string>();
        foreach (var triplet in allConnections)
        {
            if (uniqueCids.Contains(triplet.LastKnownCid))
            {
                continue;
            }
            
            uniqueCids.Add(triplet.LastKnownCid);
        }

        if (uniqueCids.Count == 0)
        {
            return [];
        }

        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM login_triplets WHERE FIND_IN_SET(last_known_cid, @cids)";
        sqlCommand.Parameters.AddWithValue("@cids", string.Join(",", uniqueCids.ToArray()));

        return AcquireTriplets(sqlCommand);

    }

    private List<LoginTriplet> AcquireTriplets(MySqlCommand sqlCommand)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            sqlCommand.Connection = sqlConnection;

            var triplets = new List<LoginTriplet>();

            using var sqlReader = sqlCommand.ExecuteReader();
            while (sqlReader.Read())
            {
                triplets.Add(new LoginTriplet(
                    Id: sqlReader.GetInt32("id"),
                    Ckey: sqlReader.GetString("ckey"),
                    Ip1: sqlReader.GetInt32("ip1"),
                    Ip2: sqlReader.GetInt32("ip2"),
                    Ip3: sqlReader.GetInt32("ip3"),
                    Ip4: sqlReader.GetInt32("ip4"),
                    LastKnownCid: sqlReader.GetString("last_known_cid"),
                    LoginDate: sqlReader.GetDateTime("login_date")
                ));
            }
                
            sqlConnection.Close();

            return triplets;
            
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return new List<LoginTriplet>();

    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(configuration.GetConnectionString("mysql"));
    }
    
    private static bool GetBoolNullSafe(MySqlDataReader reader, string column)
    {
        return !reader.IsDBNull(reader.GetOrdinal(column)) && reader.GetBoolean(column);
    }
    private static string? GetStringNullSafe(MySqlDataReader reader, string column)
    {
        return reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetString(column);
    }

    private static Int32? GetInt32NullSafe(MySqlDataReader reader, string column)
    {
        return reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetInt32(column);
    }

    private static Int64? GetInt64NullSafe(MySqlDataReader reader, string column)
    {
        return reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetInt64(column);
    }
    
}

public interface IDatabase
{
    Player? GetPlayer(int id);
    Player? GetPlayer(string ckey);

    List<LoginTriplet> GetConnectionsByCid(string cid);
    List<LoginTriplet> GetConnectionsByIp(string ip);
    List<LoginTriplet> GetConnectionsByCkey(string ckey);

    List<LoginTriplet> GetConnectionsWithMatchingCidByCkey(string ckey);
    List<LoginTriplet> GetConnectionsWithMatchingIpByCkey(string ckey);

    IEnumerable<PlayerNote>? GetPlayerNotes(int id);
}