using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using TmxCSharp.Renderer;
using XmlParser;

// TMX地图渲染
public class TMXRenderer : MonoBehaviour, ITmxTileDataParent
{
	public bool LoadMapFromFile(string fileName, ITmxLoader loader)
    {
        Clear();
		if (string.IsNullOrEmpty(fileName) || loader == null)
            return false;

		TileMap tileMap = TmxLoader.Parse(fileName, loader);

        bool ret = tileMap != null && tileMap.IsVaild;
        if (ret)
        {
            m_TileMap = tileMap;
            m_ResRootPath = Path.GetDirectoryName(fileName);
            LoadRes(tileMap);
        }

        return ret;
    }

    private void Clear()
    {
        ClearTileData();
        m_ResRootPath = string.Empty;
    }

    private void ClearTileData()
    {
        var iter = m_TileDataMap.GetEnumerator();
        while (iter.MoveNext())
        {
            if (iter.Current.Value != null)
                iter.Current.Value.Destroy();
        }
        iter.Dispose();
        m_TileDataMap.Clear();

        m_TileMap = null;
    }

    private void LoadRes(TileMap tileMap)
    {
        if (tileMap == null)
            return;

        var tileSets = tileMap.TileSets;
        if (tileSets != null)
        {
            for (int i = 0; i < tileSets.Count; ++i)
            {
                TileSet tileSet = tileSets[i];
                if (tileSet == null || !tileSet.IsVaid)
                    continue;

                TmxTileData tileData = new TmxTileData(tileSet, this);
                if (m_TileDataMap.ContainsKey(tileSet.FirstId))
                    m_TileDataMap[tileSet.FirstId] = tileData;
                else
                {
                    m_TileDataMap.Add(tileSet.FirstId, tileData);
                }
            }
        }
    }

	private void AddVertex2(

		int col, int row, 
		int layerIdx, int layerWidth, int layerHeight, 
		int baseTileWidth, int baseTileHeight, 
		ref TileIdData tileData, TileSet tile,
		List<Vector3> vertList, List<Vector2> uvList, List<int> indexList
		// ,Dictionary<KeyValuePair<int, int>, int> XYToVertIdx

		)
	{
		Vector2 _meshsize_ = new Vector2(1.0f/((float)layerWidth), 1.0f/((float)layerHeight));
		Vector2 _pivotPoint = new Vector2((col-1) * _meshsize_.x * -1 - _meshsize_.x / 2f, (row-1) * _meshsize_.y + _meshsize_.y / 2f); 
		_pivotPoint.x += .5f;
		_pivotPoint.y -= .5f;
		float dx = ((float)tile.TileWidth / (float)baseTileWidth) - 1;
		float dy = ((float)tile.TileHeight /(float)baseTileHeight) - 1;

		int tileId = tileData.tileId;
		tileId = tileId - tile.FirstId;
		int deltaY = tile.TileHeight/baseTileHeight;
		int tileColCnt = Mathf.CeilToInt(tile.Image.Width / tile.TileWidth);
		int r = tileId / tileColCnt;
		int c = tileId % tileColCnt;
		float uvPerY = ((float)tile.TileHeight) / ((float)tile.Image.Height);
		float uvPerX = ((float)tile.TileWidth) / ((float)tile.Image.Width);
		float uvY = 1f - (float)(r) * uvPerY;
		float uvX = (float)(c) * uvPerX;

		float x0 = ((_meshsize_.x/2) * -1) - _pivotPoint.x;
		float y0 = (_meshsize_.y/2) - _pivotPoint.y + (dy * _meshsize_.y);
		float x1 = (_meshsize_.x/2) - _pivotPoint.x + (dx * _meshsize_.x);
		float y1 = ((_meshsize_.y/2) * -1) - _pivotPoint.y;

		float uvX0;
		float uvX1;
		float uvY0;
		float uvY1;

		if (tileData.isFlipX)
		{
			uvX0 = uvX + uvPerX;
			uvX1 = uvX;
		} else
		{
			uvX0 = uvX;
			uvX1 = uvX + uvPerX;
		}

		if (tileData.isFlipY)
		{
			uvY0 = uvY - uvPerY;
			uvY1 = uvY;
		} else
		{
			uvY0 = uvY;
			uvY1 = uvY - uvPerY;
		}

		float z = -layerIdx * 0.01f;

		// left, top
		int vertIdx;
		{
			vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x0, y0, z);
			vertList.Add(vec);

			Vector2 uv = new Vector2(uvX0, uvY0);
			uvList.Add(uv);
		}
		indexList.Add(vertIdx);

		{
			vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x1, y0, z);
			vertList.Add(vec);

			Vector2 uv = new Vector2(uvX1, uvY0);
			uvList.Add(uv);
		}
		indexList.Add(vertIdx);

		{
			vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x1, y1, z);
			vertList.Add(vec);
			//        XYToVertIdx.Add(key, vertIdx);

			Vector2 uv = new Vector2(uvX1, uvY1);
			uvList.Add(uv);
		}
		indexList.Add(vertIdx);

		{
			vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x0, y1, z);
			vertList.Add(vec);
			//        XYToVertIdx.Add(key, vertIdx);

			Vector2 uv = new Vector2(uvX0, uvY1);
			uvList.Add(uv);
		}
		indexList.Add(vertIdx);
	}

	private void AddVertex(int col, int row, 
						   int layerIdx, int layerWidth, int layerHeight, 
						   int baseTileWidth, int baseTileHeight, 
							ref TileIdData tileData, TileSet tile,
                           List<Vector3> vertList, List<Vector2> uvList, List<int> indexList
                          // ,Dictionary<KeyValuePair<int, int>, int> XYToVertIdx
                          )

    {
		int tileId = tileData.tileId;

		tileId = tileId - tile.FirstId;
		int deltaY = tile.TileHeight/baseTileHeight;

		int tileColCnt = Mathf.CeilToInt(tile.Image.Width / tile.TileWidth);
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

		if (tileData.isFlipX)
		{
			uvX0 = uvX + uvPerX;
			uvX1 = uvX;
		} else
		{
			uvX0 = uvX;
			uvX1 = uvX + uvPerX;
		}

		if (tileData.isFlipY)
		{
			uvY0 = uvY - uvPerY;
			uvY1 = uvY;
		} else
		{
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
			Vector3 vec = new Vector3(x0, y0, z);
            vertList.Add(vec);
      //      XYToVertIdx.Add(key, vertIdx);

            Vector2 uv = new Vector2(uvX0, uvY0);
            uvList.Add(uv);
        }
        indexList.Add(vertIdx);

        // right, top
      //  key = new KeyValuePair<int, int>(x, y1);
    //    if (!XYToVertIdx.TryGetValue(key, out vertIdx))
        {
            vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x1, y0, z);
            vertList.Add(vec);
     //       XYToVertIdx.Add(key, vertIdx);

			Vector2 uv = new Vector2(uvX1, uvY0);
            uvList.Add(uv);
        }
        indexList.Add(vertIdx);

        // right, bottom
    //    key = new KeyValuePair<int, int>(x1, y1);
   //     if (!XYToVertIdx.TryGetValue(key, out vertIdx))
        {
            vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x1, y1, z);
            vertList.Add(vec);
    //        XYToVertIdx.Add(key, vertIdx);

            Vector2 uv = new Vector2(uvX1, uvY1);
            uvList.Add(uv);
        }
        indexList.Add(vertIdx);

        // left, bottom
    //    key = new KeyValuePair<int, int>(x1, y);
    //    if (!XYToVertIdx.TryGetValue(key, out vertIdx))
        {
            vertIdx = vertList.Count;
			Vector3 vec = new Vector3(x0, y1, z);
			vertList.Add(vec);
    //        XYToVertIdx.Add(key, vertIdx);

            Vector2 uv = new Vector2(uvX0, uvY1);
            uvList.Add(uv);
        }
        indexList.Add(vertIdx);
    }

	// 根据摄影机生成TILE
	public void BuildMesh(Camera camera)
	{
		if (camera == null)
			return;
		
	}

    // 全部到Mesh
    public void BuildAllToMesh(Mesh mesh)
    {
        if (m_TileMap == null || !m_TileMap.IsVaild || mesh == null)
            return;

        mesh.Clear();

        // 设置顶点
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
		List<List<int>> indexLists = new List<List<int>>();
        //Dictionary<KeyValuePair<int, int>, int> XYToVertIdx = new Dictionary<KeyValuePair<int, int>, int>();
	
        var matIter = m_TileDataMap.GetEnumerator();
        while (matIter.MoveNext())
        {
            TmxTileData tmxData = matIter.Current.Value;
            if (tmxData == null || tmxData.Tile == null || !tmxData.Tile.IsVaid)
                continue;
			
			int tilePerX = tmxData.Tile.TileWidth / m_TileMap.Size.TileWidth;
			int tilePerY = tmxData.Tile.TileHeight / m_TileMap.Size.TileHeight;
           // XYToVertIdx.Clear();
			List<int> indexList = null;
           
            for (int l = 0; l < m_TileMap.Layers.Count; ++l)
            {
                MapLayer layer = m_TileMap.Layers[l];
                if (layer == null || !layer.IsVaild)
                    continue;

                for (int r = 0; r < layer.Height; ++r)
                {
                    for (int c = 0; c < layer.Width; ++c)
                    {
						TileIdData tileData = layer.TileIds[r, c];
						if (!tmxData.Tile.ContainsTile(tileData.tileId))
							continue;

						if (indexList == null)
							indexList = new List<int>();
						
						AddVertex(c, r, l, layer.Width, layer.Height, 
								m_TileMap.Size.TileWidth, m_TileMap.Size.TileHeight, 
								ref tileData, tmxData.Tile, 
								vertList, uvList, indexList/*, XYToVertIdx*/);
                      
                    }
                }
            }

			if (indexList != null && indexList.Count > 0)
            {
                // 添加SUBMESH
				indexLists.Add(indexList);
            }

        }
        matIter.Dispose();

        // 设置顶点
        mesh.SetVertices(vertList);
        // 设置UV
        mesh.SetUVs(0, uvList);

		mesh.subMeshCount = indexLists.Count;
		int subMesh = 0;
		for (int i = 0; i < indexLists.Count; ++i)
		{
			var indexList = indexLists[i];
			if (indexList != null && indexList.Count > 0)
				mesh.SetIndices(indexList.ToArray(), MeshTopology.Quads, subMesh++);
		}

		mesh.RecalculateBounds();
		mesh.UploadMeshData(true);
    }

    public string ResRootPath
    {
        get
        {
            return m_ResRootPath;
        }
    }

    void Start()
    {
        // 测试
     //   LoadMapFromFile("maps/@testtmx/d104.tmx.bytes");
    }

    public TileMap Tile
    {
        get
        {
            return m_TileMap;
        }
    }

    public TmxTileData FindTileData(int tileId)
    {
        TmxTileData ret = null;
        var iter = m_TileDataMap.GetEnumerator();
        while (iter.MoveNext())
        {
            TmxTileData data = iter.Current.Value;
            if (data != null && data.Tile != null)
            {
                if (data.Tile.ContainsTile(tileId))
                {
                    ret = data;
                    break;
                }
            }
        }
        iter.Dispose();

        return ret;
    }

    void OnDestroy()
    {
        Clear();
    }

    private string m_ResRootPath = string.Empty;
    private TileMap m_TileMap = null;
    private Dictionary<int, TmxTileData> m_TileDataMap = new Dictionary<int, TmxTileData>();
}
