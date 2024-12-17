using Microsoft.Data.Sqlite;
using Tesseract;

namespace Truncation
{
    public class OpticalCharacterRecognition
    {
        public static string GetTesseractEngineLanguageByTargetString(string TargetString)
        {
            TargetString = StringOperations.ReplaceSimilar(TargetString);

            var uniqueCharacters = StringOperations.GetUniqueCharacters(TargetString);

            List<string> TesseractLanguages = [];

            foreach (var character in uniqueCharacters)
            {
                string Language = GetCharacterLanguage(character);

                if (Language.Length > 0)
                {
                    TesseractLanguages.Add(Language);
                }
            }

            TesseractLanguages = TesseractLanguages.Distinct().ToList();
            string TesseractLanguagesString = string.Join("+", TesseractLanguages);
            return TesseractLanguagesString;
        }

        public static string GetCharacterLanguage(char character)
        {
            var returnable = "";

            var queryCharacterId = "SELECT Id FROM Characters WHERE Character = @Character";
            var queryLanguage = @"
            SELECT t.LanguageCode
            FROM CharacterLanguageMapping clm
            JOIN TesseractLanguages t ON clm.LanguageId = t.Id
            WHERE clm.CharacterId = @CharacterId
            ORDER BY t.Priority ASC
            LIMIT 1";

            try
            {
                using (var connection = new SqliteConnection($"Data Source=Database.db;"))
                {
                    connection.Open();

                    int? characterId = null;
                    using (var cmdCharacter = new SqliteCommand(queryCharacterId, connection))
                    {
                        cmdCharacter.Parameters.AddWithValue("@Character", character);
                        var result = cmdCharacter.ExecuteScalar();
                        if (result != null)
                        {
                            characterId = Convert.ToInt32(result);
                        }
                    }

                    if (characterId == null)
                    {
                        return returnable;
                    }

                    using (var cmdLanguage = new SqliteCommand(queryLanguage, connection))
                    {
                        cmdLanguage.Parameters.AddWithValue("@CharacterId", characterId);
                        var result = cmdLanguage.ExecuteScalar();
                        if (result != null)
                        {
                            returnable = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + ex.Message);
            }

            return returnable;
        }

        //TODO
        // replace hardcoded path with Settings one
        public static string RunOpticalCharacterRecognition(byte[] Screenshot, string TargetString)
        {
            string TesseractLanguage = GetTesseractEngineLanguageByTargetString(TargetString);

            if (TesseractLanguage.Length == 0)
            {
                return "";
            }

            Console.WriteLine("Running language: " + TesseractLanguage);

            using (var engine = new TesseractEngine(@"C:\tessdata_best-main\tessdata_best-main", GetTesseractEngineLanguageByTargetString(TesseractLanguage), EngineMode.Default))
            {
                using (var pix = Pix.LoadFromMemory(Screenshot))
                {
                    using (var page = engine.Process(pix))
                    {
                        string text = page.GetText();
                        return text;
                    }
                }
            }
        }
    }
}