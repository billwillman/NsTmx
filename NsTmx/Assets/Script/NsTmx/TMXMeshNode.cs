using System;
using UnityEngine;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using TmxCSharp.Renderer;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TMXMeshNode: MonoBehaviour
{

	public TileIdData tileData
	{
		get;
		set;
	}

	public Mesh mesh
	{
		get
		{
			return m_Mesh;
		}
	}

	public void Destroy()
	{
		tileData = null;
	}

	void OnDestroy()
	{
		Destroy ();
	}

	private Mesh m_Mesh = null;
}

