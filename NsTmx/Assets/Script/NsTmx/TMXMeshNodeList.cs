using System;
using System.Collections;
using System.Collections.Generic;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using Utils;

namespace TmxCSharp.Renderer
{
	internal class TMXMeshNodeLoader
	{
		public TileIdData data {
			get;
			set;
		}

		public int layerIdx {
			get;
			set;
		}

		public int row {
			get;
			set;
		}

		public int col {
			get;
			set;
		}

		public  LinkedListNode<TMXMeshNodeLoader> ListNode
		{
			get
			{
				return m_ListNode;
			}
		}

		public TMXMeshNodeLoader ()
		{
			m_ListNode = new LinkedListNode<TMXMeshNodeLoader> (this);
		}

		private LinkedListNode<TMXMeshNodeLoader> m_ListNode = null;
	}

	public class TMXMeshNodeLoaderList: Singleton<TMXMeshNodeLoaderList>
	{

		private class AsyncLoadKeyComparser: StructComparser<LoaderNodeKey>
		{}

		private struct LoaderNodeKey: IEquatable<LoaderNodeKey>
		{
			public bool Equals (LoaderNodeKey other)
			{
				return this == other;
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;

				if (GetType () != obj.GetType ())
					return false;

				if (obj is LoaderNodeKey) {
					LoaderNodeKey other = (LoaderNodeKey)obj;
					return Equals (other);
				} else
					return false;

			}

			public override int GetHashCode ()
			{
				int ret = FilePathMgr.InitHashValue ();
				FilePathMgr.HashCode (ref ret, row);
				FilePathMgr.HashCode (ref ret, col);
				FilePathMgr.HashCode (ref ret, layerIdx);
				return ret;
			}

			public static bool operator == (LoaderNodeKey a, LoaderNodeKey b)
			{
				return (a.row == b.row) && (a.col == b.col) && (a.layerIdx == b.layerIdx);
			}

			public static bool operator != (LoaderNodeKey a, LoaderNodeKey b)
			{
				return !(a == b);
			}

			public int row;
			public int col;
			public int layerIdx;
		}


		public void AddLoader(int row, int col, int layerIndex, TileIdData data)
		{
			if (data == null || Renderer == null)
				return;

			TMXMeshNode node = data.userData as TMXMeshNode;
			if (node != null || (Renderer != null && Renderer.HasCacheMeshNode))
			{
				// 说明已经有顶点数据
				Renderer.BuildTMXMeshNode(row, col, layerIndex, data);
				return;	
			}

			LoaderNodeKey key = new LoaderNodeKey();
			key.row = row;
			key.col = col;
			key.layerIdx = layerIndex;
			TMXMeshNodeLoader loader;
			if (m_LoaderMap.TryGetValue(key, out loader))
			{
				loader.data = data;
				return;
			}

			loader = CreateLoader(row, col, layerIndex, data);
			m_LoaderMap.Add(key, loader);
			m_List.AddLast(loader.ListNode);
		}

		public void RemoveLoader(int row, int col, int layerIndex)
		{
			LoaderNodeKey key = new LoaderNodeKey();
			key.row = row;
			key.col = col;
			key.layerIdx = layerIndex;
			TMXMeshNodeLoader loader;
			if (m_LoaderMap.TryGetValue(key, out loader))
			{
				m_List.Remove(loader.ListNode);
				m_LoaderMap.Remove(key);
				InPool(loader);
			}
		}

		public void Clear()
		{
			var node = m_List.First;
			while (node != null)
			{
				var value = node.Value;
				InPool(value);
				node = node.Next;
			}
			m_List.Clear();
			m_LoaderMap.Clear();
		}

		private static void InitPool()
		{
			if (m_InitPool)
				return;
			m_InitPool = true;
			m_Pool.Init(0);
		}

		private static void InPool(TMXMeshNodeLoader loader)
		{
			if (loader == null)
				return;
			InitPool();
			loader.layerIdx = -1;
			loader.row = -1;
			loader.col = -1;
			loader.data = null;
		
			m_Pool.Store(loader);
		}

		private static TMXMeshNodeLoader CreateLoader(int row, int col, int layerIdx, TileIdData data)
		{
			if (data == null)
				return null;

			InitPool();

			TMXMeshNodeLoader ret = m_Pool.GetObject();
			ret.row = row;
			ret.col = col;
			ret.layerIdx = layerIdx;
			ret.data = data;
			return ret;
		}

		private void Process(TMXMeshNodeLoader loader)
		{
			if (loader == null || Renderer == null)
				return;

			Renderer.BuildTMXMeshNode(loader.row, loader.col, loader.layerIdx, loader.data);
		}

		private void OnLoaderTimer(Timer time, float delta)
		{
			var node = m_List.First;

			int cnt = 1;
			const int cMaxLoad = 500;
			while (node != null && cnt <= cMaxLoad)
			{
				var nextNode = node.Next;
				var loader = node.Value;
				Process(loader);

				LoaderNodeKey key = new LoaderNodeKey();
				key.row = loader.row;
				key.col = loader.col;
				key.layerIdx = loader.layerIdx;
				m_List.Remove(node);
				m_LoaderMap.Remove(key);

				InPool(loader);

				node = nextNode;
				++cnt;
			}
		}

		public TMXMeshNodeLoaderList()
		{
			m_Timer = TimerMgr.Instance.CreateTimer(false, 0, true, true);
			m_Timer.AddListener(OnLoaderTimer);
		}

		public TMXRenderer Renderer
		{
			get;
			set;
		}

		private static ObjectPool<TMXMeshNodeLoader> m_Pool = new ObjectPool<TMXMeshNodeLoader>();
		private static bool m_InitPool = false;

		private LinkedList<TMXMeshNodeLoader> m_List = new LinkedList<TMXMeshNodeLoader> ();
		private Dictionary<LoaderNodeKey, TMXMeshNodeLoader> m_LoaderMap = new Dictionary<LoaderNodeKey, TMXMeshNodeLoader>(AsyncLoadKeyComparser.Default);
		private Timer m_Timer = null;
	}
		
}
