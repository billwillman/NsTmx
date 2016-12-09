#define _USE_ADDVERTEX2
#define _USE_SPLIT_PERLAYER

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using XmlParser;
using Utils;

namespace TmxCSharp.Renderer
{

	// TMX地图渲染
	public class TMXRenderer : MonoBehaviour, ITmxTileDataParent
	{
		void Start()
		{
			InitMeshMgr();
		}

		public bool LoadMapFromBinaryFile (string fileName, ITmxLoader loader)
		{
			Clear ();
			if (string.IsNullOrEmpty (fileName) || loader == null)
				return false;

			TileMap tileMap = TmxLoader.ParseBinary (fileName, loader);

			bool ret = tileMap != null && tileMap.IsVaild;
			if (ret) {
				m_TileMap = tileMap;
				m_ResRootPath = Path.GetDirectoryName (fileName);
				LoadRes (tileMap);
			}

			return ret;
		}

		public bool LoadMapFromXMLFile (string fileName, ITmxLoader loader)
		{
			Clear ();
			if (string.IsNullOrEmpty (fileName) || loader == null)
				return false;

			TileMap tileMap = TmxLoader.Parse (fileName, loader);

			bool ret = tileMap != null && tileMap.IsVaild;
			if (ret) {
				m_TileMap = tileMap;
				m_ResRootPath = Path.GetDirectoryName (fileName);
				LoadRes (tileMap);
			}

			return ret;
		}

		public Func<TileSet, bool> OnIsTileSetVisible {
			get;
			set;
		}

		private bool DoIsTileSetVisible (TileSet tile)
		{
			if (tile == null)
				return false;
			if (OnIsTileSetVisible != null)
				return OnIsTileSetVisible (tile);
			return true;
		}

		public void Close()
		{
			Clear();
		}

		private void Clear ()
		{
			ClearTileData ();
            ClearAllToMesh(gameObject);
            ClearMeshPerLayer(gameObject);
			m_ResRootPath = string.Empty;
		}

		private void ClearTileData ()
		{
			if (m_MeshMgr != null) {
				m_MeshMgr.Clear ();
			}

			var iter = m_TileDataMap.GetEnumerator ();
			while (iter.MoveNext ()) {
				if (iter.Current.Value != null)
					iter.Current.Value.Destroy ();
			}
			iter.Dispose ();
			m_TileDataMap.Clear ();

			m_TileMap = null;
		}

		private void LoadRes (TileMap tileMap)
		{
			if (tileMap == null)
				return;

			if (m_DesignHeight > 0 && tileMap.IsVaild) {
				m_DesignWidth = (((float)tileMap.Size.Width) * ((float)tileMap.Size.TileWidth)) / (((float)tileMap.Size.Height) * ((float)tileMap.Size.TileHeight)) * ((float)m_DesignHeight);
			}

			var tileSets = tileMap.TileSets;
			if (tileSets != null) {
				for (int i = 0; i < tileSets.Count; ++i) {
					TileSet tileSet = tileSets [i];
					if (tileSet == null || !tileSet.IsVaid || !DoIsTileSetVisible (tileSet))
						continue;

					TmxTileData tileData = new TmxTileData (tileSet, this);
					if (m_TileDataMap.ContainsKey (tileSet.FirstId))
						m_TileDataMap [tileSet.FirstId] = tileData;
					else {
						m_TileDataMap.Add (tileSet.FirstId, tileData);
					}
				}
			}
		}

		private void AddVertex2 (

			int col, int row, 
			int layerIdx, int layerWidth, int layerHeight,
			int baseTileWidth, int baseTileHeight, 
			TileIdData tileData, TmxTileData tmxTile
        )
		{
          
            TMXMeshNode node = tileData.userData as TMXMeshNode;
			GameObject gameObj = node.gameObject;
			if (!gameObj.activeSelf) {
				gameObj.SetActive (true);
			}

			MeshRenderer render = node.Renderer;
			render.sharedMaterial = tmxTile.Mat;
			var tile = tmxTile.Tile;

			Vector3[] vertList = node.VertBuf;
			Vector2[] uvList = node.UVBuf;
			int[] indexList = node.IndexBuf;

			Vector2 _meshsize_ = new Vector2 (1.0f / ((float)layerWidth), 1.0f / ((float)layerHeight));

            Vector2 _pivotPoint = new Vector2((col - 1) * _meshsize_.x * -1 - _meshsize_.x / 2f, (row - 1) * _meshsize_.y + _meshsize_.y / 2f);
            float z = -layerIdx * 0.01f;

            _pivotPoint.x += .5f;
			_pivotPoint.y -= .5f;
			float dx = ((float)tile.TileWidth / (float)baseTileWidth) - 1;
			float dy = ((float)tile.TileHeight / (float)baseTileHeight) - 1;

			int tileId = tileData.tileId;
			tileId = tileId - tile.FirstId;
			//int deltaY = tile.TileHeight/baseTileHeight;
			int tileColCnt = Mathf.CeilToInt (tile.Image.Width / tile.TileWidth);
			int r = tileId / tileColCnt;
			int c = tileId % tileColCnt;
			float uvPerY = ((float)tile.TileHeight) / ((float)tile.Image.Height);
			float uvPerX = ((float)tile.TileWidth) / ((float)tile.Image.Width);
			float uvY = 1f - (float)(r) * uvPerY;
			float uvX = (float)(c) * uvPerX;

			float x0 = ((_meshsize_.x / 2) * -1) - _pivotPoint.x;
			float y0 = (_meshsize_.y / 2) - _pivotPoint.y + (dy * _meshsize_.y);
			float x1 = (_meshsize_.x / 2) - _pivotPoint.x + (dx * _meshsize_.x);
			float y1 = ((_meshsize_.y / 2) * -1) - _pivotPoint.y;

			float uvX0;
			float uvX1;
			float uvY0;
			float uvY1;

			if (tileData.isFlipX) {
				uvX0 = uvX + uvPerX;
				uvX1 = uvX;
			} else {
				uvX0 = uvX;
				uvX1 = uvX + uvPerX;
			}

			if (tileData.isFlipY) {
				uvY0 = uvY - uvPerY;
				uvY1 = uvY;
			} else {
				uvY0 = uvY;
				uvY1 = uvY - uvPerY;
			}

			int vertIdx = 0;
			int indexIdx = 0;

			// 处理面片
			float halfW = (x1 - x0) / 2f; 
			float halfH = (y0 - y1) / 2f;
			Vector3 pos = new Vector3 (x0 + halfW, y0 - halfH, z);
			x0 = -halfW;
			y0 = halfH;
			x1 = halfW;
			y1 = -halfH;
			node.transform.localPosition = pos;

			Vector3 vec = new Vector3 (x0, y0, 0);
			vertList [vertIdx] = vec;
			Vector2 uv = new Vector2 (uvX0, uvY0);
			uvList [vertIdx] = uv;
			indexList [indexIdx] = vertIdx;
			++vertIdx;
			++indexIdx;

			vec = new Vector3 (x1, y0, 0);
			vertList [vertIdx] = vec;
			uv = new Vector2 (uvX1, uvY0);
			uvList [vertIdx] = uv;
			indexList [indexIdx] = vertIdx;
			++vertIdx;
			++indexIdx;

			vec = new Vector3 (x1, y1, 0);
			vertList [vertIdx] = vec;
			uv = new Vector2 (uvX1, uvY1);
			uvList [vertIdx] = uv;
			indexList [indexIdx] = vertIdx;
			++vertIdx;
			++indexIdx;

			vec = new Vector3 (x0, y1, 0);
			vertList [vertIdx] = vec;
			uv = new Vector2 (uvX0, uvY1);
			uvList [vertIdx] = uv;
			indexList [indexIdx] = vertIdx;

			node.UpdateMesh();
		}

		private void AddVertex2 (

			int col, int row, 
			int layerIdx, int layerWidth, int layerHeight, 
			int baseTileWidth, int baseTileHeight, 
			TileIdData tileData, TileSet tile,
			List<Vector3> vertList, List<Vector2> uvList, List<int> indexList
		// ,Dictionary<KeyValuePair<int, int>, int> XYToVertIdx
           , TileMap.TileMapType tileType = TileMap.TileMapType.ttOrient
		)
		{
            Vector2 _meshsize_ = new Vector2(1.0f / ((float)layerWidth), 1.0f / ((float)layerHeight));
            Vector2 _pivotPoint;

            float z = -layerIdx * 0.01f;
			if (tileType == TileMap.TileMapType.ttIsometric)
            {
               // col = col * 2 + row % 2;
                float pX = 1f / 2f + (row - col) * _meshsize_.x / 2f - 1f;
                float pY = (col + row) * _meshsize_.y / 2f;
                _pivotPoint = new Vector2(pX, pY);
                z -= 0.001f * (col + row);
            }
            else
            {
                _pivotPoint = new Vector2((col - 1) * _meshsize_.x * -1 - _meshsize_.x / 2f, (row - 1) * _meshsize_.y + _meshsize_.y / 2f);
                
            }
            _pivotPoint.x += .5f;
            _pivotPoint.y -= .5f;

            float dx = ((float)tile.TileWidth / (float)baseTileWidth) - 1;
			float dy = ((float)tile.TileHeight / (float)baseTileHeight) - 1;

            float x0 = ((_meshsize_.x / 2) * -1) - _pivotPoint.x;
            float y0 = (_meshsize_.y / 2) - _pivotPoint.y + (dy * _meshsize_.y);
            float x1 = (_meshsize_.x / 2) - _pivotPoint.x + (dx * _meshsize_.x);
            float y1 = ((_meshsize_.y / 2) * -1) - _pivotPoint.y;

            


            int tileId = tileData.tileId;
			tileId = tileId - tile.FirstId;
			//int deltaY = tile.TileHeight/baseTileHeight;
			int tileColCnt = Mathf.CeilToInt (tile.Image.Width / tile.TileWidth);
			int r = tileId / tileColCnt;
			int c = tileId % tileColCnt;
			float uvPerY = ((float)tile.TileHeight) / ((float)tile.Image.Height);
			float uvPerX = ((float)tile.TileWidth) / ((float)tile.Image.Width);
			float uvY = 1f - (float)(r) * uvPerY;
			float uvX = (float)(c) * uvPerX;

			
			float uvX0;
			float uvX1;
			float uvY0;
			float uvY1;

			if (tileData.isFlipX) {
				uvX0 = uvX + uvPerX;
				uvX1 = uvX;
			} else {
				uvX0 = uvX;
				uvX1 = uvX + uvPerX;
			}

			if (tileData.isFlipY) {
				uvY0 = uvY - uvPerY;
				uvY1 = uvY;
			} else {
				uvY0 = uvY;
				uvY1 = uvY - uvPerY;
			}

			

			// left, top
			int vertIdx;
			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x0, y0, z);
				vertList.Add (vec);

				Vector2 uv = new Vector2 (uvX0, uvY0);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x1, y0, z);
				vertList.Add (vec);

				Vector2 uv = new Vector2 (uvX1, uvY0);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x1, y1, z);
				vertList.Add (vec);
				//        XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX1, uvY1);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x0, y1, z);
				vertList.Add (vec);
				//        XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX0, uvY1);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);
		}

		private void AddVertex(
			int col, int row, 
			int layerIdx, int layerWidth, int layerHeight,
			int baseTileWidth, int baseTileHeight, 
			TileIdData tileData, TmxTileData tmxTile
		)
		{

		}

		private void AddVertex (int col, int row, 
		                      int layerIdx, int layerWidth, int layerHeight, 
		                      int baseTileWidth, int baseTileHeight, 
		                      TileIdData tileData, TileSet tile,
		                      List<Vector3> vertList, List<Vector2> uvList, List<int> indexList
                          // ,Dictionary<KeyValuePair<int, int>, int> XYToVertIdx
		)
		{
			int tileId = tileData.tileId;

			tileId = tileId - tile.FirstId;
			int deltaY = tile.TileHeight / baseTileHeight;

			int tileColCnt = Mathf.CeilToInt (tile.Image.Width / tile.TileWidth);
			int r = tileId / tileColCnt;
			int c = tileId % tileColCnt;
			float uvPerY = ((float)tile.TileHeight) / ((float)tile.Image.Height);
			float uvPerX = ((float)tile.TileWidth) / ((float)tile.Image.Width);
			float uvY = 1f - (float)(r) * uvPerY;
			float uvX = (float)(c) * uvPerX;

			const float _cScale = 0.01f;

			float x0 = (float)(col * baseTileWidth) * _cScale;
			//float y0 = ((float)((layerHeight - row) * baseTileHeight -  tile.TileHeight)) * 0.01f;
			float y0 = (-((float)row) + (deltaY - 1)) * baseTileHeight * _cScale; 
			float x1 = x0 + tile.TileWidth * _cScale;
			float y1 = y0 - tile.TileHeight * _cScale;

			float uvX0;
			float uvX1;
			float uvY0;
			float uvY1;

			if (tileData.isFlipX) {
				uvX0 = uvX + uvPerX;
				uvX1 = uvX;
			} else {
				uvX0 = uvX;
				uvX1 = uvX + uvPerX;
			}

			if (tileData.isFlipY) {
				uvY0 = uvY - uvPerY;
				uvY1 = uvY;
			} else {
				uvY0 = uvY;
				uvY1 = uvY - uvPerY;
			}

			float z = -layerIdx * 0.01f;

			// left, top
			int vertIdx;
			//    KeyValuePair<int, int> key = new KeyValuePair<int, int>(x, y);
			//   if (!XYToVertIdx.TryGetValue(key, out vertIdx))
			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x0, y0, z);
				vertList.Add (vec);
				//      XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX0, uvY0);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			// right, top
			//  key = new KeyValuePair<int, int>(x, y1);
			//    if (!XYToVertIdx.TryGetValue(key, out vertIdx))
			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x1, y0, z);
				vertList.Add (vec);
				//       XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX1, uvY0);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			// right, bottom
			//    key = new KeyValuePair<int, int>(x1, y1);
			//     if (!XYToVertIdx.TryGetValue(key, out vertIdx))
			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x1, y1, z);
				vertList.Add (vec);
				//        XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX1, uvY1);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);

			// left, bottom
			//    key = new KeyValuePair<int, int>(x1, y);
			//    if (!XYToVertIdx.TryGetValue(key, out vertIdx))
			{
				vertIdx = vertList.Count;
				Vector3 vec = new Vector3 (x0, y1, z);
				vertList.Add (vec);
				//        XYToVertIdx.Add(key, vertIdx);

				Vector2 uv = new Vector2 (uvX0, uvY1);
				uvList.Add (uv);
			}
			indexList.Add (vertIdx);
		}

		private Vector4 GetCamView(Camera cam)
		{
			
			Vector4 view;
			if (m_UseDesign && m_DesignWidth > 0 && m_DesignHeight > 0)
			{
				Vector2 pos = cam.transform.position * 100f;
				float orthW = (float)cam.pixelWidth/(float)cam.pixelHeight * ((float)cam.orthographicSize);
				orthW *= 100f;
//				float scale = m_DesignWidth/orthW;

				float halfW =  m_DesignWidth/2f;

				if (m_TileMap != null)
					halfW += m_TileMap.Size.TileWidth * 4;

				//float halfH  = ((float)m_DesignHeight)/2f * scale;
				float halfH = halfW;
				// 世界坐标系
				view = new Vector4(pos.x - halfW, pos.y + halfH, pos.x + halfW, pos.y - halfH);
;
			} else
			{
				Vector2 pos = cam.transform.position * 100f;
				float halfW = cam.pixelWidth/2f;
				float halfH = cam.pixelHeight/2f;
				// 世界坐标系
				view = new Vector4(pos.x - halfW, pos.y + halfH, pos.x + halfW, pos.y - halfH);
			}

			return view;
		}

		// 跳地图
		public void MeshJumpTo(Camera cam)
		{
			// 有問題
			if (cam == null)
				return;

			// 设置为5f
			cam.orthographicSize = 5f;

			Vector4 view = GetCamView(cam);

			MeshJumpTo(ref view, cam);
		}

		public void MeshMove(Camera cam)
		{
			if (cam == null)
				return;

			Vector4 view = GetCamView(cam);

			MeshMove(ref view);
		}

		// 移动
		public void MeshMove(ref Vector4 view)
		{
			if (m_TileMap == null)
				return;
			var meshMgr = this.MeshMgr;
			meshMgr.MoveMap(ref view);
		}

		// 跳地图
		public void MeshJumpTo (ref Vector4 view, Camera cam)
		{
			if (m_TileMap == null)
				return;
			var meshMgr = this.MeshMgr;
			meshMgr.JumpTo (ref view, m_TileMap, cam);
		}

		internal void SetTMXMeshManagerScale (TMXMeshManager mgr, Camera cam)
		{
			if (mgr == null || cam == null)
				return;

			Vector3 targetScale = GetTargetScale(cam);

			Transform targetTrans = mgr.transform;
			targetTrans.localScale = targetScale;
		}

		/*
		internal TMXMeshNode CreateTMXMeshNode(int minR, int maxR, int minC, int maxC, int layerIdx)
		{
			if (layerIdx < 0 || m_TileMap == null)
				return;

			var layers = m_TileMap.Layers;
			if (layers == null || layers.Count <= 0 || layerIdx >= layers.Count)
				return;

			if (minR < 0)
				minR = 0;
			if (maxR >= m_TileMap.Size.Height)
				maxR = m_TileMap.Size.Height - 1;
			if (minC < 0)
				minC = 0;
			if (maxC >= m_TileMap.Size.Width)
				maxC = m_TileMap.Size.Width - 1;

			for (int r = minR; r <= maxR; ++r) {
				
			}
		}*/

		internal void BuildTMXMeshNode (int row, int col, int layerIdx, TileIdData data)
		{
			if (layerIdx < 0 || data == null || m_TileMap == null)
				return;

			var layers = m_TileMap.Layers;
			if (layers == null || layers.Count <= 0 || layerIdx >= layers.Count)
				return;
		
			MapLayer layer = layers [layerIdx];

			TmxTileData tileSet = null;
			var matIter = m_TileDataMap.GetEnumerator ();
			while (matIter.MoveNext ()) {
				var t = matIter.Current.Value.Tile;
				if (t.ContainsTile (data.tileId)) {
					tileSet = matIter.Current.Value;
					break;
				}
			}
			matIter.Dispose ();

			if (tileSet == null)
				return;
			
			TMXMeshNode node = data.userData as TMXMeshNode;
			bool hasVertex = node != null;
			if (!hasVertex) {
				node = m_MeshMgr.CreateMeshNode ();
			}

			data.userData = node;
			node.tileData = data;

			if (hasVertex)
				return;

			AddVertex2 (col, row, layerIdx, layer.Width, layer.Height, 
				m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, data, 
				tileSet);
		}

		// 采用全散图
		public void BuildAllMeshMap(Camera cam)
		{
			if (m_TileMap == null || !m_TileMap.IsVaild)
				return;

			float halfW = ((float)m_TileMap.Size.TileWidth) * ((float)m_TileMap.Size.Width)/2f;
			float halfH = ((float)m_TileMap.Size.TileHeight) * ((float)m_TileMap.Size.Height)/2f;
			Vector4 view = new Vector4 (-halfW, halfH, halfW, -halfH);

			MeshJumpTo (ref view, cam);
		}

        // ------------------------------------------------分层显示Mesh--------------------------------------------------

		public void ClearMeshPerLayer(GameObject target)
		{
			if (target == null)
				return;

			Transform parent = target.transform.FindChild("LayerRoot");
			if (parent == null)
				return;

			for (int i = parent.childCount - 1; i >= 0; --i) {
				var trans = parent.GetChild(i);
				MeshFilter filter = trans.GetComponent<MeshFilter>();
				if (filter != null && filter.sharedMesh != null) {
					GameObject.Destroy(filter.sharedMesh);
					filter.sharedMesh = null;
				}
				GameObject.Destroy(trans.gameObject);
			}
		}

		private Transform InitLayerRoot(GameObject target)
		{
			if (target == null)
				return null;
			
			Transform parent = target.transform.FindChild("LayerRoot");
			if (parent == null) {
				GameObject root = new GameObject("LayerRoot");
				parent = root.transform;
				parent.localScale = Vector3.one;
				parent.parent = target.transform;
			}

			return parent;
		}

        // 一层一个材质Mesh
        public void BuildMeshPerLayer(Camera cam = null) {
            GameObject target = gameObject;
            ClearMeshPerLayer(target);
            if (m_TileMap == null || !m_TileMap.IsVaild || target == null)
                return;

            Transform parent = InitLayerRoot(target);

            // 设置顶点
            List<Vector3> vertList = new List<Vector3>();
            List<Vector2> uvList = new List<Vector2>();
            List<int> indexList = new List<int>();

            var matIter = m_TileDataMap.GetEnumerator();
            while (matIter.MoveNext()) {

                vertList.Clear();
                uvList.Clear();
                indexList.Clear();

                TmxTileData tmxData = matIter.Current.Value;
                if (tmxData == null || tmxData.Tile == null || !tmxData.Tile.IsVaid)
                    continue;

                for (int l = 0; l < m_TileMap.Layers.Count; ++l) {

                    vertList.Clear();
                    uvList.Clear();
                    indexList.Clear();

                    MapLayer layer = m_TileMap.Layers[l];
                    if (layer == null || !layer.IsVaild)
                        continue;

#if _USE_SPLIT_PERLAYER
                    int perRows = (int)(1024f/ m_TileMap.Size.TileHeight);
                    int perCols = (int)(1024f / m_TileMap.Size.TileWidth);
                    int maxRows = Mathf.CeilToInt(layer.Height / (float)perRows);
                    int maxCols = Mathf.CeilToInt(layer.Width / (float)perCols);

                    float lineX = 1f / (float)layer.Width * (float)perCols;
                    float lineY = 1f / (float)layer.Height * (float)perRows;

                    int maxCell = layer.Height * layer.Width;

                    for (int rows = 0; rows < maxRows; ++rows) {
                        int realRow = rows * perRows;
                        for (int cols = 0; cols < maxCols; ++cols) {

                            vertList.Clear();
                            uvList.Clear();
                            indexList.Clear();

                            int realCol = cols * perCols;
                            int startIdx = realRow * layer.Width + realCol;
                            for (int row = 0; row < perRows; ++row) {
                                if (row + realRow >= layer.Height)
                                    break;
                                for (int col = 0; col < perCols; ++col) {
                                    if (col + realCol >= layer.Width)
                                        break;
                                    int idx = startIdx + row * layer.Width + col;
                                    TileIdData tileData = layer.TileIds[idx];
                                    if (!tmxData.Tile.ContainsTile(tileData.tileId))
                                        continue;

                                    int c = realCol + col;
                                    int r = realRow + row;
#if _USE_ADDVERTEX2
                                    AddVertex2(c, r, l, layer.Width, layer.Height,
                                        m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight,
                                        tileData, tmxData.Tile,
                                        vertList, uvList, indexList/*, XYToVertIdx*/, m_TileMap.TileType);
#else
						AddVertex(c, r, l, layer.Width, layer.Height, 
								m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, 
								tileData, tmxData.Tile, 
								vertList, uvList, indexList/*, XYToVertIdx*/);
#endif

                                }
                            }

                            if (vertList.Count > 0) {
                                string name = string.Format("{0:D}:{1:D}", cols, rows);
                                GameObject gameObj = new GameObject(name);
                                var trans = gameObj.transform;
                                trans.parent = parent;
                                trans.localScale = Vector3.one;
                          //      trans.localPosition = new Vector3(cols * lineX, rows * lineY, 0);
                                MeshFilter filter = gameObj.AddComponent<MeshFilter>();
                                Mesh mesh = new Mesh();
                                filter.sharedMesh = mesh;

                                // 设置顶点
                                mesh.SetVertices(vertList);
                                // 设置UV
                                mesh.SetUVs(0, uvList);
                                mesh.subMeshCount = 1;
                                mesh.SetIndices(indexList.ToArray(), MeshTopology.Quads, 0);

                                mesh.RecalculateBounds();
                                mesh.UploadMeshData(false);

                                MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();
                                renderer.sharedMaterial = tmxData.Mat;
                            }


                        }
                    }

#else


                    for (int r = 0; r < layer.Height; ++r) {
                        for (int c = 0; c < layer.Width; ++c) {
                            int idx = r * layer.Width + c;
                            TileIdData tileData = layer.TileIds[idx];
                            if (!tmxData.Tile.ContainsTile(tileData.tileId))
                                continue;

#if _USE_ADDVERTEX2
                            AddVertex2(c, r, l, layer.Width, layer.Height,
                                m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight,
                                tileData, tmxData.Tile,
                                vertList, uvList, indexList/*, XYToVertIdx*/, m_TileMap.TileType);
#else
						AddVertex(c, r, l, layer.Width, layer.Height, 
								m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, 
								tileData, tmxData.Tile, 
								vertList, uvList, indexList/*, XYToVertIdx*/);
#endif

                        }
                    }

                    if (vertList.Count > 0) {
                        GameObject gameObj = new GameObject();
                        var trans = gameObj.transform;
                        trans.parent = parent;
                        trans.localScale = Vector3.one;
                        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
                        Mesh mesh = new Mesh();
                        filter.sharedMesh = mesh;

                        // 设置顶点
                        mesh.SetVertices(vertList);
                        // 设置UV
                        mesh.SetUVs(0, uvList);
                        mesh.subMeshCount = 1;
                        mesh.SetIndices(indexList.ToArray(), MeshTopology.Quads, 0);

                        mesh.RecalculateBounds();
                        mesh.UploadMeshData(false);

                        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = tmxData.Mat;
                    }
#endif

                }

            }
            matIter.Dispose();

#if _USE_ADDVERTEX2
            // 摄影机Size

            if (target != null) {

                Vector3 targetScale = GetTargetScale(cam);

                Transform targetTrans = target.transform;
                targetTrans.localScale = targetScale;
            }
#endif

            // 合并批次, 优化DrawCall
            StaticBatchingUtility.Combine(parent.gameObject);
        }

        //------------------------------------------------------------------------------------------------------------------------

        public void ClearAllToMesh(GameObject target)
		{
			if (target == null)
				return;

			MeshFilter meshFilter = target.GetComponent<MeshFilter> ();
			if (meshFilter != null)
			{
				if (meshFilter.sharedMesh != null)
				{
					GameObject.Destroy(meshFilter.sharedMesh);
					meshFilter.sharedMesh = null;
				}
			}
		}

        // 全部到整Mesh
        public void BuildAllToMesh (GameObject target, Camera cam = null)
		{
			ClearAllToMesh(target);
			if (m_TileMap == null || !m_TileMap.IsVaild || target == null)
				return;

			MeshRenderer renderer = null;
			if (target != null) {
				renderer = target.GetComponent<MeshRenderer> ();
				if (renderer == null)
					renderer = target.AddComponent<MeshRenderer> ();
			}

			MeshFilter meshFilter = null;
			Mesh mesh = null;
			if (target != null) {
				meshFilter = target.GetComponent<MeshFilter> ();
				if (meshFilter == null)
					meshFilter = target.AddComponent<MeshFilter> ();
				mesh = new Mesh();
				meshFilter.sharedMesh = mesh;
			} else
				return;
	


			// 设置顶点
			List<Vector3> vertList = new List<Vector3> ();
			List<Vector2> uvList = new List<Vector2> ();
			List<List<int>> indexLists = new List<List<int>> ();
			List<Material> matList = new List<Material> ();
			//Dictionary<KeyValuePair<int, int>, int> XYToVertIdx = new Dictionary<KeyValuePair<int, int>, int>();

			var matIter = m_TileDataMap.GetEnumerator ();
			while (matIter.MoveNext ()) {
				TmxTileData tmxData = matIter.Current.Value;
				if (tmxData == null || tmxData.Tile == null || !tmxData.Tile.IsVaid)
					continue;
				matList.Add (tmxData.Mat);

				//	int tilePerX = tmxData.Tile.TileWidth / m_TileMap.Size.TileWidth;
				//	int tilePerY = tmxData.Tile.TileHeight / m_TileMap.Size.TileHeight;
				// XYToVertIdx.Clear();
				List<int> indexList = null;
           
				for (int l = 0; l < m_TileMap.Layers.Count; ++l) {
					MapLayer layer = m_TileMap.Layers [l];
					if (layer == null || !layer.IsVaild)
						continue;

					for (int r = 0; r < layer.Height; ++r) {
						for (int c = 0; c < layer.Width; ++c) {
							int idx = r * layer.Width + c;
							TileIdData tileData = layer.TileIds [idx];
							if (!tmxData.Tile.ContainsTile (tileData.tileId))
								continue;

							if (indexList == null)
								indexList = new List<int> ();
							#if _USE_ADDVERTEX2
							AddVertex2 (c, r, l, layer.Width, layer.Height, 
								m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, 
								tileData, tmxData.Tile, 
								vertList, uvList, indexList/*, XYToVertIdx*/, m_TileMap.TileType);
							#else
						AddVertex(c, r, l, layer.Width, layer.Height, 
								m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, 
								tileData, tmxData.Tile, 
								vertList, uvList, indexList/*, XYToVertIdx*/);
							#endif
                      
						}
					}
				}

				if (indexList != null && indexList.Count > 0) {
					// 添加SUBMESH
					indexLists.Add (indexList);
				}

			}
			matIter.Dispose ();

			// 设置顶点
			mesh.SetVertices (vertList);
			// 设置UV
			mesh.SetUVs (0, uvList);

			mesh.subMeshCount = indexLists.Count;
			int subMesh = 0;
			for (int i = 0; i < indexLists.Count; ++i) {
				var indexList = indexLists [i];
				if (indexList != null && indexList.Count > 0)
					mesh.SetIndices (indexList.ToArray (), MeshTopology.Quads, subMesh++);
			}

			mesh.RecalculateBounds ();
			mesh.UploadMeshData (true);

			#if _USE_ADDVERTEX2
			// 摄影机Size

			if (target != null) {

				Vector3 targetScale = GetTargetScale(cam);

				Transform targetTrans = target.transform;
				targetTrans.localScale = targetScale;
			}
			#endif

			if (renderer != null && m_TileDataMap.Count > 0) {
				renderer.sharedMaterials = matList.ToArray ();
			}
		}

		private Vector3 GetTargetScale(Camera cam)
		{
			Vector3 targetScale = new Vector3 (m_TileMap.Size.Width * m_TileMap.Size.TileWidth, 
				m_TileMap.Size.Height * m_TileMap.Size.TileHeight, 
				1f);

			targetScale *= m_Scale;

			if (m_UseDesign)
			{
				// 自适应
				float scale = cam.orthographicSize/(m_DesignHeight/2);
				float midW = ((float)m_DesignWidth) / ((float)m_DesignHeight) * cam.pixelHeight;
				scale *= cam.pixelWidth / midW;
				targetScale *= scale;
				//---------
			}

			return targetScale;
		}

		public string ResRootPath {
			get {
				return m_ResRootPath;
			}
		}

		public TileMap Tile {
			get {
				return m_TileMap;
			}
		}

		public TmxTileData FindTileData (int tileId)
		{
			TmxTileData ret = null;
			var iter = m_TileDataMap.GetEnumerator ();
			while (iter.MoveNext ()) {
				TmxTileData data = iter.Current.Value;
				if (data != null && data.Tile != null) {
					if (data.Tile.ContainsTile (tileId)) {
						ret = data;
						break;
					}
				}
			}
			iter.Dispose ();

			return ret;
		}

		void OnDestroy ()
		{
			Clear ();
		}

		private void InitMeshMgr()
		{
			if (m_MeshMgr == null)
			{
				GameObject obj = new GameObject ();
				var trans = obj.transform;
				trans.parent = transform;
				trans.localScale = Vector3.one;
				trans.localPosition = Vector3.zero;
				trans.localRotation = Quaternion.identity;
				obj.name = "TMXMeshManager";
				m_MeshMgr = obj.AddComponent<TMXMeshManager> ();
			}
		}

		private TMXMeshManager MeshMgr
		{
			get {
				if (m_MeshMgr == null) {
					InitMeshMgr();
				}
				return m_MeshMgr;
			}
		}

		#if _USE_ADDVERTEX2
		public bool m_UseDesign = true;
		private float m_DesignWidth = 1280;
		public int m_DesignHeight = 720;
		public float m_Scale = 1.0f;
		#endif

		public bool HasCacheMeshNode
		{
			get
			{
				return (m_MeshMgr != null) && (m_MeshMgr.PoolCount > 0);
			}
		}

		private string m_ResRootPath = string.Empty;
		private TileMap m_TileMap = null;
		private TMXMeshManager m_MeshMgr = null;
		private Dictionary<int, TmxTileData> m_TileDataMap = new Dictionary<int, TmxTileData> ();
	}

}
