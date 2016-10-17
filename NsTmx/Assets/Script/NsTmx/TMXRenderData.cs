using System;
using System.Collections;
using System.IO;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using UnityEngine;

namespace TmxCSharp.Renderer
{
    public interface ITmxTileDataParent
    {
        // 资源根目录
        string ResRootPath
        {
            get;
        }
    }

    public class TmxTileData
    {
        public TmxTileData(TileSet tile, ITmxTileDataParent parent)
        {
            m_Tile = tile;
            m_Parent = parent;
        }

        public TileSet Tile
        {
            get
            {
                return m_Tile;
            }
        }

		public Material Mat
		{
			get
			{
				if (m_Mat == null) {
					if (m_Tile != null && m_Tile.Image != null && m_Parent != null) {
						string resRootPath = m_Parent.ResRootPath;
						if (!string.IsNullOrEmpty(resRootPath))
						{
							string fileName = Path.GetFileName(m_Tile.Image.FilePath);
							if (!string.IsNullOrEmpty(fileName))
							{
								fileName = Path.GetFileNameWithoutExtension (fileName);
								fileName = string.Format("{0}/{1}.mat", resRootPath, fileName);
								m_Mat = TmxLoader.Loader._LoadMaterial(fileName);
							}
						}
					}
				}

				return m_Mat;
			}
		}

        public void Destroy()
        {
			if (m_Mat != null) {
				TmxLoader.Loader._DestroyResource(m_Mat);
				m_Mat = null;
			}
        }

        private TileSet m_Tile = null;
		private Material m_Mat = null;
        private ITmxTileDataParent m_Parent = null;
    }
}