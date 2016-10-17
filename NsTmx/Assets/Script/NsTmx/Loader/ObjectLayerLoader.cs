using System;
using System.Collections;
using System.Collections.Generic;
using XmlParser;
using TmxCSharp.Models;

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
			}

		}

	}
}

