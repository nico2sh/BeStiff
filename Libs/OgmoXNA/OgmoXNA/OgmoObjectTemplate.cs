using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA.Values;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data about an Ogmo Editor object.
	/// </summary>
	public class OgmoObjectTemplate
	{
		private Dictionary<string, OgmoValueTemplate> values = new Dictionary<string, OgmoValueTemplate>();

		/// <summary>
		/// Gets the height (in pixels) of the object.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets whether the object can be resized horizontally.
		/// </summary>
		public bool IsResizableX { get; private set; }

		/// <summary>
		/// Gets whether the object can be resized vertically.
		/// </summary>
		public bool IsResizableY { get; private set; }

		/// <summary>
		/// Gets whether the object should tile its texture if resized along an axis.
		/// </summary>
		public bool IsTiled { get; private set; }

		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the origin of the object.
		/// </summary>
		public Vector2 Origin { get; private set; }

		/// <summary>
		/// Gets the texture source rectangle of the object.
		/// </summary>
		public Rectangle Source { get; private set; }

		/// <summary>
		/// Gets the filename of the object's texture.
		/// </summary>
		public string TextureFile { get; private set; }

		/// <summary>
		/// Gets the object's texture.
		/// </summary>
		public Texture2D Texture { get; private set; }

		/// <summary>
		/// Gets the object's values.
		/// </summary>
		public OgmoValueTemplate[] Values => values.Values.ToArray();

		/// <summary>
		/// Gets the width (in pixels) of the object.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.OgmoObjectTemplate" />.
		/// </summary>
		public OgmoObjectTemplate()
		{
		}

		internal OgmoObjectTemplate(ContentReader reader)
		{
			Height = reader.ReadInt32();
			IsResizableX = reader.ReadBoolean();
			IsResizableX = reader.ReadBoolean();
			IsTiled = reader.ReadBoolean();
			Name = reader.ReadString();
			Origin = reader.ReadVector2();
			Rectangle empty = Rectangle.Empty;
			empty.X = reader.ReadInt32();
			empty.Y = reader.ReadInt32();
			empty.Width = reader.ReadInt32();
			empty.Height = reader.ReadInt32();
			Source = empty;
			Texture = reader.ReadExternalReference<Texture2D>();
			TextureFile = reader.ReadString();
			Width = reader.ReadInt32();
			int num = reader.ReadInt32();
			if (num <= 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				OgmoValueTemplate ogmoValueTemplate = OgmoValueTemplateReader.Read(reader);
				if (ogmoValueTemplate != null)
				{
					values.Add(ogmoValueTemplate.Name, ogmoValueTemplate);
				}
			}
		}

		/// <summary>
		/// Gets the specified object value template.
		/// </summary>
		/// <typeparam name="T">The type of value template to retrieve.</typeparam>
		/// <param name="name">The name of the value template.</param>
		/// <returns>Returns the specified value template if found; otherwise, <c>null</c>.</returns>
		public T GetValueTemplate<T>(string name) where T : OgmoValueTemplate
		{
			OgmoValueTemplate value = null;
			if (values.TryGetValue(name, out value))
			{
				return value as T;
			}
			return null;
		}
	}
}
