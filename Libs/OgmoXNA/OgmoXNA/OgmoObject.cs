using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA.Values;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data for an Ogmo Editor object.
	/// </summary>
	public class OgmoObject
	{
		private List<OgmoNode> nodes = new List<OgmoNode>();

		private Dictionary<string, OgmoValue> values = new Dictionary<string, OgmoValue>();

		/// <summary>
		/// Gets the height (in pixels) of the object.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets whether the object's texture should be tiled or stretched if its source rectangle is larger 
		/// than its texture.
		/// </summary>
		public bool IsTiled { get; private set; }

		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the object's nodes.
		/// </summary>
		public OgmoNode[] Nodes => nodes.ToArray();

		/// <summary>
		/// Gets the origin of the object.
		/// </summary>
		public Vector2 Origin { get; private set; }

		/// <summary>
		/// Gets the position of the object.
		/// </summary>
		public Vector2 Position { get; private set; }

		/// <summary>
		/// Gets the rotation angle of the object.
		/// </summary>
		public float Rotation { get; private set; }

		/// <summary>
		/// Gets the source rectangle of the object.
		/// </summary>
		public Rectangle Source { get; private set; }

		/// <summary>
		/// Gets the texture of the object.
		/// </summary>
		public Texture2D Texture { get; private set; }

		/// <summary>
		/// Gets the object's values.
		/// </summary>
		public OgmoValue[] Values => values.Values.ToArray();

		/// <summary>
		/// Gets the width (in pixels) of the object.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Object templates whose size was NOT doubled when the project moved
		/// to its larger grid; every other template was.
		/// </summary>
		private static readonly string[] UnscaledTemplates = { "GirderSmall", "Skyline", "Clouds", "Cushions", "Sector" };

		/// <summary>
		/// Legacy levels store the current (doubled) template size for
		/// fixed-size objects; bring such dimensions back to the old scale so
		/// physics built from them matches the old world.
		/// </summary>
		internal void ShrinkLegacyTemplate(int factor, OgmoObjectTemplate template)
		{
			if (template == null || System.Array.IndexOf(UnscaledTemplates, Name) >= 0)
			{
				return;
			}
			bool shrinkX = !template.IsResizableX && Width == template.Width;
			bool shrinkY = !template.IsResizableY && Height == template.Height;
			if (shrinkX)
			{
				Width /= factor;
			}
			if (shrinkY)
			{
				Height /= factor;
			}
			if (shrinkX || shrinkY)
			{
				Origin = new Vector2(shrinkX ? Origin.X / factor : Origin.X, shrinkY ? Origin.Y / factor : Origin.Y);
				Rectangle source = Source;
				source.Width = Width;
				source.Height = Height;
				Source = source;
			}
		}

		internal OgmoObject(ContentReader reader)
		{
			Name = reader.ReadString();
			Origin = reader.ReadVector2();
			Position = reader.ReadVector2();
			Rotation = reader.ReadSingle();
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			Rectangle empty = Rectangle.Empty;
			empty.X = reader.ReadInt32();
			empty.Y = reader.ReadInt32();
			empty.Width = reader.ReadInt32();
			empty.Height = reader.ReadInt32();
			Source = empty;
			IsTiled = reader.ReadBoolean();
			Texture = reader.ReadExternalReference<Texture2D>();
			int num = reader.ReadInt32();
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					OgmoValue ogmoValue = OgmoValueReader.Read(reader);
					if (ogmoValue != null)
					{
						values.Add(ogmoValue.Name, ogmoValue);
					}
				}
			}
			int num2 = reader.ReadInt32();
			if (num2 > 0)
			{
				for (int j = 0; j < num2; j++)
				{
					nodes.Add(new OgmoNode(reader));
				}
			}
		}

		/// <summary>
		/// Gets the specified value.
		/// </summary>
		/// <typeparam name="T">The type of value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <returns>Returns the specified value if it exists; otherwise <c>null</c>.</returns>
		public T GetValue<T>(string name) where T : OgmoValue
		{
			OgmoValue value = null;
			if (values.TryGetValue(name, out value))
			{
				return value as T;
			}
			return null;
		}
	}
}
