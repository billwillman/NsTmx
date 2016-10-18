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

		m_Tile = map;
		
		m_MapPixelW = map.Size.Width * map.Size.TileWidth;
		m_MapPixelH = map.Size.Height * map.Size.TileHeight;

		MoveMap (ref view);
	}

	public void JumpTo(ref Vector4 view)
	{
		ClearNodes();
		ClearLastCenter();
		MoveMap(ref view);
	}

	// 地图左上角为位置
	public void MoveMap(ref Vector4 view)
	{
		if (m_MapPixelH <= 0 || m_MapPixelW <= 0 || m_Tile == null)
			return;

		if (!m_InitCenter)
		{
			m_InitCenter = true;
			m_LastCenter = new Vector2(view.x + (view.z - view.x)/2f, view.y + (view.w - view.y)/2f);
			SearcNodes(ref view);
		} else
		{
			
		}
	}

	private void SearcNodes(ref Vector4 vec)
	{
		ClearNodes();
		// 创建TMXNODE
	}

	private void ClearNodes()
	{
		if (IsVaildTile)
		{
			var mapLayers = m_Tile.Layers;
			if (mapLayers == null)
				return;
			for (int l = 0; l < mapLayers.Count; ++l)
			{
				var layer = mapLayers[l];
				for (int r = m_YStart; r <= m_YEnd; ++r)
				{
					for (int c = m_XStart; c <= m_XEnd; ++c)
					{
						int idx = r * layer.Width + c;
						TileIdData data = layer.TileIds[idx];
						InPool(data);
					}
				}
			}
		}
	}

	private void ClearLastCenter()
	{
		m_InitCenter = false;
		m_LastCenter = Vector2.zero;
	}

	private void Clear()
	{
		ClearNodes();

		m_MapPixelW = 0;
		m_MapPixelH = 0;
		m_Tile = null;

		ClearLastCenter();
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
		node.transform.localPosition = Vector3.one;
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

	private bool IsVaildTile
	{
		get
		{
			return (m_XStart >= 0) && (m_XEnd >= 0) && (m_XStart < m_XEnd) && 
				(m_YStart >= 0) && (m_YEnd >= 0) && (m_YStart < m_YEnd) && m_Tile != null && m_Tile.IsVaild;
		}
	}

	private int m_XStart = -1;
	private int m_XEnd = -1;
	private int m_YStart = -1;
	private int m_YEnd = -1;
	private TileMap m_Tile = null;

	// 池
	private ObjectPool<TMXMeshNode> m_Pool = new ObjectPool<TMXMeshNode>();
	private bool m_InitPool = false;

	private float m_MapPixelW = 0;
	private float m_MapPixelH = 0;

	private Vector2 m_LastCenter = Vector2.zero;
	private bool m_InitCenter = false;

	private GameObject m_GameObj = null;
}
