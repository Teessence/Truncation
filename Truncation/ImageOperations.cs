using System.Drawing;

namespace Truncation
{
    public class ImageOperations
    {
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

        public static byte[] ImageToByteArray(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}