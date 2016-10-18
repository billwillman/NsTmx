using System;
using UnityEngine;
using Utils;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using TmxCSharp.Renderer;

// AOI算法
// TMX可视范围的MESH管理
public class TMXMeshManager: MonoBehaviour
{
	// view可视范围 view已经是地图范围
	public void OpenMap(ref Vector4 view, TileMap map)
	{
		Clear ();

		if (map == null)
			return;
		
		m_MapPixelW = map.Size.Width * map.Size.TileWidth;
		m_MapPixelH = map.Size.Height * map.Size.TileHeight;

		MoveMap (ref view, map);
	}

	// 地图左上角为位置
	public void MoveMap(ref Vector4 view, TileMap map)
	{
		if (m_MapPixelH <= 0 || m_MapPixelW <= 0 || map == null)
			return;

		if (!m_InitCenter)
		{
			m_InitCenter = true;
			m_LastCenter = new Vector2(view.x + (view.z - view.x)/2f, view.y + (view.w - view.y)/2f);
			SearcNodes(ref view);
		}
	}

	private void SearcNodes(ref Vector4 vec)
	{
		ClearNodes();
		SearchXNode(vec.x, vec.z);
		SearchYNode(vec.y, vec.w);
	}


	private void SearchXNode(float xMin, float xMax)
	{
		
	}

	private void SearchYNode(float yMin, float yMax)
	{
		
	}

	public TMXMeshNode XFirstNode
	{
		get {
			if (m_XFirstNode == null)
				return null;
			return (m_XFirstNode.userData as TMXMeshNode);
		}
	}

	public TMXMeshNode YFirstNode
	{
		get {
			if (m_YFirstNode == null)
				return null;
			return m_YFirstNode.userData as TMXMeshNode;
		}
	}

	private void ClearNodes()
	{
		TileIdData node = m_XFirstNode;

		while (node != null) {
			TileIdData next = node.XNodeNext;
			InPool(node);
			node = next;
		}

		m_XFirstNode = null;
		m_YFirstNode = null;
		m_XEndNode = null;
		m_YEndNode = null;
	}

	private void Clear()
	{
		ClearNodes();

		m_MapPixelW = 0;
		m_MapPixelH = 0;

		m_InitCenter = false;
		m_LastCenter = Vector2.zero;
	}

	private TMXMeshNode _CreateMeshNode()
	{
		GameObject obj = new GameObject();
		obj.SetActive(false);
		TMXMeshNode ret = obj.AddComponent<TMXMeshNode>();
		var trans = obj.transform;
		trans.parent = this.transform;
		trans.localPosition = Vector3.zero;
		trans.localScale = Vector3.one;
		trans.localRotation = Quaternion.identity;

		return ret;
	}

	private static void _DestroyMeshNode(TMXMeshNode node)
	{
		node.Destroy();
		node.gameObject.SetActive(false);
	}

	private void InitPool()
	{
		if (m_InitPool)
			return;
		m_InitPool = true;
		m_Pool.Init(0, _CreateMeshNode, _DestroyMeshNode);
	}

	private TMXMeshNode CreateMeshNode()
	{
		InitPool();
		return m_Pool.GetObject();
	}

	private void InPool(TileIdData node)
	{
		if (node == null)
			return;

		TMXMeshNode data = node.userData as TMXMeshNode;
		if (data == null)
			return;

		InitPool();
	
		node.userData = null;

		m_Pool.Store(data);
	}



	protected GameObject GameObj
	{
		get
		{
			if (m_GameObj == null)
				m_GameObj = this.gameObject;
			return m_GameObj;
		}
	}

	private TileIdData m_XFirstNode = null;
	private TileIdData m_YFirstNode = null;
	private TileIdData m_XEndNode = null;
	private TileIdData m_YEndNode = null;

	// 池
	private ObjectPool<TMXMeshNode> m_Pool = new ObjectPool<TMXMeshNode>();
	private bool m_InitPool = false;

	private int m_MapPixelW = 0;
	private int m_MapPixelH = 0;

	private Vector2 m_LastCenter = Vector2.zero;
	private bool m_InitCenter = false;

	private GameObject m_GameObj = null;
}
