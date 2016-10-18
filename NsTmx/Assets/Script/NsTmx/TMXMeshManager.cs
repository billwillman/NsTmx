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
	}

	private TMXMeshNode SearchXNode(float xMin, float xMax)
	{
		
	}

	public TMXMeshNode XFirstNode
	{
		get {
			return m_XFirstNode;
		}
	}

	public TMXMeshNode YFirstNode
	{
		get {
			return m_YFirstNode;
		}
	}

	private void Clear()
	{
		TMXMeshNode node = m_XFirstNode;

		while (node != null) {
			node.Destroy ();
			node = node.XNodeNext;
		}

		m_XFirstNode = null;
		m_YFirstNode = null;

		m_MapPixelW = 0;
		m_MapPixelH = 0;
	}

	// 池
	private ObjectPool<TMXMeshNode> m_Pool = null;
	private TMXMeshNode m_XFirstNode = null;
	private TMXMeshNode m_YFirstNode = null;
	private TMXMeshNode m_XVisableLeft = null;

	private int m_MapPixelW = 0;
	private int m_MapPixelH = 0;
}
