using System;
using UnityEngine;
using Utils;
using TmxCSharp.Loader;
using TmxCSharp.Models;

namespace TmxCSharp.Renderer
{

	// AOI算法
	// TMX可视范围的MESH管理
	public class TMXMeshManager: MonoBehaviour
	{
		// view可视范围 view已经是地图范围
		private void OpenMap (ref Vector4 view, TileMap map, Camera cam)
		{
			Clear ();

			if (map == null)
				return;

			m_Tile = map;
		
			m_MapPixelW = map.Size.Width * map.Size.TileWidth;
			m_MapPixelH = map.Size.Height * map.Size.TileHeight;

			var render = this.Renderer;
			render.SetTMXMeshManagerScale (this, cam);

			MoveMap (ref view);
		}

		public void JumpTo (ref Vector4 view, TileMap map, Camera cam)
		{
			if (m_Tile != map)
				OpenMap (ref view, map, cam);
			else {
				if (m_Tile == null)
					return;
				ClearNodes ();
				ClearLastCenter ();
				MoveMap (ref view);
			}
		}

		// 地图左上角为位置
		public void MoveMap (ref Vector4 view)
		{
			if (m_MapPixelH <= 0 || m_MapPixelW <= 0 || m_Tile == null)
				return;

			if (!m_InitCenter) {
				m_InitCenter = true;
				m_LastCenter = new Vector2 (view.x + (view.z - view.x) / 2f, view.y + (view.w - view.y) / 2f);
				SearcNodes (ref view);
			} else {
			
			}
		}

		private int GetTileRow (float y, bool isCeil)
		{
			y += m_MapPixelH / 2f;
			int perH = m_Tile.Size.TileHeight;
			int ret;
			if (isCeil)
				ret = Mathf.CeilToInt (y / perH);
			else
				ret = Mathf.FloorToInt (y / perH);
			if (ret < 0)
				ret = 0;
			else if (ret >= m_Tile.Size.TileHeight)
				ret = m_Tile.Size.TileHeight - 1;
			return ret;
		}

		private int GetTileCol (float x, bool isCeil)
		{
			x += m_MapPixelW / 2f;
			int perW = m_Tile.Size.TileHeight;
			int ret;
			if (isCeil)
				ret = Mathf.CeilToInt (x / perW);
			else
				ret = Mathf.FloorToInt (x / perW);
			if (ret < 0)
				ret = 0;
			else if (ret >= m_Tile.Size.Width)
				ret = m_Tile.Size.Width - 1;
			return ret;
		}

		private void SearcNodes (ref Vector4 vec)
		{
			ClearNodes ();
			if (m_Tile == null || !m_Tile.IsVaild)
				return;

			var mapLayers = m_Tile.Layers;
			if (mapLayers == null || mapLayers.Count <= 0)
				return;

			// 创建TMXNODE
			m_XStart = GetTileCol (vec.x, false);
			m_XEnd = GetTileCol (vec.z, true);
			m_YStart = GetTileRow (vec.y, false);
			m_YEnd = GetTileRow (vec.w, true);

			for (int i = 0; i < mapLayers.Count; ++i) {
				var layer = mapLayers [i];
				for (int r = m_YStart; r <= m_YEnd; ++r) {
					for (int c = m_XStart; c <= m_XEnd; ++c) {
						int idx = r * layer.Width + c;
						TileIdData data = layer.TileIds [idx];
						TMXRenderer render = this.Renderer;
						render.BuildTMXMeshNode (r, c, i, data);
					
					}
				}
			}

		}

		private void ClearNodes ()
		{
			if (IsVaildTile) {
				var mapLayers = m_Tile.Layers;
				if (mapLayers == null)
					return;
				for (int l = 0; l < mapLayers.Count; ++l) {
					var layer = mapLayers [l];
					if (layer.TileIds == null || layer.TileIds.Count <= 0)
						continue;

					bool isOut = false;
					for (int r = m_YStart; r <= m_YEnd; ++r) {
						for (int c = m_XStart; c <= m_XEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
							{
								isOut = true;
								break;
							}
							TileIdData data = layer.TileIds [idx];
							InPool (data);
						}
						if (isOut)
							break;
					}
				}
			}
		}

		private void ClearLastCenter ()
		{
			m_InitCenter = false;
			m_LastCenter = Vector2.zero;
		}

		public void Clear ()
		{
			ClearNodes ();

			m_MapPixelW = 0;
			m_MapPixelH = 0;
			m_Tile = null;

			ClearLastCenter ();
		}

		private TMXMeshNode _CreateMeshNode ()
		{
			GameObject obj = new GameObject ();
			obj.name = "MeshTitle";
			obj.SetActive (false);
			TMXMeshNode ret = obj.AddComponent<TMXMeshNode> ();
			ret.IsDestroy = false;
			var trans = obj.transform;
			trans.parent = this.transform;
			trans.localPosition = Vector3.zero;
			trans.localScale = Vector3.one;
			trans.localRotation = Quaternion.identity;

			return ret;
		}

		private static void _DestroyMeshNode (TMXMeshNode node)
		{
			node.Destroy ();
			node.gameObject.SetActive (false);
			node.transform.localPosition = Vector3.one;
		}

		private void InitPool ()
		{
			if (m_InitPool)
				return;
			m_InitPool = true;
			m_Pool.Init (0, _CreateMeshNode, _DestroyMeshNode);
		}

		public TMXMeshNode CreateMeshNode ()
		{
			InitPool ();
			TMXMeshNode ret = m_Pool.GetObject ();
			ret.IsDestroy = false;
			return ret;
		}

		private void InPool (TileIdData node)
		{
			if (node == null)
				return;

			TMXMeshNode data = node.userData as TMXMeshNode;
			if (data == null)
				return;

			InitPool ();
	
			node.userData = null;

			m_Pool.Store (data);
		}



		protected GameObject GameObj {
			get {
				if (m_GameObj == null)
					m_GameObj = this.gameObject;
				return m_GameObj;
			}
		}

		protected TMXRenderer Renderer {
			get {
				if (m_Renderer == null) {
					m_Renderer = GetComponentInParent<TMXRenderer> ();
				}
				return m_Renderer;
			}
		}

		private bool IsVaildTile {
			get {
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
		private ObjectPool<TMXMeshNode> m_Pool = new ObjectPool<TMXMeshNode> ();
		private bool m_InitPool = false;

		private float m_MapPixelW = 0;
		private float m_MapPixelH = 0;

		private Vector2 m_LastCenter = Vector2.zero;
		private bool m_InitCenter = false;

		private GameObject m_GameObj = null;
		private TMXRenderer m_Renderer = null;
	}

}
