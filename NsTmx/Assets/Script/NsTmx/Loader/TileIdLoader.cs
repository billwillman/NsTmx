using System;
using System.Collections.Generic;
using System.IO;
using TmxCSharp.Models;
using XmlParser;
using UnityEngine;
using Utils;

namespace TmxCSharp.Loader
{
    internal class TileIdLoader
    {
        private readonly TileMapSize _size;
        private readonly int _expectedIds;
        
        public TileIdLoader(TileMapSize tileMapSize)
        {
            if (tileMapSize == null)
            {
				#if DEBUG
				Debug.LogError("tileMapSize is null");
				#endif
                return;
            }

            _size = tileMapSize;
            _expectedIds = tileMapSize.Width * tileMapSize.Height;
        }

		public TileMapSize Size
		{
			get
			{
				return _size;
			}
		}

		public static void SaveToBinary(Stream stream, MapLayer layer, TileMapSize size)
		{
			if (stream == null || layer == null || size == null)
				return;

			int cnt = size.Width * size.Height;
			FilePathMgr.Instance.WriteInt (stream, cnt);

			for (int y = 0; y < size.Height; y++)
			{
				for (int x = 0; x < size.Width; x++)
				{
					int idx = y * size.Width + x;
					int tileId;
					//if (idx >= layer.TileIds.Count)
					//{
					//	tileId = 0;
					//} else
					{
						TileIdData data = layer.TileIds[idx];
						tileId = GetTileDataToId(data);
					}
					FilePathMgr.Instance.WriteInt(stream, tileId);
				}
			}

		}

		public void LoadLayer(MapLayer mapLayer, Stream stream)
		{
			if (mapLayer == null || stream == null || stream.Length <= 0)
				return;
			
			ApplyIds(GetMapIdsFromBinary(stream), mapLayer);
		}

        public void LoadLayer(MapLayer mapLayer, XMLNode layerData)
        {
            if (mapLayer == null)
            {
                throw new ArgumentNullException("mapLayer");
            }

            if (layerData == null)
            {
                throw new System.Exception("Layer does not have a data element");
            }

            string encoding = layerData.GetValue("@encoding");

            switch (encoding)
            {
                case "base64":
                    string dataStr = layerData.GetValue("_text");

                    ApplyIds(GetMapIdsFromBase64(dataStr, layerData.GetValue("@compression")), mapLayer);
                    break;

                case "csv":
                    ApplyIds(ParseCsvData(layerData), mapLayer);
                    break;

                default:
                    if (string.IsNullOrEmpty(encoding))
                    {
                        XMLNodeList tileList = layerData.GetNodeList("tile");
                        ApplyIds(GetMapIdsFromXml(tileList), mapLayer);
                    }
                    else
                    {
					#if DEBUG
					Debug.LogError("Unsupported layer data encoding (expected base64 or csv)");
                    #endif
					}
                    break;
            }
        }

		private IList<TileIdData> GetMapIdsFromBase64(string value, string compression)
        {
            return GetMapIdsFromBytes(Decompression.Decompress(compression, Convert.FromBase64String(value)));
        }

		private IList<TileIdData> GetMapIdsFromBinary(Stream stream)
		{
			int cnt = FilePathMgr.Instance.ReadInt(stream);
			if (cnt <= 0)
				return null;

			IList<TileIdData> ret = new List<TileIdData>(cnt);
			for (int i = 0; i < cnt; ++i)
			{
				int gid = FilePathMgr.Instance.ReadInt(stream);
				TileIdData data = GetTileData(gid);
				ret.Add(data);
			}

			return ret;
		}

		private IList<TileIdData> GetMapIdsFromXml(XMLNodeList tiles)
        {
			IList<TileIdData> ret = null;

            for (int i = 0; i < tiles.Count; ++i)
            {
                XMLNode tile = tiles[i] as XMLNode;
                if (tile == null)
                    continue;
                string s = tile.GetValue("@gid");
                if (string.IsNullOrEmpty(s))
                    continue;
                int gid;
                if (!int.TryParse(s, out gid))
                    continue;
                if (ret == null)
					ret = new List<TileIdData>();

				TileIdData data = GetTileData(gid);
				ret.Add(data);
            }

            return ret;
        }

		private void ApplyIds(IList<TileIdData> ids, MapLayer layer)
        {
			layer.TileIds = ids;
        }

		private IList<TileIdData> ParseCsvData(string str)
		{
			if (string.IsNullOrEmpty(str))
				return null;

			string[] ss = str.Split(new char[] {','});
			if (ss == null || ss.Length <= 0)
				return null;

			IList<TileIdData> ret = null;
			for (int i = 0; i < ss.Length; ++i)
			{
				string s = ss[i];
				if (string.IsNullOrEmpty(s))
					continue;
				int idx;
				if (int.TryParse(s, out idx))
				{
					if (ret == null)
						ret = new List<TileIdData>();

					TileIdData data = GetTileData(idx);

					ret.Add(data);
				}
			}

			return ret;
		}

		private IList<TileIdData> ParseCsvData(XMLNode layerData)
        {
            if (layerData == null)
                return null;

            string str = layerData.GetValue("_text");
			return ParseCsvData(str);
        }

		private IList<TileIdData> GetMapIdsFromBytes(byte[] decompressedData)
        {
            int expectedBytes = _expectedIds * 4;

            if (decompressedData.Length != expectedBytes)
            {
				#if DEBUG
				Debug.LogError("Decompressed data is not identical in size to map");
                #endif
				return null;
            }

			IList<TileIdData> ret = null;
            for (int tileIndex = 0; tileIndex < expectedBytes; tileIndex += 4)
            {
				TileIdData data = GetTileId(decompressedData, tileIndex);
                if (ret == null)
					ret = new List<TileIdData>();
                ret.Add(data);
            }

            return ret;
        }

		const uint flippedHorizontallyFlag = 0x80000000;
		const uint flippedVerticallyFlag = 0x40000000;
        const uint flippedDiagonallyFlag = 0x20000000;

		public static int GetTileDataToId(TileIdData data)
		{
			uint ret = (uint)data.tileId;
			if (data.isFlipX)
				ret |= flippedHorizontallyFlag;

			if (data.isFlipY)
				ret |= flippedVerticallyFlag;

            if (data.isRot)
                ret |= flippedDiagonallyFlag;

			return ((int)ret);
		}

		public static TileIdData GetTileData(int tileId)
		{

			const uint flipMask = ~(flippedHorizontallyFlag | flippedVerticallyFlag | flippedDiagonallyFlag);

			bool flippedHorizontally = (tileId & flippedHorizontallyFlag) > 0;
			bool flippedVertically = (tileId & flippedVerticallyFlag) > 0;
			bool flippedDiagonally = (tileId & flippedDiagonallyFlag) != 0;

			int realTileId = (int)(tileId & flipMask);
			if (realTileId == 0)
			{
				return m_DefaultTileIdData;	
			}

			TileIdData ret = new TileIdData();
			ret.tileId = realTileId;
			ret.isFlipX = flippedHorizontally;
			ret.isFlipY = flippedVertically;
            ret.isRot = flippedDiagonally;

			return ret;
		}

		private static readonly TileIdData m_DefaultTileIdData = new TileIdData(); 

		private static TileIdData GetTileId(byte[] decompressedData, int tileIndex)
        {

            int tileId = decompressedData[tileIndex]
                          | (decompressedData[tileIndex + 1] << 8)
                          | (decompressedData[tileIndex + 2] << 16)
                          | (decompressedData[tileIndex + 3] << 24);

			TileIdData ret = GetTileData(tileId);

			return ret;
        }
    }
}
