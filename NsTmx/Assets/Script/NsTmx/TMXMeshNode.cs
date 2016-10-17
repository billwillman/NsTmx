using System;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TMXMeshNode: MonoBehaviour
{
	/* 节点 */

	public TMXMeshNode XNodePrev {
		get;
		set;
	}

	public TMXMeshNode XNodeNext {
		get;
		set;
	}

	public TMXMeshNode YNodePrev
	{
		get;
		set;
	}

	public TMXMeshNode YNodeNext
	{
		get;
		set;
	}

	//---------------------------------------------

	public Mesh mesh
	{
		get
		{
			return m_Mesh;
		}
	}

	public void Destroy()
	{
	}

	void OnDestroy()
	{
		Destroy ();
	}

	private Mesh m_Mesh = null;
}

