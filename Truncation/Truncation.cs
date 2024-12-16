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
        }

        static void InsertIntoTesseractLanguages(string languageCode, int priority)
        {
            string connectionString = $"Data Source=Database.db;";

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // SQL INSERT statement
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