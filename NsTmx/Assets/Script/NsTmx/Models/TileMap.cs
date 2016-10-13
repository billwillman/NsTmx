using System;
using System.Collections.Generic;
using UnityEngine;

namespace TmxCSharp.Models
{
    public class TileMap
    {
        public TileMap(TileMapSize size, IList<TileSet> tileSets, IList<MapLayer> layers)
        {
            if (size == null)
            {
				#if DEBUG
				Debug.LogError("size is null");
				#endif
				return;
            }

            if (tileSets == null)
            {
				#if DEBUG
				Debug.LogError("tileSets is null");
                #endif
				return;
            }

            if (layers == null)
            {
				#if DEBUG
				Debug.LogError("layers is null");
                #endif
				return;
            }

            Size = size;

            TileSets = tileSets;

            Layers = layers;
        }

        public TileSet FindTileSet(int tileId)
        {
            if (TileSets == null || TileSets.Count <= 0)
                return null;
            for (int i = 0; i < TileSets.Count; ++i)
            {
                TileSet tile = TileSets[i];
                if (tile == null)
                    continue;
                if (tile.ContainsTile(tileId))
                    return tile;
            }

            return null;
        }

        public bool IsVaild
        {
            get
            {
                return (Size != null) && (Size.Width > 0) && (Size.Height > 0) && (TileSets != null) && (TileSets.Count > 0) && (Layers != null) && (Layers.Count > 0);
            }
        }

        public TileMapSize Size { get; private set; }

        public IList<TileSet> TileSets { get; private set; }

        public IList<MapLayer> Layers { get; private set; }
    }
}