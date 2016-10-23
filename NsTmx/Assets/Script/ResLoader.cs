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

	public bool IsUseAllMesh = true;

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
				Scene = 0;
				float t1 = Time.realtimeSinceStartup;
				Debug.LogFormat("加载TMX地图时间：{0}", (t1 - t).ToString());
				/*
				if (IsUseAllMesh)
					m_Renderer.BuildAllToMesh(m_Mesh, gameObject, Camera.main);
				else
					m_Renderer.MeshJumpTo(Camera.main);

				float t2 = Time.realtimeSinceStartup;
				Debug.LogFormat("生成地圖時間：{0}", (t2 - t1).ToString());
				*/
			}
		}
	}

	private int Scene = 0;

	void OnGUI()
	{
		const float delta = 0.5f;
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			Camera cam = Camera.main;
			var trans = cam.transform;
			Vector3 vec = trans.localPosition;
			vec.y += delta;
			trans.localPosition = vec;
			if (!IsUseAllMesh)
				m_Renderer.MeshMove (cam);
		} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
			Camera cam = Camera.main;
			var trans = cam.transform;
			Vector3 vec = trans.localPosition;
			vec.y -= delta;
			trans.localPosition = vec;
			if (!IsUseAllMesh)
				m_Renderer.MeshMove (cam);
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			Camera cam = Camera.main;
			var trans = cam.transform;
			Vector3 vec = trans.localPosition;
			vec.x -= delta;
			trans.localPosition = vec;
			if (!IsUseAllMesh)
				m_Renderer.MeshMove (cam);
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			Camera cam = Camera.main;
			var trans = cam.transform;
			Vector3 vec = trans.localPosition;
			vec.x += delta;
			trans.localPosition = vec;
			if (!IsUseAllMesh)
				m_Renderer.MeshMove (cam);
		}

		if (GUILayout.Button("刷新可視範圍"))
		{
			if (m_Renderer != null)
			{
				m_Renderer.MeshJumpTo(Camera.main);
			}
		}

		if (GUILayout.Button("切換地圖"))
		{
			string fileName;
			if (Scene == 0)
			{
				fileName = "tmx/TiledSupport-1.bytes";
				Scene = 1;
			} else
			{
				fileName = "tmx/d107.bytes";
				Scene = 0;
			}

			float t = Time.realtimeSinceStartup;
			if (m_Renderer.LoadMapFromBinaryFile(fileName, this))
			{
				float t1 = Time.realtimeSinceStartup;
				Debug.LogFormat("加载TMX地图时间：{0}", (t1 - t).ToString());


				if (IsUseAllMesh)
					m_Renderer.BuildAllToMesh(m_Mesh, gameObject, Camera.main);
				else
					m_Renderer.MeshJumpTo(Camera.main);

				float t2 = Time.realtimeSinceStartup;
				Debug.LogFormat("生成地圖時間：{0}", (t2 - t1).ToString());

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