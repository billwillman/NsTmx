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
			if (m_Mesh == null)
			{
				m_Mesh = new Mesh();
				m_Mesh.vertices = new Vector3[4];
				m_Mesh.uv = new Vector2[4];
				m_Mesh.SetIndices(new int[4], MeshTopology.Quads, 0);
				m_Mesh.UploadMeshData(false);
				MeshFilter filter = GetComponent<MeshFilter>();
				filter.sharedMesh = m_Mesh;
			}
			return m_Mesh;
		}
	}

	public MeshRenderer Renderer
	{
		get
		{
			if (m_Render == null)
				m_Render = GetComponent<MeshRenderer>();
			return m_Render;
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
	private MeshRenderer m_Render = null;
}

