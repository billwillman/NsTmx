using System;
using System.IO;
using UnityEngine;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using TmxCSharp.Renderer;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(TMXRenderer))]
// 简易谢谢
public class ResLoader: MonoBehaviour, ITmxLoader
{
	private TMXRenderer m_Renderer = null;
	private Mesh m_Mesh;

	void Awake()
	{
		m_Renderer = GetComponent<TMXRenderer>();
		m_Mesh = new Mesh();
	}

	void Start()
	{
		if (m_Renderer != null)
		{
			float t = Time.realtimeSinceStartup;
			if (m_Renderer.LoadMapFromBinaryFile("tmx/d107.bytes", this))
			{
				float t1 = Time.realtimeSinceStartup;
				Debug.LogFormat("加载TMX地图时间：{0}", (t1 - t).ToString());
				Vector4 view = new Vector4 (0, 0, 960, 540);
				//m_Renderer.MeshJumpTo(Camera.main);
				m_Renderer.BuildAllToMesh(m_Mesh, gameObject, Camera.main);
			}
		}
	}

	public string _LoadMapXml(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;
		TextAsset text = Resources.Load<TextAsset>(fileName);
		if (text == null)
			return string.Empty;

		return text.text;
	}

	public Material _LoadMaterial (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return null;
		int idx = fileName.LastIndexOf ('.');
		if (idx >= 0) {
			fileName = fileName.Substring (0, idx);
		}
		if (string.IsNullOrEmpty(fileName))
			return null;
		Material ret = Resources.Load<Material>(fileName);
		return ret;
	}

	public byte[] _LoadBinary(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return null;
		int idx = fileName.LastIndexOf ('.');
		if (idx >= 0) {
			fileName = fileName.Substring (0, idx);
		}
		if (string.IsNullOrEmpty(fileName))
			return null;
		TextAsset text = Resources.Load<TextAsset>(fileName);
		if (text == null)
			return null;
		return text.bytes;
	}

	public void _DestroyResource(UnityEngine.Object res)
	{}
}