namespace TmxCSharp.Models
{
    public class TileSet
    {
        public TileSet(int firstId, string name, int tileWidth, int tileHeight, TileSetImage image)
        {
            FirstId = firstId;

            Name = name;

            TileWidth = tileWidth;

            TileHeight = tileHeight;

            Image = image;

            LastId = FirstId + ((Image.Width / TileWidth) * (Image.Height / TileHeight)) - 1;
        }

        public bool IsVaid
        {
            get
            {
                return (TileWidth > 0) && (TileHeight > 0) && (Image != null);
            }
        }

        public int FirstId { get; private set; }

        public int LastId { get; private set; }

        public string Name { get; private set; }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public TileSetImage Image { get; private set; }

        public bool ContainsTile(int tileId)
        {
            return (tileId >= FirstId && tileId <= LastId);
        }
    }
}