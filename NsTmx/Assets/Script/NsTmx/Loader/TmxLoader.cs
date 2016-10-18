using System.Collections.Generic;
using System.IO;
using TmxCSharp.Models;
using XmlParser;
using UnityEngine;

namespace TmxCSharp.Loader
{
	public interface ITmxLoader
	{
		// 加载地图
		string _LoadMapXml(string fileName);
		byte[] _LoadBinary(string fileName);
		// 加载材质
		Material _LoadMaterial (string fileName);
		void _DestroyResource(UnityEngine.Object res);
	}

    /// <summary>
    /// Used to load .tmx files that were created with Tiled: http://www.mapeditor.org/
    /// </summary>
    public static class TmxLoader
    {
        private const string SupportedVersion = "1.0";
        private const string SupportedOrientation = "orthogonal";

		public static TileMap Parse(string fileName, ITmxLoader loader)
        {
			if (string.IsNullOrEmpty(fileName) || loader == null)
				return null;
			
			string str = loader._LoadMapXml(fileName);

            if (string.IsNullOrEmpty(str))
                return null;

            // 开始解析XML
            XMLParser parser = new XMLParser();
            XMLNode document = parser.Parse(str);
            if (document == null)
                return null;

			m_Loader = loader;

			return Parse(document);
        }

        public static TileMap Parse(XMLNode document)
        {
            if (document == null)
                return null;

            XMLNodeList mapNodeList = document.GetNodeList("map");

            if (mapNodeList == null || mapNodeList.Count <= 0)
                return null;

            if (mapNodeList.Count > 1)
			{
				#if DEBUG
				Debug.LogError("Tmx MapNode only one!");
				#endif
			}

            XMLNode map = mapNodeList[0] as XMLNode;

            if (map == null)
            {
				#if DEBUG
				Debug.LogError("Missing 'map' element");
                #endif
				return null;
            }

            if (!AssertRequirements(map))
                return null;

            TileMapSize tileMapSize = TileMapSizeLoader.LoadTileMapSize(map);

            IList<TileSet> tileSets = TileSetLoader.LoadTileSets(map.GetNodeList("tileset"));

            TileIdLoader tileIdLoader = new TileIdLoader(tileMapSize);

            IList<MapLayer> layers = MapLayerLoader.LoadMapLayers(map, tileIdLoader);

			IList<ObjectGroup> gps = ObjectLayerLoader.LoadObjectGroup (map);

			return new TileMap(tileMapSize, tileSets, layers, gps);
        }

		// 二进制文件读取
		public static TileMap ParseBinary(string fileName, ITmxLoader loader)
		{
			if (string.IsNullOrEmpty(fileName) || loader == null)
				return null;

			byte[] buf = loader._LoadBinary(fileName);
			if (buf == null || buf.Length <= 0)
				return null;

			m_Loader = loader;

			MemoryStream stream = new MemoryStream(buf);
			TileMapSize tileMapSize = TileMapSizeLoader.LoadTileMapSize(stream);
			TileIdLoader tileIdLoader = new TileIdLoader(tileMapSize);
			IList<TileSet> tileSets = TileSetLoader.LoadTileSets(stream);
			IList<MapLayer> layers = MapLayerLoader.LoadMapLayers(stream, tileIdLoader);
			IList<ObjectGroup> gps = ObjectLayerLoader.LoadObjectGroup (stream);

			return new TileMap(tileMapSize, tileSets, layers, gps);
		}

		public static void SaveToBinary(string fileName, TileMap map)
		{
			if (string.IsNullOrEmpty(fileName) || map == null)
				return;

			FileStream stream = new FileStream(fileName, FileMode.Create);
			TileMapSizeLoader.SaveToBinary(stream, map.Size);
			TileSetLoader.SaveToBinary(stream, map.TileSets);
			MapLayerLoader.SaveToBinary(stream, map.Layers, map.Size);
			ObjectLayerLoader.SaveToBinary(stream, map.ObjGroups);

			stream.Close();
			stream.Dispose();
		}


        private static bool AssertRequirements(XMLNode map)
        {
            // 读取版本信息
            string version = map.GetValue("@version");

            if (version != SupportedVersion)
            {
				#if DEBUG
				Debug.LogErrorFormat("Unsupported map version '{0}'. Only version '{1}' is supported.", version, SupportedVersion);
                #endif
				return false;
            }

            string orientation = map.GetValue("@orientation");

            if (orientation != SupportedOrientation)
            {
				#if DEBUG
				Debug.LogErrorFormat("Unsupported orientation '{0}'. Only '{1}' orientation is supported.", orientation, SupportedOrientation);
                #endif
				return false;
            }

            return true;
        }

		public static ITmxLoader Loader
		{
			get
			{
				return m_Loader;
			}
		}

		private static ITmxLoader m_Loader = null;
    }
}