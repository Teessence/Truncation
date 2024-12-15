using System.Drawing;
using Tesseract;

namespace Truncation
{
    public class Truncation
    {
        public static bool AreTextSegmentsOverlapping(TextSegment ts, Size s)
        {
            int sX = 0;
            int sY = 0;
            int sWidth = s.Width;
            int sHeight = s.Height;

            if (ts.X + ts.Width < sX || sX + sWidth < ts.X || ts.Y + ts.Height < sY || sY + sHeight < ts.Y)
            {
                return false;
            }

            return true;
        }

        // TODO
        // Deletes TextSegments which are fully outside of bounds of the screenshots
        // Write tests for it
        public static List<TextSegment> DeleteTextSegmentsFullyOutsideOfBounds(Size ScreenshotSize, List<TextSegment> TextSegments)
        {
            List<TextSegment> TextSegmentsReturnable = [];

            foreach (TextSegment ts in TextSegments)
            {
                if (AreTextSegmentsOverlapping(ts, ScreenshotSize))
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

        public static Size GetScreenshotSize(string ScreenshotPath)
        {
            Size returnableSize = new Size(0, 0);

            using (Image pngImage = Image.FromFile(ScreenshotPath))
            {
                returnableSize.Width = pngImage.Width;
                returnableSize.Height = pngImage.Height;
            }

            return returnableSize;
        }

        //TODO
        // Should return values based on characters in targetstring, so if it cotains japanese characters only shuld terun jap etc...
        // if Polish + Japanese should return pol+jap
        // Write tests for it
        public static string GetTesseractEngineLanguageByTargetString(string TargetString)
        {
            return "eng";
        }

        //TODO
        // replace hardcoded path with Settings one
        public static string RunOpticalCharacterRecognition(byte[] Screenshot, string TargetString)
        {
            using (var engine = new TesseractEngine(@"C:\tessdata_best-main\tessdata_best-main", GetTesseractEngineLanguageByTargetString(TargetString), EngineMode.Default))
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

        private static byte[] ImageToByteArray(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        //TODO
        // Replace similar such as i, 1 with l
        // $ with s
        // Do it in some better way instead of one by one.
        public static string ReplaceSimilar(string String)
        {
            String = String.Replace("1", "i", StringComparison.InvariantCultureIgnoreCase);
            String = String.Replace("l", "i", StringComparison.InvariantCultureIgnoreCase);

            return String;
        }

        public static Dictionary<char, int> GetCharacterCounts(string input)
        {
            Dictionary<char, int> characterCounts = new Dictionary<char, int>();

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (characterCounts.ContainsKey(c))
                {
                    characterCounts[c]++;
                }
                else
                {
                    characterCounts[c] = 1;
                }
            }

            return characterCounts;
        }

        public static bool IsDictionarySubset(Dictionary<char, int> smaller, Dictionary<char, int> larger)
        {
            foreach (var pair in smaller)
            {
                char key = pair.Key;
                int count = pair.Value;

                if (!larger.ContainsKey(key) || larger[key] < count)
                {
                    return false;
                }
            }

            return true;
        }

        // TargetString: Text from Screenshot, SourceString: Text from OCR 
        public static bool IsTruncated(string TargetString, string SourceString)
        {
            TargetString = ReplaceSimilar(TargetString);
            SourceString = ReplaceSimilar(SourceString);

            TargetString = TargetString.ToLowerInvariant();
            SourceString = SourceString.ToLowerInvariant();

            Dictionary<char, int> TargetCharacterCounts = GetCharacterCounts(TargetString);
            Dictionary<char, int> SourceCharacterCounts = GetCharacterCounts(SourceString);

            var returnable = !IsDictionarySubset(TargetCharacterCounts, SourceCharacterCounts);

            Console.WriteLine("Automation: " + TargetString);
            Console.WriteLine("OCR: " + SourceString);

            return returnable;
        }

        public static Bitmap ProcessBitmapToGrayscaleAndInvert(Bitmap originalBitmap)
        {
            Bitmap processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);

                    grayValue = 255 - grayValue;

                    Color invertedColor = Color.FromArgb(grayValue, grayValue, grayValue);

                    processedBitmap.SetPixel(x, y, invertedColor);
                }
            }

            return processedBitmap;
        }

        public static Bitmap DoubleBitmapSize(Bitmap originalBitmap)
        {
            Bitmap doubledBitmap = new Bitmap(originalBitmap.Width * 2, originalBitmap.Height * 2);

            using (Graphics g = Graphics.FromImage(doubledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(originalBitmap, 0, 0, doubledBitmap.Width, doubledBitmap.Height);
            }

            return doubledBitmap;
        }

        public static void SaveBitmapToDisk(Bitmap bitmap, string filePath)
        {
            try
            {
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving the image: " + ex.Message);
            }
        }

        public static Bitmap RotateImage(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return image;
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

                            byte[] byteArray = ImageToByteArray(transformedImage);

                            var ReceivedString = RunOpticalCharacterRecognition(byteArray, ts.Text);

                            if (!IsTruncated(ts.Text, ReceivedString))
                            {
                                IsTextSegmentTruncated = false;
                                break;
                            }

                            if (nameOfFile == "")
                            {
                                nameOfFile = "original_" + ts.Id.ToString() + "_";
                            }

                            SaveBitmapToDisk(transformedImage, nameOfFile + ".png");
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

        public static List<int> Analyze(string ScreenshotPath, List<TextSegment> TextSegments)
        {
            Size ScreenshotSize = GetScreenshotSize(ScreenshotPath);
            TextSegments = DeleteTextSegmentsFullyOutsideOfBounds(ScreenshotSize, TextSegments);
            TextSegments = AdjustTextSegmentsPartiallyOutsideOfBounds(ScreenshotSize, TextSegments);

            List<List<Func<Bitmap, Bitmap>>> listOfFunctionLists =
            [
                [],
                [ProcessBitmapToGrayscaleAndInvert, DoubleBitmapSize],
                [ProcessBitmapToGrayscaleAndInvert, DoubleBitmapSize, RotateImage]
            ];

            TextSegments = RunOpticalCharacterRecognition(ScreenshotPath, TextSegments, listOfFunctionLists);
            return TextSegments.Select(obj => obj.Id).ToList();
        }
    }
}