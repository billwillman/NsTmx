namespace TmxCSharp.Models
{
	public struct TileIdData
	{
		public int tileId;
		public bool isFlipX;
		public bool isFlipY;
	}

    public class MapLayer
    {
        public MapLayer(string name, int width, int height)
        {
            Name = name;

            Width = width;

            Height = height;

			TileIds = new TileIdData[Height, Width];
        }

        public bool IsVaild
        {
            get
            {
                return (Width > 0) && (Height > 0) && (TileIds != null) && (TileIds.Length > 0);
            }
        }

        public string Name { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

		public TileIdData[,] TileIds { get; private set; }
    }
}