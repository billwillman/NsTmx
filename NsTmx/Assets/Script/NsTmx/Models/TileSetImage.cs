namespace TmxCSharp.Models
{
    public class TileSetImage
    {
        public TileSetImage(string filePath, int width, int height)
        {
            FilePath = filePath;

            Width = width;

            Height = height;
        }

        public string FilePath { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }
    }
}