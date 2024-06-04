using System.Text.RegularExpressions;
using CmApi.Records;
using MySql.Data.MySqlClient;

namespace CmApi.Classes;

public partial class Database(IConfiguration configuration) : IDatabase
{

    private IDictionary<int, string> _shallowCacheToCkey = new Dictionary<int, string>();
    private IDictionary<string, int> _shallowCacheToId = new Dictionary<string, int>();

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
                    sqlConnection.Close();
                    return null;
                }
                
                dataReader.Read();

                var gottenId = dataReader.GetInt32("id");

                // if our player got made, but not actually by a player existing (ie, a note lookup)
                if (dataReader.IsDBNull(dataReader.GetOrdinal("last_login")))
                {
                    sqlConnection.Close();
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

                var discordId = GetDiscordLink(gottenId)?.DiscordId.ToString();

                var ckey = dataReader.GetString("ckey");

                user = new Player(
                    Id: gottenId,
                    Ckey: ckey,
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
                
                _shallowCacheToCkey[gottenId] = ckey;
                _shallowCacheToId[ckey] = gottenId;

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

    public IEnumerable<PlayerNote> GetPlayerNotes(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM player_notes WHERE player_id = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquirePlayerNotes(sqlCommand);
    }

    public IEnumerable<PlayerNote> GetAppliedPlayerNotes(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM player_notes WHERE admin_id = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquirePlayerNotes(sqlCommand); 
    }

    private IEnumerable<PlayerNote> AcquirePlayerNotes(MySqlCommand sqlCommand)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            sqlCommand.Connection = sqlConnection;

            var notes = new List<PlayerNote>();

            using (var sqlReader = sqlCommand.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var notingAdminId = sqlReader.GetInt32("admin_id");
                    var notingAdmin = ShallowPlayerName(notingAdminId);

                    var notedPlayerId = sqlReader.GetInt32("player_id");
                    var notedPlayer = ShallowPlayerName(notedPlayerId);
                    
                    var note = new PlayerNote(
                        Id: sqlReader.GetInt32("id"),
                        PlayerId: notedPlayerId,
                        AdminId: notingAdminId,
                        Text: GetStringNullSafe(sqlReader, "text"),
                        Date: sqlReader.GetString("date"),
                        IsBan: sqlReader.GetBoolean("is_ban"),
                        BanTime: GetInt64NullSafe(sqlReader, "ban_time"),
                        IsConfidential: sqlReader.GetBoolean("is_confidential"),
                        AdminRank: sqlReader.GetString("admin_rank"),
                        NoteCategory: GetInt32NullSafe(sqlReader, "note_category"),
                        RoundId: GetInt32NullSafe(sqlReader, "round_id"),
                        NotedPlayerCkey: notedPlayer,
                        NotingAdminCkey: notingAdmin
                    );

                    notes.Add(note);
                }
                
            }
            
            sqlConnection.Close();
            return notes;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
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

                var banningAdminId = GetInt32NullSafe(sqlReader, "admin_id");
                if (!banningAdminId.HasValue)
                {
                    continue;
                }
                
                var banningAdminCkey = ShallowPlayerName(banningAdminId.Value);
                    
                var ban = new PlayerJobBan(
                    Id: sqlReader.GetInt32("id"),
                    PlayerId: sqlReader.GetInt32("player_id"),
                    AdminId: banningAdminId,
                    Text: sqlReader.GetString("text"),
                    Date: GetStringNullSafe(sqlReader, "date"),
                    BanTime: GetInt32NullSafe(sqlReader, "ban_time"),
                    Expiration: GetInt64NullSafe(sqlReader, "expiration"),
                    Role: sqlReader.GetString("role"),
                    BanningAdminCkey: banningAdminCkey
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

    public string? ShallowPlayerName(int id)
    {

        string? cached;
        _shallowCacheToCkey.TryGetValue(id, out cached);
        if (cached != null)
        {
            return cached;
        }
        
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"SELECT ckey FROM players WHERE id = @id";
            sqlCommand.Parameters.AddWithValue("@id", id);

            using var sqlReader = sqlCommand.ExecuteReader();
            if (!sqlReader.HasRows)
            {
                sqlReader.Close();
                return null;
            }
                
            sqlReader.Read();
            var ckey = sqlReader.GetString("ckey");
            sqlConnection.Close();
            _shallowCacheToCkey[id] = ckey;
            _shallowCacheToId[ckey] = id;
            return ckey;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return null;
    }
    
    public int? ShallowPlayerId(string ckey)
    {

        // looked at opendream impl for this: https://github.com/OpenDreamProject/OpenDream/blob/ada468d5209809bbee8dddef68415730d13c7937/OpenDreamRuntime/Procs/Native/DreamProcNativeHelpers.cs#L407
        ckey = CkeyRegex().Replace(ckey.ToLower(), "");
        
        var found = _shallowCacheToId.TryGetValue(ckey, out var cached);
        if (found)
        {
            return cached;
        }
        
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"SELECT id FROM players WHERE ckey = @ckey";
            sqlCommand.Parameters.AddWithValue("@ckey", ckey);

            using var sqlReader = sqlCommand.ExecuteReader();
            if (!sqlReader.HasRows)
            {
                sqlReader.Close();
                return null;
            }
                
            sqlReader.Read();
            var id = sqlReader.GetInt32("id");
            sqlConnection.Close();
            _shallowCacheToCkey[id] = ckey;
            _shallowCacheToId[ckey] = id;
            return id;
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
                
                    
                }
                
                sqlConnection.Close();
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
            return [];
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

        var uniqueIps = new HashSet<(int, int, int, int)>();
        foreach (var triplet in allConnections)
        {
            uniqueIps.Add((triplet.Ip1, triplet.Ip2, triplet.Ip3, triplet.Ip4));
        }

        if (uniqueIps.Count == 0)
        {
            return [];
        }

        var command = new List<string>();

        foreach (var ip in uniqueIps)
        {
            command.Add($"(ip1 = {ip.Item1} AND ip2 = {ip.Item2} AND ip3 = {ip.Item3} AND ip4 = {ip.Item4})");
        }

        var commandString = String.Join(" OR ", command);
        
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @$"SELECT * FROM login_triplets WHERE {commandString}";

        return AcquireTriplets(sqlCommand);

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
    
    public List<Stickyban> GetStickybans()
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban";

        return AcquireStickyban(sqlCommand);
    }
    
    public List<Stickyban> GetStickybanWithMatchingCid(string cid)
    {
        var stickyIds = new HashSet<int>();
        foreach (var stickybanMatch in GetStickybanMatchedCidsByCid(cid))
        {
            stickyIds.Add(stickybanMatch.LinkedStickyban);
        }

        return AcquireStickybansById(stickyIds);
    }

    public List<Stickyban> GetStickybanWithMatchingCkey(string ckey)
    {
        var stickyIds = new HashSet<int>();
        foreach (var stickybanMatch in GetStickybanMatchedCkeysByCkey(ckey))
        {
            stickyIds.Add(stickybanMatch.LinkedStickyban);
        }

        return AcquireStickybansById(stickyIds);
    }

    public List<Stickyban> GetStickybanWithMatchingIp(string ip)
    {
        var stickyIds = new HashSet<int>();
        foreach (var stickybanMatch in GetStickybanMatchedIpsByIp(ip))
        {
            stickyIds.Add(stickybanMatch.LinkedStickyban);
        }

        return AcquireStickybansById(stickyIds);
    }

    private List<Stickyban> AcquireStickybansById(IEnumerable<int> ids)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban WHERE FIND_IN_SET(id, @ids) AND active = 1";
        sqlCommand.Parameters.AddWithValue("@ids", string.Join(",", ids.ToArray()));
        
        return AcquireStickyban(sqlCommand);
    }
    
    private List<Stickyban> AcquireStickyban(MySqlCommand command)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            command.Connection = sqlConnection;
            
            var stickybans = new List<Stickyban>();

            using var sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                stickybans.Add(ReadStickyban(sqlReader));
            }
            
            sqlConnection.Close();
            return stickybans;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
    }

    public List<StickybanMatchedCid> GetStickybanMatchedCids(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_cid WHERE linked_stickyban = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquireStickybanMatchedCid(sqlCommand);
    }
    
    public List<StickybanMatchedCkey> GetStickybanMatchedCkeys(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_ckey WHERE linked_stickyban = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquireStickybanMatchedCkey(sqlCommand);

    }
    
    public List<StickybanMatchedIp> GetStickybanMatchedIps(int id)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_ip WHERE linked_stickyban = @id";
        sqlCommand.Parameters.AddWithValue("@id", id);

        return AcquireStickybanMatchedIp(sqlCommand);
    }

    public List<StickybanMatchedCid> GetStickybanMatchedCidsByCid(string cid)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_cid WHERE cid = @cid";
        sqlCommand.Parameters.AddWithValue("@cid", cid);

        return AcquireStickybanMatchedCid(sqlCommand);
    }
    
    public List<StickybanMatchedCkey> GetStickybanMatchedCkeysByCkey(string ckey)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_ckey WHERE ckey = @ckey AND whitelisted = 0";
        sqlCommand.Parameters.AddWithValue("@ckey", ckey);

        return AcquireStickybanMatchedCkey(sqlCommand);
    }
    
    public List<StickybanMatchedIp> GetStickybanMatchedIpsByIp(string ip)
    {
        var sqlCommand = new MySqlCommand();
        sqlCommand.CommandText = @"SELECT * FROM stickyban_matched_ip WHERE ip = @ip";
        sqlCommand.Parameters.AddWithValue("@ip", ip);

        return AcquireStickybanMatchedIp(sqlCommand);
    }

    private List<StickybanMatchedCid> AcquireStickybanMatchedCid(MySqlCommand command)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            command.Connection = sqlConnection;
            
            var stickybans = new List<StickybanMatchedCid>();

            using var sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                stickybans.Add(ReadStickybanMatchedCid(sqlReader));
            }
            
            sqlConnection.Close();
            return stickybans;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
    }
    
    private List<StickybanMatchedCkey> AcquireStickybanMatchedCkey(MySqlCommand command)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            command.Connection = sqlConnection;
            
            var stickybans = new List<StickybanMatchedCkey>();

            using var sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                stickybans.Add(ReadStickybanMatchedCkey(sqlReader));
            }
            
            sqlConnection.Close();
            return stickybans;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
    }
    
    private List<StickybanMatchedIp> AcquireStickybanMatchedIp(MySqlCommand command)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            command.Connection = sqlConnection;
            
            var stickybans = new List<StickybanMatchedIp>();

            using var sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                stickybans.Add(ReadStickybanMatchedIp(sqlReader));
            }
            
            sqlConnection.Close();
            return stickybans;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
    }

    private Stickyban ReadStickyban(MySqlDataReader reader)
    {
        var adminId = GetInt32NullSafe(reader, "adminid");
        string? adminCkey = null;
        if (adminId.HasValue)
        {
            adminCkey = ShallowPlayerName(adminId.Value);
        }
                
        return new Stickyban
        {
            Id = reader.GetInt32("id"),
            Identifier = reader.GetString("identifier"),
            Reason = reader.GetString("reason"),
            Message = reader.GetString("message"),
            Date = reader.GetString("date"),
            Active = reader.GetBoolean("active"),
            AdminId = adminId,
            AdminCkey = adminCkey
        };
    }

    private StickybanMatchedCid ReadStickybanMatchedCid(MySqlDataReader reader)
    {
        return new StickybanMatchedCid
        {
            Id = reader.GetInt32("id"),
            Cid = reader.GetString("cid"),
            LinkedStickyban = reader.GetInt32("linked_stickyban")
        };
    }

    private StickybanMatchedCkey ReadStickybanMatchedCkey(MySqlDataReader reader)
    {
        return new StickybanMatchedCkey{
            Id = reader.GetInt32("id"),
            Ckey = reader.GetString("ckey"),
            LinkedStickyban = reader.GetInt32("linked_stickyban"),
            Whitelisted = reader.GetBoolean("whitelisted")
        };
    }

    private StickybanMatchedIp ReadStickybanMatchedIp(MySqlDataReader reader)
    {
        return new StickybanMatchedIp{
            Id = reader.GetInt32("id"),
            Ip = reader.GetString("ip"),
            LinkedStickyban = reader.GetInt32("linked_stickyban")
        };
    }

    public int StickybanWhitelistCkey(string ckey)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = @"UPDATE stickyban_matched_ckey SET whitelisted = 1 WHERE ckey = @ckey AND whitelisted = 0";
            sqlCommand.Parameters.AddWithValue("@ckey", ckey);
            
            var rows = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();

            return rows;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return 0;
    }
    
    public bool CreateNote(string ckey, string adminCkey, string text, bool confidential = false, int noteCategory = 1,
        bool isBan = false)
    {
        var playerId = ShallowPlayerId(ckey);
        if (!playerId.HasValue)
        {
            return false;
        }

        return CreateNote(playerId.Value, adminCkey, text, confidential, noteCategory, isBan);
    }
    
    public bool CreateNote(int playerId, string adminCkey, string text, bool confidential = false, int noteCategory = 1, bool isBan = false)
    {
        var adminId = ShallowPlayerId(adminCkey);

        if (!adminId.HasValue)
        {
            return false;
        }
        
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            var sqlCommand = new MySqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = 
                """
                INSERT INTO player_notes (player_id, admin_id, text, date, is_ban, is_confidential, admin_rank, note_category)
                VALUES (@player_id, @admin_id, @text, @date, @is_ban, @is_confidential, @admin_rank, @note_category)
                """;

            sqlCommand.Parameters.AddWithValue("@player_id", playerId);
            sqlCommand.Parameters.AddWithValue("@admin_id", adminId);
            sqlCommand.Parameters.AddWithValue("@text", text);
            sqlCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            sqlCommand.Parameters.AddWithValue("@is_ban", isBan ? 1 : 0);
            sqlCommand.Parameters.AddWithValue("@is_confidential", confidential ? 1 : 0);
            sqlCommand.Parameters.AddWithValue("@admin_rank", "[cmdb]");
            sqlCommand.Parameters.AddWithValue("@note_category", noteCategory);
            
            var rows = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();

            return rows > 0;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }
        
        return true;
    }

    public List<Ticket> GetRecentTickets()
    {
        return [];
    }

    private List<Ticket> AcquireTicket(MySqlCommand command)
    {
        try
        {
            var sqlConnection = GetConnection();
            sqlConnection.Open();

            command.Connection = sqlConnection;
            
            var tickets = new List<Ticket>();

            using var sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                tickets.Add(new Ticket
                {
                    Id = sqlReader.GetInt32("id"),
                    TicketId = sqlReader.GetInt32("ticket"),
                    Action = sqlReader.GetString("action"),
                    Message = sqlReader.GetString("message"),
                    Recipient = GetStringNullSafe(sqlReader, "recipient"),
                    Sender = sqlReader.GetString("sender"),
                    RoundId = GetInt32NullSafe(sqlReader, "round_id"),
                    Time = sqlReader.GetDateTime("time"),
                    Urgent = sqlReader.GetBoolean("urgent")
                });
            }
            
            sqlConnection.Close();
            return tickets;
        }
        catch (MySqlException exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        return [];
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

    [GeneratedRegex("[\\^]|[^a-z0-9@]")]
    private static partial Regex CkeyRegex();
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

    List<Stickyban> GetStickybans();

    List<Stickyban> GetStickybanWithMatchingCid(string cid);
    List<Stickyban> GetStickybanWithMatchingCkey(string ckey);
    List<Stickyban> GetStickybanWithMatchingIp(string ip);

    List<StickybanMatchedCid> GetStickybanMatchedCids(int id);
    List<StickybanMatchedCkey> GetStickybanMatchedCkeys(int id);
    List<StickybanMatchedIp> GetStickybanMatchedIps(int id);
    
    List<StickybanMatchedCid> GetStickybanMatchedCidsByCid(string cid);
    List<StickybanMatchedCkey> GetStickybanMatchedCkeysByCkey(string ckey);
    List<StickybanMatchedIp> GetStickybanMatchedIpsByIp(string ip);

    string? ShallowPlayerName(int id);
    int? ShallowPlayerId(string ckey);

    bool CreateNote(int playerId, string adminCkey, string text, bool confidential = false, int noteCategory = 1, bool isBan = false);
    bool CreateNote(string ckey, string adminCkey, string text, bool confidential = false, int noteCategory = 1, bool isBan = false);

    List<Ticket> GetRecentTickets();
    
    int StickybanWhitelistCkey(string ckey);
    

    IEnumerable<PlayerNote> GetPlayerNotes(int id);
    IEnumerable<PlayerNote> GetAppliedPlayerNotes(int id);
}