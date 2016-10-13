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

        public Texture Tex
        {
            get
            {
                if (m_Tex == null)
                {
                    if (m_Tile != null && m_Tile.Image != null && m_Parent != null)
                    {
                        string resRootPath = m_Parent.ResRootPath;
                        if (!string.IsNullOrEmpty(resRootPath))
                        {
                            string fileName = Path.GetFileName(m_Tile.Image.FilePath);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                fileName = string.Format("{0}/{1}", resRootPath, fileName);
								m_Tex = TmxLoader.Loader._LoadTexture(resRootPath);
                            }
                        }
                    }
                }

                return m_Tex;
            }
        }

        public void Destroy()
        {
            if (m_Tex != null)
            {
				TmxLoader.Loader._DestroyResource(m_Tex);
                m_Tex = null;
            }
        }

        private TileSet m_Tile = null;
        private Texture m_Tex = null;
        private ITmxTileDataParent m_Parent = null;
    }
}