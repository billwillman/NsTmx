using System;
using System.Collections;
using System.Collections.Generic;

namespace TmxCSharp.Models
{
	public class TileIdData
	{
		public int tileId
		{
			get;
			set;
		}

		public bool isFlipX
		{
			get;
			set;
		}

		public bool isFlipY
		{
			get;
			set;
		}

		public System.Object userData
		{
			get;
			set;
		}

		public TileIdData XNodePrev {
			get;
			set;
		}

		public TileIdData XNodeNext {
			get;
			set;
		}

		public TileIdData YNodePrev
		{
			get;
			set;
		}

		public TileIdData YNodeNext
		{
			get;
			set;
		}
	}

    public class MapLayer
    {
        public MapLayer(string name, int width, int height)
        {
            Name = name;

            Width = width;

            Height = height;

			TileIds = null;

			TileIdsGroup = null;
        }

        public bool IsVaild
        {
            get
            {
				return (Width > 0) && (Height > 0) && (TileIds != null) && (TileIds.Count > 0);
            }
        }

        public string Name { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

		public IList<TileIdData> TileIds {
			get;
			internal set;
		}

		public TileIdGroups TileIdsGroup
		{
			get;
			internal set;
		}
    }
}