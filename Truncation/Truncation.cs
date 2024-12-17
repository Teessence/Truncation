using System.Drawing;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace Truncation
{
    public class Truncation
    {
        // TODO
        // Deletes TextSegments which are fully outside of bounds of the screenshots
        // Write tests for it
        public static List<TextSegment> DeleteTextSegmentsFullyOutsideOfBounds(Size ScreenshotSize, List<TextSegment> TextSegments)
        {
            List<TextSegment> TextSegmentsReturnable = [];

            foreach (TextSegment ts in TextSegments)
            {
                if (Geometry.AreTextSegmentsOverlapping(ts, ScreenshotSize))
                {
                    TextSegmentsReturnable.Add(ts);
                }
            }

            return TextSegmentsReturnable;
        }

        // TODO
        // Adjustes TextSegments X, Y, Width and Height values to fall inside the screenshot 
        public static List<TextSegment> AdjustTextSegmentsPartiallyOutsideOfBounds(Size ScreenshotSize, List<TextSegment> TextSegments)
        {
            List<TextSegment> TextSegmentsReturnable = [];
            return TextSegments;
        }

        public static List<TextSegment> RunOpticalCharacterRecognition(string ScreenshotPath, List<TextSegment> TextSegments, List<List<Func<Bitmap, Bitmap>>> FunctionGroups)
        {
            byte[] imageBytes = File.ReadAllBytes(ScreenshotPath);
            List<TextSegment> TextSegmentsReturnable = new List<TextSegment>();

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap originalImage = new Bitmap(ms);

                Parallel.ForEach(TextSegments, ts =>
                {
                    bool IsTextSegmentTruncated = true;

                    Bitmap threadSafeImage;

                    lock (originalImage)
                    {
                        threadSafeImage = new Bitmap(originalImage);
                    }

                    Rectangle cropArea = new Rectangle(ts.X, ts.Y, ts.Width, ts.Height);

                    using (Bitmap croppedImage = threadSafeImage.Clone(cropArea, threadSafeImage.PixelFormat))
                    {
                        foreach (var functionGroup in FunctionGroups)
                        {
                            var nameOfFile = "";

                            Bitmap transformedImage = croppedImage;

                            foreach (var function in functionGroup)
                            {
                                transformedImage = function(transformedImage);
                                nameOfFile += function.Method.Name;
                            }

                            byte[] byteArray = ImageOperations.ImageToByteArray(transformedImage);

                            var ReceivedString = OpticalCharacterRecognition.RunOpticalCharacterRecognition(byteArray, ts.Text);

                            if (!StringOperations.IsTruncated(ts.Text, ReceivedString))
                            {
                                IsTextSegmentTruncated = false;
                                break;
                            }

                            if (nameOfFile == "")
                            {
                                nameOfFile = "original_" + ts.Id.ToString() + "_";
                            }

                            ImageOperations.SaveBitmapToDisk(transformedImage, nameOfFile + ".png");
                        }
                    }

                    if (IsTextSegmentTruncated)
                    {
                        lock (TextSegmentsReturnable)
                        {
                            TextSegmentsReturnable.Add(ts);
                        }
                    }
                });
            }

            return TextSegmentsReturnable;
        }

        public static void testSomething()
        {
            string[] files = Directory.GetFiles(@"C:\tessdata_best-main\tessdata_best-main", "*.traineddata");

            foreach (string file in files)
            {
                Console.WriteLine("file: " + file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine("filename: " + fileName);
                InsertIntoTesseractLanguages(fileName, 0);
            }


            string[] filesTraining = Directory.GetFiles(@"Training", "*.txt");
            foreach (string file in filesTraining)
            {
                Console.WriteLine("trainingfile: " + file);
                string trainingfilefilename = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine("trainingfilefilename: " + trainingfilefilename);

                int IdOfLanguage = GetIdOfLanguage(trainingfilefilename);


                Console.WriteLine("IdOfLanguage: " + IdOfLanguage);


                string fileContents = File.ReadAllText(file);

                foreach (char c in fileContents)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        continue;  // Skip to the next character
                    }
                    SaveCharacter(c, IdOfLanguage);
                    Console.WriteLine(c);
                }

            }
        }

        static void SaveCharacter(char c, int IdOfLanguage)
        {
            string connectionString = $"Data Source=Database.db;";  // Your SQLite connection string

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Check if character exists in the Characters table
                    int characterId = GetCharacterId(connection, c);

                    // If the character does not exist, insert it
                    if (characterId == -1)
                    {
                        characterId = InsertCharacter(connection, c);
                    }

                    // Insert into CharacterLanguageMapping table
                    InsertCharacterLanguageMapping(connection, characterId, IdOfLanguage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Helper function to check if the character exists in the Characters table
        static int GetCharacterId(SqliteConnection connection, char c)
        {
            string query = "SELECT Id FROM Characters WHERE Character = @Character";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Character", c);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;  // Return -1 if not found
            }
        }

        // Helper function to insert a new character into the Characters table
        static int InsertCharacter(SqliteConnection connection, char c)
        {
            string query = "INSERT INTO Characters (Character) VALUES (@Character); SELECT last_insert_rowid();";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Character", c);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);  // Return the newly inserted CharacterId
            }
        }

        // Helper function to insert into the CharacterLanguageMapping table
        static void InsertCharacterLanguageMapping(SqliteConnection connection, int characterId, int languageId)
        {
            string query = "INSERT INTO CharacterLanguageMapping (CharacterId, LanguageId) VALUES (@CharacterId, @LanguageId)";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CharacterId", characterId);
                command.Parameters.AddWithValue("@LanguageId", languageId);

                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} row(s) inserted into CharacterLanguageMapping.");
            }
        }

        static int GetIdOfLanguage(string Language)
        {
            string connectionString = $"Data Source=Database.db;";  // Your connection string

            // Initialize the result ID to -1 (in case the language is not found)
            int languageId = -1;

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to fetch the ID of the language by LanguageCode
                    string query = "SELECT Id FROM TesseractLanguages WHERE LanguageCode = @LanguageCode;";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LanguageCode", Language);

                        // Execute the query and retrieve the result
                        var result = command.ExecuteScalar();

                        // Check if a result was returned
                        if (result != null)
                        {
                            // Parse the result to integer
                            languageId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return the language ID (or -1 if not found)
            return languageId;
        }

        static void InsertIntoTesseractLanguages(string languageCode, int priority)
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

        public static List<int> Analyze(string ScreenshotPath, List<TextSegment> TextSegments)
        {
            testSomething();
            Console.WriteLine("Done2");
            // return null;

            Size ScreenshotSize = ImageOperations.GetScreenshotSize(ScreenshotPath);
            TextSegments = DeleteTextSegmentsFullyOutsideOfBounds(ScreenshotSize, TextSegments);
            TextSegments = AdjustTextSegmentsPartiallyOutsideOfBounds(ScreenshotSize, TextSegments);

            List<List<Func<Bitmap, Bitmap>>> listOfFunctionLists =
            [
                [],
                [ImageOperations.ProcessBitmapToGrayscaleAndInvert, ImageOperations.DoubleBitmapSize],
                [ImageOperations.ProcessBitmapToGrayscaleAndInvert, ImageOperations.DoubleBitmapSize, ImageOperations.RotateImage]
            ];

            TextSegments = RunOpticalCharacterRecognition(ScreenshotPath, TextSegments, listOfFunctionLists);
            return TextSegments.Select(obj => obj.Id).ToList();
        }
    }
}