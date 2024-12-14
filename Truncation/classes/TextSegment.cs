namespace Truncation
{
    public class TextSegment
    {
        public int Id { get; set; }
        public int TextId { get; set; }
        public string Text { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TextSegment(int id, int textId, string text, int x, int y, int width, int height)
        {
            Id = id;
            TextId = textId;
            Text = text;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return $"TextSegment(Id: {Id}, TextId: {TextId}, Text: \"{Text}\", X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
        }
    }
}