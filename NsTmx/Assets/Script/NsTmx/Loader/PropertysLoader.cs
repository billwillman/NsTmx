using System;
using XmlParser;
using TmxCSharp.Models;

namespace TmxCSharp.Loader
{
	internal static class PropertysLoader
	{
		public static Propertys LoadPropertys(XMLNode parent)
		{
			if (parent == null)
				return null;

			Propertys ret = null;

			XMLNodeList propNodeList = parent.GetNodeList ("properties");
			if (propNodeList == null || propNodeList.Count <= 0)
				return null;

			for (int i = 0; i < propNodeList.Count; ++i) {
				XMLNode node = propNodeList [i] as XMLNode;
				if (node == null)
					continue;

				XMLNodeList props = node.GetNodeList ("property");
				if (props != null && props.Count > 0) {
					for (int j = 0; j < props.Count; ++j) {
						XMLNode propNode = props [j] as XMLNode;
						if (propNode == null)
							continue;

						string name = propNode.GetValue ("@name");
						if (string.IsNullOrEmpty (name))
							continue;
						string value = propNode.GetValue ("@value");
						if (ret == null)
							ret = new Propertys ();
						Property prop = new Property (name, value);
						ret.AddProp (prop);
					}
				}
			}

			return ret;
		}

	}
}