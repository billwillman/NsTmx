using System;
using System.Collections;
using System.Collections.Generic;

namespace TmxCSharp.Models
{
	public class Property
	{
		public Property(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public string Name
		{
			get;
			private set;
		}

		public string Value
		{
			get;
			private set;
		}

		public bool IsVaild
		{
			get
			{
				return (!string.IsNullOrEmpty (Name));
			}
		}
	}

	public class Propertys
	{
		public int PropCount
		{
			get {
				if (m_PropMap == null)
					return 0;
				return m_PropMap.Count;
			}
		}

		public Dictionary<string, Property>.Enumerator GetPropIter()
		{
			if (m_PropMap == null)
			{
				return new Dictionary<string, Property>.Enumerator();
			}

			return m_PropMap.GetEnumerator();
		}

		public void AddProp(Property prop)
		{
			if (prop == null || !prop.IsVaild)
				return;

			if (m_PropMap == null) {
				m_PropMap = new Dictionary<string, Property> ();
				m_PropMap.Add (prop.Name, prop);
			} else {
				if (m_PropMap.ContainsKey (prop.Name))
					m_PropMap [prop.Name] = prop;
				else
					m_PropMap.Add (prop.Name, prop);
			}
		}

		public Property GetProp(string name)
		{
			if (string.IsNullOrEmpty (name) || m_PropMap == null)
				return null;

			Property ret;
			if (m_PropMap.TryGetValue (name, out ret))
				return ret;
			return null;
		}

		public bool HasProp
		{
			get {
				return m_PropMap != null && m_PropMap.Count > 0;	
			}
		}

		private Dictionary<string, Property> m_PropMap = null;
	}
}