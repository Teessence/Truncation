using Microsoft.Data.Sqlite;

namespace Truncation
{
    public class Database
    {
        public static void SaveCharacter(char c, int IdOfLanguage)
        {
            string connectionString = $"Data Source=Database.db;";  // Your SQLite connection string

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    int characterId = GetCharacterId(connection, c);

                    if (characterId == -1)
                    {
                        characterId = InsertCharacter(connection, c);
                    }

                    InsertCharacterLanguageMapping(connection, characterId, IdOfLanguage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static int GetCharacterId(SqliteConnection connection, char c)
        {
            string query = "SELECT Id FROM Characters WHERE Character = @Character";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Character", c);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;  // Return -1 if not found
            }
        }

        public static int InsertCharacter(SqliteConnection connection, char c)
        {
            string query = "INSERT INTO Characters (Character) VALUES (@Character); SELECT last_insert_rowid();";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Character", c);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result); 
            }
        }

        public static void InsertCharacterLanguageMapping(SqliteConnection connection, int characterId, int languageId)
        {
            string query = "INSERT INTO CharacterLanguageMapping (CharacterId, LanguageId) VALUES (@CharacterId, @LanguageId)";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CharacterId", characterId);
                command.Parameters.AddWithValue("@LanguageId", languageId);

                int rowsAffected = command.ExecuteNonQuery();
            }
        }

        public static int GetIdOfLanguage(string Language)
        {
            string connectionString = $"Data Source=Database.db;";

            int languageId = -1;

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Id FROM TesseractLanguages WHERE LanguageCode = @LanguageCode;";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LanguageCode", Language);

                        var result = command.ExecuteScalar();

                        if (result != null)
                        {
                            languageId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return languageId;
        }

        public static void InsertIntoTesseractLanguages(string languageCode, int priority)
        {
            string connectionString = $"Data Source=Database.db;";

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO TesseractLanguages (LanguageCode, Priority) VALUES (@LanguageCode, @Priority);";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LanguageCode", languageCode);
                        command.Parameters.AddWithValue("@Priority", priority);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} row(s) inserted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
