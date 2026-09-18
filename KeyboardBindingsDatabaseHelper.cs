using Microsoft.Data.Sqlite;

namespace KeyboardRebind
{
    public class KeyboardBindingsDatabaseHelper
    {

        public sealed record RemapProfile(int id, string name);

        private static SqliteConnection OpenConnection(string database_path)
        {
            var connection = new SqliteConnection($"Data Source={database_path}");
            connection.Open();

            // SQLite requires this for ON DELETE CASCADE to work.
            using var command = connection.CreateCommand();
            command.CommandText = """
            PRAGMA foreign_keys = ON;
            """;

            command.ExecuteNonQuery();
            return connection;
        }

        public static void InitializeDatabase(string database_path)
        {
            using var connection = OpenConnection(database_path);
            using var command = connection.CreateCommand();

            command.CommandText = """
            CREATE TABLE IF NOT EXISTS RemapProfiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS KeyMappings (
                ProfileId INTEGER NOT NULL,
                SourceKeyName TEXT NOT NULL,
                TargetHidCode INTEGER NOT NULL,

                PRIMARY KEY (ProfileId, SourceKeyName),

                FOREIGN KEY (ProfileId)
                    REFERENCES RemapProfiles(Id)
                    ON DELETE CASCADE
            );
            """;

            command.ExecuteNonQuery();
        }

        public static int CreateProfile(Keyboard keyboard, string profile_name)
        {
            if (string.IsNullOrWhiteSpace(profile_name)) {
                throw new ArgumentException("A profile name is required.", nameof(profile_name));
            }

            using var connection = OpenConnection(keyboard.DatabasePath);
            using var command = connection.CreateCommand();

            command.CommandText = """
            INSERT INTO RemapProfiles (Name)
            VALUES ($profileName);

            SELECT last_insert_rowid();
            """;

            command.Parameters.AddWithValue("$profileName", profile_name);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public static List<RemapProfile> GetProfiles(Keyboard keyboard)
        {
            using var connection = OpenConnection(keyboard.DatabasePath);
            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT Id, Name
            FROM RemapProfiles
            ORDER BY Name;
            """;

            using var reader = command.ExecuteReader();
            var profiles = new List<RemapProfile>();
            while (reader.Read()) {
                int profile_id = reader.GetInt32(0);
                string profile_name = reader.GetString(1);
                profiles.Add(new RemapProfile(profile_id, profile_name));
            }

            return profiles;
        }

        public static void LoadModifiedBindings(Keyboard keyboard, int profile_id)
        {
            // Load the base bindings as a "basis" before applying remapping
            keyboard.ModifiedBindings = new Dictionary<string, byte>(keyboard.BaseBindings);

            using var connection = OpenConnection(keyboard.DatabasePath);
            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT SourceKeyName, TargetHidCode
            FROM KeyMappings
            WHERE ProfileId = $profileId;
            """;

            command.Parameters.AddWithValue("$profileId", profile_id);
            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                string source_key_name = reader.GetString(0);
                byte target_hid_code = Convert.ToByte(reader.GetInt32(1));
                keyboard.ModifiedBindings[source_key_name] = target_hid_code;
            }
        }

        public static void SaveModifiedBindings(Keyboard keyboard, int profile_id)
        {
            using var connection = OpenConnection(keyboard.DatabasePath);
            using var transaction = connection.BeginTransaction();

            // Delete the mapping for this profile specifically
            using (var delete_command = connection.CreateCommand()) {
                delete_command.Transaction = transaction;

                delete_command.CommandText = """
                DELETE FROM KeyMappings
                WHERE ProfileId = $profileId;
                """;

                delete_command.Parameters.AddWithValue("$profileId", profile_id);
                delete_command.ExecuteNonQuery();
            }

            foreach (var binding in keyboard.ModifiedBindings) {
                string source_key_name = binding.Key;
                byte target_hid_code = binding.Value;

                // Original mappings do not need database rows.
                if (keyboard.BaseBindings[source_key_name] == target_hid_code) {
                    continue;
                }

                using var insert_command = connection.CreateCommand();
                insert_command.Transaction = transaction;
                insert_command.CommandText = """
                INSERT INTO KeyMappings (
                    ProfileId,
                    SourceKeyName,
                    TargetHidCode
                )
                VALUES (
                    $profileId,
                    $sourceKeyName,
                    $targetHidCode
                );
                """;

                insert_command.Parameters.AddWithValue("$profileId", profile_id);
                insert_command.Parameters.AddWithValue("$sourceKeyName", source_key_name);
                insert_command.Parameters.AddWithValue("$targetHidCode", target_hid_code);
                insert_command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static void DeleteProfile(Keyboard keyboard, int profile_id)
        {
            using var connection = OpenConnection(keyboard.DatabasePath);
            using var command = connection.CreateCommand();

            command.CommandText = """
            DELETE FROM RemapProfiles
            WHERE Id = $profileId;
            """;

            command.Parameters.AddWithValue("$profileId", profile_id);
            command.ExecuteNonQuery();
        }
    }
}
