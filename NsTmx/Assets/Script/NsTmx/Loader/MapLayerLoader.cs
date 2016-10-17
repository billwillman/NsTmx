using System;
using System.IO;
using System.Collections.Generic;
using TmxCSharp.Models;
using XmlParser;
using UnityEngine;
using Utils;

namespace TmxCSharp.Loader
{
    internal static class MapLayerLoader
    {
        public static IList<MapLayer> LoadMapLayers(XMLNode map, TileIdLoader tileIdLoader)
        {
            if (map == null)
            {
				#if DEBUG
				Debug.LogError("map is null");
				#endif
				return null;
            }

            if (tileIdLoader == null)
            {
				#if DEBUG
				Debug.LogError("tileIdLoader is null");
                #endif
				return null;
            }

            IList<MapLayer> layers = null;

            XMLNodeList layerList = map.GetNodeList("layer");
            if (layerList == null || layerList.Count <= 0)
                return null;

            for (int i = 0; i < layerList.Count; ++i)
            {
                XMLNode layerNode = layerList[i] as XMLNode;
                if (layerNode == null)
                    continue;
                MapLayer mapLayer = GetLayerMetadata(layerNode);
                if (mapLayer == null)
                    continue;
                XMLNodeList dataList = layerNode.GetNodeList("data");
                if (dataList == null || dataList.Count <= 0)
                    continue;
                XMLNode dataNode = dataList[0] as XMLNode;
                if (dataNode == null)
                    continue;
                tileIdLoader.LoadLayer(mapLayer, dataNode);
                if (layers == null)
                    layers = new List<MapLayer>();
                layers.Add(mapLayer);
            }

            return layers;
        }

		public static IList<MapLayer> LoadMapLayers(Stream stream, TileIdLoader tileIdLoader)
		{
			if (stream == null || stream.Length <= 0 || tileIdLoader == null)
				return null;

			int layerCnt = FilePathMgr.Instance.ReadInt(stream);
			if (layerCnt <= 0)
				return null;

			IList<MapLayer> ret = new List<MapLayer>(layerCnt);
			for (int i = 0; i < layerCnt; ++i)
			{
				string name = FilePathMgr.Instance.ReadString(stream);
				int width = FilePathMgr.Instance.ReadInt(stream);
				int height = FilePathMgr.Instance.ReadInt(stream);
				MapLayer mapLayer = new MapLayer(name, width, height);
				ret.Add(mapLayer);

				tileIdLoader.LoadLayer(mapLayer, stream);
			}

			return ret;
		}

        private static MapLayer GetLayerMetadata(XMLNode layer)
        {
            if (layer == null)
                return null;

            string name = layer.GetValue("@name");

            string s = layer.GetValue("@width");
            int width;
            if (!int.TryParse(s, out width))
                width = 0;

            s = layer.GetValue("@height");
            int height;
            if (!int.TryParse(s, out height))
                height = 0;

            return new MapLayer(name, width, height);
        }
    }
}
