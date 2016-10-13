using System;
using TmxCSharp.Models;
using XmlParser;

namespace TmxCSharp.Loader
{
    public static class TileMapSizeLoader
    {
        public static TileMapSize LoadTileMapSize(XMLNode map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            string s = map.GetValue("@width");
            int width;
            if (!int.TryParse(s, out width))
                width = 0;

            s = map.GetValue("@height");
            int height;
            if (!int.TryParse(s, out height))
                height = 0;

            s = map.GetValue("@tilewidth");
            int tileWidth;
            if (!int.TryParse(s, out tileWidth))
                tileWidth = 0;

            s = map.GetValue("@tileheight");
            int tileHeight;
            if (!int.TryParse(s, out tileHeight))
                tileHeight = 0;

            return new TileMapSize(width, height, tileWidth, tileHeight);
        }
    }
}
