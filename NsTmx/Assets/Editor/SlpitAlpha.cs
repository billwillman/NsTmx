/*
 * 角色格式修改
 */
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SlpitAlphaTool : Editor
{

	private static bool HasAlphaChannel(Texture2D tex) {
		if (tex == null)
			return false;
		for (int i = 0; i < tex.width; ++i)
			for (int j = 0; j < tex.height; ++j) {
				Color color = tex.GetPixel(i, j);
				float alpha = color.a;
				if (alpha < 1.0f - 0.001f) {
					return true;
				}
			}

		return false;
	}

	[MenuItem("Assets/图片Alpha生成")]
	public static void ProcessPicSplitAlpha() 
    {
		if (Selection.activeObject == null)
			return;

        UnityEngine.Object[] objArr = Selection.objects;
	    if (!objArr.Any())
	    {
	        return;
	    }
	    if (objArr.Length == 1)
	    {
	        Texture2D d = objArr[0] as Texture2D;
	        if (d != null)
	        {
	            CreateAlphaPic(d);
	        }
	    }
	    else
	    {
	        int len = objArr.Length;
            for (int i = 0; i < len; i++)
	        {
                Texture2D d = objArr[i] as Texture2D;
                if (d != null)
                {
                    CreateAlphaPic(d);
                }
	        }
	    }
        
		EditorUtility.UnloadUnusedAssetsImmediate();
		AssetDatabase.Refresh();
	}

    private static void CreateAlphaPic(Texture2D tex)
    {
        if (tex == null)
            return;
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path))
            return;

        TextureImporter import = AssetImporter.GetAtPath(path) as TextureImporter;
        if (import == null)
            return;

        bool isChged = false;
        if (!import.isReadable)
        {
            isChged = true;
            import.isReadable = true;
            import.SaveAndReimport();
        }

        try
        {

            if (!HasAlphaChannel(tex))
                return;

            path = Path.GetFullPath(path);

            Texture2D alphaTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, true);

            for (int i = 0; i < tex.width; ++i)
                for (int j = 0; j < tex.height; ++j)
                {
                    Color color = tex.GetPixel(i, j);
                    Color alphaColor = color;
                    alphaColor.r = color.a;
                    alphaColor.g = color.a;
                    alphaColor.b = color.a;
                    alphaTex.SetPixel(i, j, alphaColor);
                }

            alphaTex.Apply();
            byte[] bytes = alphaTex.EncodeToPNG();

            string fileName = Path.GetFileNameWithoutExtension(path);
            fileName += "_a.png";
            path = Path.GetDirectoryName(path);
            path += "/" + fileName;

            FileStream stream = new FileStream(path, FileMode.Create);
            try
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }

        }
        finally
        {
            if (isChged)
            {
                import.isReadable = false;
                import.SaveAndReimport();
            }
        }
    }
		
}