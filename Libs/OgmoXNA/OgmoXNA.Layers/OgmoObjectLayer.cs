using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers
{
	/// <summary>
	/// Contains data about an Ogmo Editor object layer.
	/// </summary>
	public sealed class OgmoObjectLayer : OgmoLayer
	{
		private Dictionary<string, List<OgmoObject>> objects = new Dictionary<string, List<OgmoObject>>();

		private List<OgmoObject> allObjects = new List<OgmoObject>();

		/// <summary>
		/// Gets all of the layer's objects.
		/// </summary>
		public OgmoObject[] Objects => allObjects.ToArray();

		internal void ShrinkLegacyTemplates(int factor, OgmoProject project)
		{
			foreach (OgmoObject obj in allObjects)
			{
				obj.ShrinkLegacyTemplate(factor, project.GetObjectTemplate(obj.Name));
			}
		}

		internal void ScaleLegacy(int factor, OgmoProject project)
		{
			foreach (OgmoObject obj in allObjects)
			{
				obj.ScaleLegacy(factor, project.GetObjectTemplate(obj.Name));
			}
		}

		internal OgmoObjectLayer(ContentReader reader, OgmoLevel level)
			: base(reader)
		{
			int num = reader.ReadInt32();
			if (num <= 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				OgmoObject ogmoObject = new OgmoObject(reader);
				if (ogmoObject != null)
				{
					if (objects.ContainsKey(ogmoObject.Name))
					{
						objects[ogmoObject.Name].Add(ogmoObject);
					}
					else
					{
						objects.Add(ogmoObject.Name, new List<OgmoObject> { ogmoObject });
					}
				}
				allObjects.Add(ogmoObject);
			}
		}

		/// <summary>
		/// Gets the first found object with the specified name.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <returns>Returns the first found object with the specified name; otherwise, <c>null</c>.</returns>
		public OgmoObject GetObject(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			List<OgmoObject> value = null;
			if (objects.TryGetValue(name, out value))
			{
				return value.First((OgmoObject x) => x.Name.Equals(name));
			}
			return null;
		}

		/// <summary>
		/// Gets all objects with the specified name.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <returns>Returns an array of objects with the specified name.</returns>
		public OgmoObject[] GetObjects(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			List<OgmoObject> value = null;
			if (objects.TryGetValue(name, out value))
			{
				return value.ToArray();
			}
			return null;
		}
	}
}
