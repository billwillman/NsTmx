using System;
using System.Collections.Generic;
using System.IO;
using TmxCSharp.Models;
using XmlParser;
using UnityEngine;
using Utils;

namespace TmxCSharp.Loader
{
    internal static class TileSetLoader
    {

        public static IList<TileSet> LoadTileSets(XMLNodeList tileSets)
        {
            if (tileSets == null || tileSets.Count <= 0)
                return null;

			// not support link tsx
            IList<TileSet> list = null;
            for (int i = 0; i < tileSets.Count; ++i)
            {
                XMLNode tileSet = tileSets[i] as XMLNode;
                if (tileSet == null)
                    continue;
                XMLNodeList imgList = tileSet.GetNodeList("image");
                if (imgList == null || imgList.Count <= 0)
                    continue;
                XMLNode imgNode = imgList[0] as XMLNode;
                if (imgNode == null)
                    continue;
                TileSet tile = GetTileSet(tileSet, GetTileSetImage(imgNode));
                if (tile == null)
                    continue;

                if (list == null)
                    list = new List<TileSet>();
                list.Add(tile);
            }

            return list;
        }

        private static TileSet GetTileSet(XMLNode tileSetDefinition, TileSetImage tileSetImage)
        {
            if (tileSetDefinition == null)
            {
				#if DEBUG
				Debug.LogError("tileSetDefinition is null");
                #endif
				return null;
            }

            if (tileSetImage == null)
            {
				#if DEBUG
				Debug.LogError("tileSetImage is null");
                #endif
				return null;
            }

            string s = tileSetDefinition.GetValue("@firstgid");
            int firstgid;
            if (!int.TryParse(s, out firstgid))
                firstgid = 0;

            string name = tileSetDefinition.GetValue("@name");

            s = tileSetDefinition.GetValue("@tilewidth");
            int tilewidth;
            if (!int.TryParse(s, out tilewidth))
                tilewidth = 0;

            s = tileSetDefinition.GetValue("@tileheight");
            int tileheight;
            if (!int.TryParse(s, out tileheight))
                tileheight = 0;

            return new TileSet(firstgid, name, tilewidth, tileheight, tileSetImage);
        }

		public static IList<TileSet> LoadTileSets(Stream stream)
		{
			if (stream == null || stream.Length <= 0)
				return null;

			int cnt = FilePathMgr.Instance.ReadInt(stream);
			if (cnt <= 0)
				return null;

			List<TileSet> ret = new List<TileSet>(cnt);

			for (int i = 0; i < cnt; ++i)
			{
				int firstgid = FilePathMgr.Instance.ReadInt(stream);
				string name = FilePathMgr.Instance.ReadString(stream);
				int tileWidth = FilePathMgr.Instance.ReadInt(stream);
				int tileHeight = FilePathMgr.Instance.ReadInt(stream);

				string imgSource = FilePathMgr.Instance.ReadString(stream);
				int imgWidth = FilePathMgr.Instance.ReadInt(stream);
				int imgHeight = FilePathMgr.Instance.ReadInt(stream);
				TileSetImage img = new TileSetImage(imgSource, imgWidth, imgHeight);

				TileSet tile = new TileSet(firstgid, name, tileWidth, tileHeight, img);
				ret.Add(tile);
			}

			return ret;
		}

		public static void SaveToBinary(Stream stream, IList<TileSet> list)
		{
			if (stream == null)
				return;

			if (list == null || list.Count <= 0)
			{
				FilePathMgr.Instance.WriteInt(stream, 0);
				return;
			}

			FilePathMgr.Instance.WriteInt(stream, list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				TileSet tile = list[i];
				FilePathMgr.Instance.WriteInt(stream, tile.FirstId);
				FilePathMgr.Instance.WriteString(stream, tile.Name);
				FilePathMgr.Instance.WriteInt(stream, tile.TileWidth);
				FilePathMgr.Instance.WriteInt(stream, tile.TileHeight);
				if (tile.Image == null)
				{
					FilePathMgr.Instance.WriteString(stream, string.Empty);
					FilePathMgr.Instance.WriteInt(stream, 0);
					FilePathMgr.Instance.WriteInt(stream, 0);
				} else
				{
					FilePathMgr.Instance.WriteString(stream, tile.Image.FilePath);
					FilePathMgr.Instance.WriteInt(stream, tile.Image.Width);
					FilePathMgr.Instance.WriteInt(stream, tile.Image.Height);
				}
			}
		}

        private static TileSetImage GetTileSetImage(XMLNode image)
        {
            if (image == null)
            {
				#if DEBUG
				Debug.LogError("Tile set missing image");
                #endif
				return null;
            }

            string source = image.GetValue("@source");

            string s = image.GetValue("@width");
            int width;
            if (!int.TryParse(s, out width))
                width = 0;

            s = image.GetValue("@height");
            int height;
            if (!int.TryParse(s, out height))
                height = 0;

            return new TileSetImage(source, width, height);
        }
    }
}
