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
	public int TileId = -1;

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
			fileName = "tmx/471_randommap_cavestyle3.bytes";
			Scene = 0;
		}

		float t = Time.realtimeSinceStartup;
		if (m_Renderer.LoadMapFromBinaryFile(fileName, this))
		{
			float t1 = Time.realtimeSinceStartup;
			Debug.LogFormat("加载TMX地图时间：{0}", (t1 - t).ToString());


            if (IsUseAllMesh) {
                // 此段代码讲解：UseDesign支持按分辨率自适应，如果不需要或者
                // 外部代码已经做了自适应，例如相机处理，则设置为false即可
                // m_Scale是像素和米转换，UNITY坐标是米为单位
                m_Renderer.m_UseDesign = false;
                m_Renderer.m_Scale = 0.01f;

                m_Renderer.BuildMeshPerLayer(Camera.main);
             //   m_Renderer.fixedTmxMap();
                //m_Renderer.BuildAllToMesh(gameObject, Camera.main);
            } else {
                m_Renderer.m_Scale = 100f;
                m_Renderer.m_UseDesign = true;
                m_Renderer.MeshJumpTo(Camera.main);
            }

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
		bool isEnableClick = true;
		TileId = -1;
		
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			isEnableClick = false;
			GoUp();
		} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
			isEnableClick = false;
			GoDown();
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			isEnableClick = false;
			GoLeft();
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			isEnableClick = false;
			GoRight();
		}

		if (GUI.Button(new Rect(0, 0, 100, 100), "分块地图"))
		{
			isEnableClick = false;
			IsUseAllMesh = false;
			ChangeScene();
		}

		if (GUI.Button(new Rect(150, 0, 100, 100), "整体地图"))
		{
			isEnableClick = false;
			IsUseAllMesh = true;
			ChangeScene();
		}

		if (GUI.Button(new Rect(0, 150, 100, 100), "向上"))
		{
			isEnableClick = false;
			GoUp();
		}

		if (GUI.Button(new Rect(0, 250, 100, 100), "向下"))
		{
			isEnableClick = false;
			GoDown();
		}

		if (GUI.Button(new Rect(0, 350, 100, 100), "向左"))
		{
			isEnableClick = false;
			GoLeft();
		}

		if (GUI.Button(new Rect(0, 450, 100, 100), "向右"))
		{
			isEnableClick = false;
			GoRight();
		}

		if (isEnableClick && Input.GetMouseButtonDown(0)) {
			Vector3 mousePt = Input.mousePosition;
			Camera mainCam = Camera.main;
			if (mainCam != null) {
				Ray ray = mainCam.ScreenPointToRay (mousePt);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit) && hit.collider != null) {
					TMXRenderer renderer = hit.collider.GetComponent<TMXRenderer> ();
					if (renderer != null) {
						mousePt = hit.point;
						int tileId = renderer.GetTileIdByWorldPos (mousePt);
					}
				}
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