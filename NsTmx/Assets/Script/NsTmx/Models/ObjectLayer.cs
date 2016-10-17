using System;
using System.Collections;
using System.Collections.Generic;

namespace TmxCSharp.Models
{
	public class ObjectLayer
	{
		public ObjectLayer(string name, int x, int y, int width, int height, string type = "")
		{
			Name = name;
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Type = type;
		}

		public string Name {
			get;
			private set;
		}

		public string Type {
			get;
			private set;
		}

		public int X
		{
			get;
			private set;
		}

		public int Y {
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public bool HasProp
		{
			get {
				return (m_Props != null) && m_Props.HasProp;
			}
		}

		public void AddProp(Property prop)
		{
			if (prop == null || !prop.IsVaild)
				return;

			if (m_Props == null)
				m_Props = new Propertys ();
			m_Props.AddProp (prop);
		}

		public Propertys Props
		{
			get {
				return m_Props;
			}

			internal set {
				m_Props = value;
			}
		}

		private Propertys m_Props = null;
	}

	public class ObjectGroup
	{
		public ObjectGroup(string name, int width, int height)
		{
			Name = name;
			Width = width;
			Height = height;
		}

		public bool IsVaild
		{
			get
			{
				return !string.IsNullOrEmpty (Name);
			}
		}

		public string Name
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public int LayerCount
		{
			get {
				if (m_LayerList == null)
					return 0;
				return m_LayerList.Count;
			}
		}

		public void AddLayer(ObjectLayer layer)
		{
			if (layer == null)
				return;

			if (m_LayerList == null) {
				m_LayerList = new List<ObjectLayer> ();
			}

			m_LayerList.Add (layer);
		}

		public ObjectLayer GetLayer(string name)
		{
			if (string.IsNullOrEmpty (name) || m_LayerList == null)
				return null;
			
			for (int i = 0; i < m_LayerList.Count; ++i) {
				ObjectLayer layer = m_LayerList [i];
				if (layer == null)
					continue;
				if (string.Compare (layer.Name, name) == 0)
					return layer;
			}
			return null;
		}

		private IList<ObjectLayer> m_LayerList = null;
	}
}
