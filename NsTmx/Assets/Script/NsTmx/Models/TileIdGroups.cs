using System;

namespace TmxCSharp.Models
{
	// AOI可视范围（每一层有个这样的Group）
	public class TileIdGroups
	{
		public TileIdData XStartNode {
			get;
			set;
		}

		public TileIdData XEndNode {
			get;
			set;
		}

		public TileIdData YStartNode
		{
			get;
			set;
		}

		public TileIdData YEndNode
		{
			get;
			set;
		}
	}
}

