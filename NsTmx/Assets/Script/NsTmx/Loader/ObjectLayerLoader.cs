using System;
using System.Collections;
using System.Collections.Generic;
using XmlParser;
using TmxCSharp.Models;
using UnityEngine;

namespace TmxCSharp.Loader
{
	internal static class ObjectLayerLoader
	{
		public static IList<ObjectGroup> LoadObjectGroup(XMLNode parent)
		{
			if (parent == null)
				return null;

			XMLNodeList objectNodeList = parent.GetNodeList ("objectgroup");
			if (objectNodeList == null || objectNodeList.Count <= 0)
				return null;

			IList<ObjectGroup> ret = null;
			for (int i = 0; i < objectNodeList.Count; ++i) {
				XMLNode node = objectNodeList [i] as XMLNode;
				if (node == null)
					continue;

				string name = node.GetValue ("@name");
				if (string.IsNullOrEmpty (name))
					continue;
				
				int width;
				string str = node.GetValue ("@width");
				if (!int.TryParse (str, out width))
					width = 0;

				int height;
				str = node.GetValue ("@height");
				if (!int.TryParse (str, out height))
					height = 0;

				if (ret == null)
					ret = new List<ObjectGroup> ();
				ObjectGroup gp = new ObjectGroup (name, width, height);
				LoadObject (node, gp);
				ret.Add (gp);
			}

			return ret;

		}

		private static void LoadObject(XMLNode parent, ObjectGroup gp)
		{
			if (parent == null || gp == null || !gp.IsVaild)
				return;

			XMLNodeList objNodeList =  parent.GetNodeList ("object");
			if (objNodeList == null || objNodeList.Count <= 0)
				return;

			for (int i = 0; i < objNodeList.Count; ++i) {
				XMLNode node = objNodeList [i] as XMLNode;
				if (node == null)
					continue;
				
				string name = node.GetValue ("@name");

				string type = node.GetValue ("@type");

				int x;
				string str = node.GetValue ("@x");
				if (!int.TryParse (str, out x))
					x = 0;

				int y;
				str = node.GetValue ("@y");
				if (!int.TryParse (str, out y))
					y = 0;

				int width;
				str = node.GetValue ("@width");
				if (!int.TryParse (str, out width))
					width = 0;

				int height;
				str = node.GetValue ("@height");
				if (!int.TryParse (str, out height))
					height = 0;

				ObjectLayer layer = new ObjectLayer (name, x, y, width, height, type);
				gp.AddLayer (layer);

				layer.Props = PropertysLoader.LoadPropertys (node);
				layer.Polygon = LoadPolygon(node);
			}

		}

		private static readonly char[] _cVecsSplit = new char[]{' '};
		private static readonly char[] _cVecSplit = new char[]{','};

		private static IList<Vector2> LoadPolygon(XMLNode parent)
		{
			if (parent == null)
				return null;
			XMLNodeList polygonList = parent.GetNodeList("polygon");
			if (polygonList == null || polygonList.Count <= 0)
				return null;

			IList<Vector2> ret = null;

			XMLNode node = polygonList[0] as XMLNode;
			if (node == null)
				return ret;

			string str = node.GetValue("@points");
			if (string.IsNullOrEmpty(str))
				return ret;

			string[] vecsStr = str.Split(_cVecsSplit);
			if (vecsStr == null || vecsStr.Length <= 0)
				return ret;

			for (int i = 0; i < vecsStr.Length; ++i)
			{
				string s = vecsStr[i];
				if (string.IsNullOrEmpty(s))
					continue;

				string[] vec = s.Split(_cVecSplit);
				if (vec == null || vec.Length < 2)
					continue;
				
				string ss = vec[0];
				int x;
				if (!int.TryParse(ss, out x))
					continue;

				ss = vec[1];
				int y;
				if (!int.TryParse(ss, out y))
					continue;

				if (ret == null)
					ret = new List<Vector2>();

				Vector2 vec2 = new Vector2(x, y);
				ret.Add(vec2);
			}

			return ret;
		}

	}
}

