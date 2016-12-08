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

	public bool IsUseAllMesh = true;

	void Awake()
	{
		m_Renderer = GetComponent<TMXRenderer>();
	}

	private int Scene = 0;

	private static readonly float delta = 0.5f;
	private void GoUp()
	{
		
		Camera cam = Camera.main;
		var trans = cam.transform;
		Vector3 vec = trans.localPosition;
		vec.y += delta;
		trans.localPosition = vec;
		if (!IsUseAllMesh)
			m_Renderer.MeshMove (cam);
	}

	private void GoDown()
	{
		Camera cam = Camera.main;
		var trans = cam.transform;
		Vector3 vec = trans.localPosition;
		vec.y -= delta;
		trans.localPosition = vec;
		if (!IsUseAllMesh)
			m_Renderer.MeshMove (cam);
	}

	private void GoLeft()
	{
		Camera cam = Camera.main;
		var trans = cam.transform;
		Vector3 vec = trans.localPosition;
		vec.x -= delta;
		trans.localPosition = vec;
		if (!IsUseAllMesh)
			m_Renderer.MeshMove (cam);
	}

	private void GoRight()
	{
		Camera cam = Camera.main;
		var trans = cam.transform;
		Vector3 vec = trans.localPosition;
		vec.x += delta;
		trans.localPosition = vec;
		if (!IsUseAllMesh)
			m_Renderer.MeshMove (cam);
	}

	private void ChangeScene()
	{
		string fileName;
		if (Scene == 0)
		{
			fileName = "tmx/jiazu01.bytes";
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
			{
				m_Renderer.BuildMeshPerLayer(gameObject, Camera.main);
				//m_Renderer.BuildAllToMesh(gameObject, Camera.main);
			}
			else
				m_Renderer.MeshJumpTo(Camera.main);

			float t2 = Time.realtimeSinceStartup;
			Debug.LogFormat("生成地圖時間：{0}", (t2 - t1).ToString());

		}
	}

	void Update()
	{
		TimerMgr.Instance.ScaleTick(Time.deltaTime);
		TimerMgr.Instance.UnScaleTick(Time.unscaledDeltaTime);
	}

	void OnGUI()
	{
		
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			GoUp();
		} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
			GoDown();
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			GoLeft();
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			GoRight();
		}

		if (GUI.Button(new Rect(0, 0, 100, 100), "分块地图"))
		{
			IsUseAllMesh = false;
			ChangeScene();
		}

		if (GUI.Button(new Rect(150, 0, 100, 100), "整体地图"))
		{
			IsUseAllMesh = true;
			ChangeScene();
		}

		if (GUI.Button(new Rect(0, 150, 100, 100), "向上"))
		{
			GoUp();
		}

		if (GUI.Button(new Rect(0, 250, 100, 100), "向下"))
		{
			GoDown();
		}

		if (GUI.Button(new Rect(0, 350, 100, 100), "向左"))
		{
			GoLeft();
		}

		if (GUI.Button(new Rect(0, 450, 100, 100), "向右"))
		{
			GoRight();
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