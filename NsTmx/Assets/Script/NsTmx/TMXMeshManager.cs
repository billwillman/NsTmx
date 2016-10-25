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
		/*
		void Start()
		{
			InitPool();
		}*/

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

			ClipTileMapView (ref view);

			if (!m_InitLastView) {
				SearcNodes (ref view);
				m_InitLastView = true;
				m_LastView = view;
			} else {
				// 开始移动囖
				MoveNodes (ref view);
			}
		}


		private void ClipTileMapView (ref Vector4 view)
		{
			if (m_Tile == null || !m_Tile.IsVaild)
				return;

			float halfW = ((float)(m_Tile.Size.TileWidth * m_Tile.Size.Width)) / 2f;
			float halfH = ((float)(m_Tile.Size.TileHeight * m_Tile.Size.Height)) / 2f;

			// Y判断
			if (view.y > halfH) {
				float delta = view.y - halfH;
				view.y = halfH;
				view.w -= delta;
			} else if (view.w < -halfH) {
				float delta = -halfH - view.w;
				view.w = -halfH;
				view.y += delta;
			}

			if (view.x < -halfW) {
				float delta = -halfW - view.x;
				view.x = -halfW;
				view.z += delta;
			} else if (view.z > halfW) {
				float delta = view.z - halfW;
				view.z = halfW;
				view.x -= delta;
			}
		}

		private int GetTileRow (float y, bool isCeil)
		{
			if (m_Tile == null || !m_Tile.IsVaild)
				return 0;
			
			float f = ((float)m_Tile.Size.Height) / 2f - y / ((float)m_Tile.Size.TileHeight);

			int ret;
			if (isCeil)
				ret = Mathf.CeilToInt (f) + 2;
			else
				ret = Mathf.FloorToInt (f);

			if (ret < 0)
				ret = 0;
			else if (ret >= m_Tile.Size.Height) {
				ret = m_Tile.Size.TileHeight - 1;
				for (int l = 0; l < m_Tile.Layers.Count; ++l) {
					var lay = m_Tile.Layers [l];
					if (lay == null)
						continue;
					if (lay.Height > ret)
						ret = lay.Height;
				}

			}
			return ret;
		}

		private int GetTileCol (float x, bool isCeil)
		{
			if (m_Tile == null || !m_Tile.IsVaild)
				return 0;
			
			float f = ((float)m_Tile.Size.Width / 2f) + x / ((float)m_Tile.Size.TileWidth);

			int ret;
			if (isCeil)
				ret = Mathf.CeilToInt (f) + 2;
			else
				ret = Mathf.FloorToInt (f);

			if (ret < 0)
				ret = 0;
			else if (ret >= m_Tile.Size.Width) {
				ret = m_Tile.Size.Width - 1;
				for (int l = 0; l < m_Tile.Layers.Count; ++l) {
					var lay = m_Tile.Layers [l];
					if (lay == null)
						continue;
					if (lay.Width > ret)
						ret = lay.Width;
				}
			}
			return ret;
		}

		private void BuildTMXMeshNode (int r, int c, int layer, TileIdData data)
		{
			TMXRenderer render = this.Renderer;
			TMXMeshNodeLoaderList.Instance.Renderer = render;
			TMXMeshNodeLoaderList.Instance.AddLoader(r, c, layer, data);
			//render.BuildTMXMeshNode (r, c, layer, data);
		}

		private void MoveNodes (ref Vector4 vec)
		{
			if (m_Tile == null || !m_Tile.IsVaild)
				return;
			
			var mapLayers = m_Tile.Layers;
			if (mapLayers == null || mapLayers.Count <= 0)
				return;

			if (vec == m_LastView)
				return;
			
			int xStart = GetTileCol (vec.x, false);
			int xEnd = GetTileCol (vec.z, true);
			int yStart = GetTileRow (vec.y, false);
			int yEnd = GetTileRow (vec.w, true);

			if (m_XStart == xStart && m_XEnd == xEnd && m_YStart == yStart && m_YEnd == yEnd)
				return;
			
			bool isYChanged = false;
			bool isXChanged = false;

			for (int i = 0; i < mapLayers.Count; ++i) {
				var layer = mapLayers [i];
				if (m_YEnd > yEnd) {
					isYChanged = true;
					// Up


					// 先InPool
					for (int r = yEnd + 1; r <= m_YEnd; ++r) {
						for (int c = m_XStart; c <= m_XEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;

							TMXMeshNodeLoaderList.Instance.RemoveLoader (r, c, i);

							TileIdData data = layer.TileIds [idx];
							InPool (data);
						}
					}

					// 再outPool
					for (int r = yStart; r <= m_YStart; ++r) {
						if (r < 0)
							continue;
						
						for (int c = m_XStart; c <= m_XEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;

							TileIdData data = layer.TileIds [idx];
							BuildTMXMeshNode (r, c, i, data);
						}
					}

				} else if (m_YStart < yStart) {
			 
					isYChanged = true;
					// Down

					// 先InPool
					for (int r = m_YStart; r < yStart; ++r) {
						for (int c = m_XStart; c <= m_XEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;
							
							TMXMeshNodeLoaderList.Instance.RemoveLoader (r, c, i);
							TileIdData data = layer.TileIds [idx];
							InPool (data);
						}
					}

					// 再outPool
					for (int r = m_YEnd; r <= yEnd; ++r) {
						if (r < 0)
							continue;

						for (int c = m_XStart; c <= m_XEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;

							TileIdData data = layer.TileIds [idx];
							BuildTMXMeshNode (r, c, i, data);
						}
					}
				}

				if (xStart < m_XStart) {
					// Left
					isXChanged = true;
					for (int r = yStart; r <= yEnd; ++r) {
						for (int c = xEnd + 1; c <= m_XEnd; ++c) {
							// InPool

							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;
							TMXMeshNodeLoaderList.Instance.RemoveLoader (r, c, i);
							TileIdData data = layer.TileIds [idx];
							InPool (data);
						}

						for (int c = xStart; c < m_XStart; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;
							TileIdData data = layer.TileIds [idx];
							BuildTMXMeshNode (r, c, i, data);
						}
					}
				} else if (xEnd > m_XEnd) {
					// right
					isXChanged = true;
					for (int r = yStart; r <= yEnd; ++r) {
						// InPool
						for (int c = m_XStart; c < xStart; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;
							TMXMeshNodeLoaderList.Instance.RemoveLoader (r, c, i);
							TileIdData data = layer.TileIds [idx];
							InPool (data);
						}

						// outPool
						for (int c = m_XEnd + 1; c <= xEnd; ++c) {
							int idx = r * layer.Width + c;
							if (idx >= layer.TileIds.Count)
								break;
							TileIdData data = layer.TileIds [idx];
							BuildTMXMeshNode (r, c, i, data);
						}
					}
				}
			}
			


			/*Debug.LogFormat("MoveStartX: {0} MoveEndX: {1} MoveStartY: {2} MoveEndY: {3}",
				(xStart - m_XStart).ToString(), (xEnd - m_XEnd).ToString(),
				(yStart - m_YStart).ToString(), (yEnd - m_YEnd).ToString());

			Debug.LogFormat("LEFT: {0} RIGHT: {1} TOP: {2} BOTTOM: {3}", 
				xStart.ToString(), xEnd.ToString(), 
				yStart.ToString().ToString(), yEnd.ToString());*/

			if (isXChanged) {
				m_XStart = xStart;
				m_XEnd = xEnd;
			}

			if (isYChanged) {
				m_YStart = yStart;
				m_YEnd = yEnd;
			}

			if (isXChanged || isYChanged) {
				m_LastView = vec;
			}
		}

		private void SearcNodes (ref Vector4 vec)
		{
			ClearNodes ();
			if (m_Tile == null || !m_Tile.IsVaild)
				return;

			var mapLayers = m_Tile.Layers;
			if (mapLayers == null || mapLayers.Count <= 0)
				return;

			m_XStart = GetTileCol (vec.x, false);
			m_XEnd = GetTileCol (vec.z, true);
			m_YStart = GetTileRow (vec.y, false);
			m_YEnd = GetTileRow (vec.w, true);

			if (m_YStart >= m_YEnd || m_XStart >= m_XEnd)
				return;
			
			for (int i = 0; i < mapLayers.Count; ++i) {
				var layer = mapLayers [i];
				bool isBreak = false;
				for (int r = m_YStart; r <= m_YEnd; ++r) {
					for (int c = m_XStart; c <= m_XEnd; ++c) {
						int idx = r * layer.Width + c;
						if (idx >= layer.TileIds.Count) {
							isBreak = true;
							break;
						}
						
						TileIdData data = layer.TileIds [idx];
						BuildTMXMeshNode (r, c, i, data);
					
					}

					if (isBreak)
						break;
				}
			}

		}

		private void ClearNodes ()
		{
			TMXMeshNodeLoaderList.Instance.Clear ();
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
							if (idx >= layer.TileIds.Count) {
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
			m_InitLastView = false;
			m_LastView = Vector4.zero;
			m_XStart = -1;
			m_XEnd = -1;
			m_YStart = -1;
			m_YEnd = -1;
		}

		public void Clear ()
		{
			ClearNodes ();

			m_MapPixelW = 0;
			m_MapPixelH = 0;
			m_Tile = null;

			ClearLastCenter ();
		}

		private GameObject m_CloneNode = null;

		private TMXMeshNode _CreateMeshNode ()
		{
			if (m_CloneNode == null) {
				// 提升性能
				GameObject obj = new GameObject ();
				obj.name = "MeshTitle";
				obj.SetActive (false);
				TMXMeshNode script = obj.AddComponent<TMXMeshNode> ();
				//script.InitBuf (4);
				//script.IsDestroy = false;
				var trans = obj.transform;
				trans.parent = this.transform;
				trans.localPosition = Vector3.zero;
				trans.localScale = Vector3.one;
				trans.localRotation = Quaternion.identity;

				m_CloneNode = obj;
			}

			GameObject obj1 = GameObject.Instantiate(m_CloneNode);
			var trans1 = obj1.transform;
			trans1.parent = this.transform;
			trans1.localPosition = Vector3.zero;
			trans1.localScale = Vector3.one;
			trans1.localRotation = Quaternion.identity;

			TMXMeshNode ret = obj1.GetComponent<TMXMeshNode>();
			ret.InitBuf(4);
			ret.IsDestroy = false;

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
			m_Pool.Init (35 * 35, _CreateMeshNode, _DestroyMeshNode);
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

		public int PoolCount {
			get {
				return m_Pool.Count;
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

		private Vector4 m_LastView = Vector4.zero;
		private bool m_InitLastView = false;

		private GameObject m_GameObj = null;
		private TMXRenderer m_Renderer = null;
	}

}
