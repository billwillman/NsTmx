namespace TmxCSharp.Models
{
    public class TileMapSize
    {
        public TileMapSize(int width, int height, int tileWidth, int tileHeight)
        {
            Width = width;
            Height = height;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }
    }
}