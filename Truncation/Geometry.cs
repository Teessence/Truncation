using System.Drawing;

namespace Truncation
{
    public class Geometry
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
    }
}