using CmApi.Records;
using MySql.Data.MySqlClient;

namespace CmApi.Classes;

public class Database : IDatabase
{ 

    private readonly IConfiguration _configuration;

    public Database(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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
                    TimeBanAdminCkey: timeBanAdmin
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

            using (var sqlReader = sqlCommand.ExecuteReader())
            {
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
            }

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
                sqlCommand.CommandText = @"SELECT * FROM discord_links WHERE id = @id";
                sqlCommand.Parameters.AddWithValue("@id", id);

                using (var sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();

                    var link = new DiscordLink(
                        Id: sqlReader.GetInt32("id"),
                        DiscordId: sqlReader.GetInt32("discord_id"),
                        PlayerId: sqlReader.GetInt32("player_id")
                    );
                    
                    sqlConnection.Close();
                    
                    return link;
                    
                }
            }
            catch (MySqlException exception)
            {
                Console.Error.WriteLine(exception.ToString());
            }

            return null;
        }
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(_configuration.GetConnectionString("mysql"));
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
    IEnumerable<PlayerNote>? GetPlayerNotes(int id);
}