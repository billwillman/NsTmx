using System;
using UnityEngine;
using TmxCSharp.Loader;
using TmxCSharp.Models;

namespace TmxCSharp.Renderer
{

	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshFilter))]
	public class TMXMeshNode: MonoBehaviour
	{

		public TileIdData tileData {
			get;
			set;
		}

		public void UpdateMesh()
		{
			if (m_Mesh == null) {
				m_Mesh = new Mesh ();
				m_Mesh.MarkDynamic();
				MeshFilter filter = this.Filter;
				filter.sharedMesh = m_Mesh;
			}

			m_Mesh.vertices = m_VertBuf;
			m_Mesh.uv = m_UVBuf;
			m_Mesh.SetIndices (m_IndexBuf, MeshTopology.Quads, 0);
			m_Mesh.UploadMeshData (false);
		}

		public MeshRenderer Renderer {
			get {
				if (m_Render == null)
					m_Render = GetComponent<MeshRenderer> ();
				return m_Render;
			}
		}

		public MeshFilter Filter
		{
			get
			{
				if (m_Filter == null)
					m_Filter = GetComponent<MeshFilter>();
				return m_Filter;
			}
		}

		public void Destroy ()
		{
			if (IsDestroy)
				return;

			IsDestroy = true;
			tileData = null;
		}

		internal bool IsDestroy {
			get;
			set;
		}

		void OnDestroy ()
		{
			Destroy ();
		}

		public Vector3[] VertBuf
		{
			get
			{
				return m_VertBuf;
			}
		}

		public Vector2[] UVBuf
		{
			get
			{
				return m_UVBuf;
			}
		}

		public int[] IndexBuf
		{
			get
			{
				return m_IndexBuf;
			}
		}

		public void InitBuf(int vertCnt)
		{
			m_VertBuf = new Vector3[vertCnt];
			m_UVBuf = new Vector2[vertCnt];
			m_IndexBuf = new int[vertCnt];
		}

		private Mesh m_Mesh = null;
		private MeshRenderer m_Render = null;
		private MeshFilter m_Filter = null;

		private Vector3[] m_VertBuf = null;
		private Vector2[] m_UVBuf = null;
		private int[] m_IndexBuf = null;
	}

}

