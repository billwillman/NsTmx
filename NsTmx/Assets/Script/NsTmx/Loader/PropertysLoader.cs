using System;
using System.IO;
using XmlParser;
using TmxCSharp.Models;
using Utils;

namespace TmxCSharp.Loader
{
	internal static class PropertysLoader
	{
		public static Propertys LoadPropertys(Stream stream)
		{
			if (stream == null || stream.Length <= 0)
				return null;

			int propsCnt = FilePathMgr.Instance.ReadInt(stream);
			if (propsCnt <= 0)
				return null;

			Propertys ret = new Propertys();
			for (int i = 0; i < propsCnt; ++i)
			{
				string name = FilePathMgr.Instance.ReadString(stream);
				string value = FilePathMgr.Instance.ReadString(stream);
				Property prop = new Property(name, value);
				ret.AddProp(prop);
			}

			return ret;
		}

		public static void SaveToBinary(Stream stream, Propertys props)
		{
			if (stream == null)
				return;

			if (props == null)
			{
				FilePathMgr.Instance.WriteInt(stream, 0);
				return;
			}

			FilePathMgr.Instance.WriteInt(stream, props.PropCount);

			var iter = props.GetPropIter();
			while (iter.MoveNext())
			{
				FilePathMgr.Instance.WriteString(stream, iter.Current.Value.Name);
				FilePathMgr.Instance.WriteString(stream, iter.Current.Value.Value);
			}
			iter.Dispose();
		}

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