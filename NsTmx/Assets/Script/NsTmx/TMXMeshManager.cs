using System;
using UnityEngine;
using Utils;

// AOI算法
// TMX可视范围的MESH管理
public class TMXMeshManager
{
	public void Build(Camera cam)
	{
		Clear ();

		if (cam == null)
			return;


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



			node = node.XNodeNext;
		}

		m_XFirstNode = null;
		m_YFirstNode = null;
	}

	// 池
	private ObjectPool<TMXMeshNode> m_Pool = null;
	private TMXMeshNode m_XFirstNode = null;
	private TMXMeshNode m_YFirstNode = null;
}
