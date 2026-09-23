using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers.Settings
{
	/// <summary>
	/// Contains settings for an Ogmo Editor layer.
	/// </summary>
	public abstract class OgmoLayerSettings
	{
		/// <summary>
		/// Gets the color of the layer's grid.  Cosmetic only.
		/// </summary>
		public Color GridColor { get; private set; }

		/// <summary>
		/// Gets the cell size of the layer's grid when rendering.  Cosmetic only.
		/// </summary>
		public int GridDrawSize { get; private set; }

		/// <summary>
		/// Gets the cell size of the layer's grid.
		/// </summary>
		public int GridSize { get; private set; }

		/// <summary>
		/// Gets the name of the layer.
		/// </summary>
		public string Name { get; private set; }

		internal OgmoLayerSettings(ContentReader reader)
		{
			GridColor = reader.ReadColor();
			GridDrawSize = reader.ReadInt32();
			GridSize = reader.ReadInt32();
			Name = reader.ReadString();
		}
	}
}
