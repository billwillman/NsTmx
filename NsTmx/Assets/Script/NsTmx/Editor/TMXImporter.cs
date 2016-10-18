using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEditor;
using TmxCSharp.Loader;
using TmxCSharp.Models;

public class TMXImporterLoader: ITmxLoader
{
	public string _LoadMapXml(string fileName)
	{
		if (!File.Exists (fileName))
			return string.Empty;
		string ret = string.Empty;
		string fullPath = Path.GetFullPath (fileName);
		FileStream stream = new FileStream (fullPath, FileMode.Open);
		if (stream.Length > 0) {
			byte[] buf = new byte[stream.Length];
			stream.Read (buf, 0, buf.Length);
			ret = System.Text.Encoding.UTF8.GetString (buf);
		}
		stream.Close ();
		stream.Dispose ();

		return ret;
	}

	public byte[] _LoadBinary(string fileName)
	{
		TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset> (fileName);
		if (text == null)
			return null;
		return text.bytes;
	}

	public Material _LoadMaterial (string fileName)
	{
		Material ret = AssetDatabase.LoadAssetAtPath<Material> (fileName);
		return ret;
	}

	public void _DestroyResource(UnityEngine.Object res)
	{}
}

// TMX导入
public class TMXImporter: AssetPostprocessor {

	private static TMXImporterLoader m_Loader = new TMXImporterLoader();

	public static void OnPostprocessAllAssets(string[]importedAsset,string[] deletedAssets,string[] movedAssets,string[]movedFromAssetPaths)
	{
		if (importedAsset != null) {
			bool isChanged = false;
			for (int i = 0; i < importedAsset.Length; ++i) {
				string fileName = importedAsset [i];
				string ext = Path.GetExtension (fileName);
				if (string.Compare (ext, ".tmx", true) == 0) {
					TileMap map = TmxLoader.Parse (fileName, m_Loader);
					if (map != null && map.IsVaild) {
						fileName = Path.ChangeExtension (fileName, ".bytes");
						TmxLoader.SaveToBinary (fileName, map);
						isChanged = true;
					} else {
						Debug.LogErrorFormat ("Import TMX: {0} is Error!", Path.GetFileName (fileName));
					}
				}
			}

			if (isChanged)
				AssetDatabase.Refresh ();
		}
	}
}
