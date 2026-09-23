using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers
{
	/// <summary>
	/// Contains data about an Ogmo Editor tile.
	/// </summary>
	public sealed class OgmoTile
	{
		/// <summary>
		/// Gets the height (in pixels) of the tile.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the position of the tile in the layer.
		/// </summary>
		public Vector2 Position { get; private set; }

		/// <summary>
		/// Gets the <see cref="T:OgmoXNA.OgmoTileset" /> associated with the tile.
		/// </summary>
		public OgmoTileset Tileset { get; private set; }

		/// <summary>
		/// Gets the texture offset of the tile texture in its tileset.  Only valid if 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.ExportTileIDs" /> is <c>false</c>.
		/// </summary>
		public Point TextureOffset { get; private set; }

		/// <summary>
		/// Gets the rectangle source index of the tile texture in its tileset.  Only valid if 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.ExportTileIDs" /> is <c>true</c>.
		/// </summary>
		public int SourceIndex { get; private set; }

		/// <summary>
		/// Gets the width (in pixels) of the tile.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>Shrinks the drawn tile size (position unchanged).</summary>
		internal void ScaleLegacy(float sizeFactor)
		{
			Width = (int)(Width * sizeFactor);
			Height = (int)(Height * sizeFactor);
		}

		internal OgmoTile(ContentReader reader, OgmoTileLayer layer)
		{
			Height = reader.ReadInt32();
			Position = reader.ReadVector2();
			SourceIndex = reader.ReadInt32();
			Vector2 vector = reader.ReadVector2();
			Point textureOffset = new Point((int)vector.X, (int)vector.Y);
			TextureOffset = textureOffset;
			string name = reader.ReadString();
			Tileset = layer.GetTileset(name);
			Width = reader.ReadInt32();
		}
	}
}
