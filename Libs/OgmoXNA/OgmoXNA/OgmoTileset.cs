using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data about an Ogmo Editor tileset.
	/// </summary>
	public class OgmoTileset
	{
		private List<Rectangle> sources = new List<Rectangle>();

		/// <summary>
		/// Gets the name of the tileset.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the rectangle sources for the tileset texture.
		/// </summary>
		public List<Rectangle> Sources => sources;

		/// <summary>
		/// Gets the tileset texture.
		/// </summary>
		public Texture2D Texture { get; private set; }

		/// <summary>
		/// Gets the filename of the tileset texture, relative to 
		/// <see cref="P:OgmoXNA.OgmoProjectSettings.WorkingDirectory" />.
		/// </summary>
		public string TextureFile { get; private set; }

		/// <summary>
		/// Gets the height (in pixels) of each tile texture in the tileset.
		/// </summary>
		public int TileHeight { get; private set; }

		/// <summary>
		/// Gets the width (in pixels) of each tile texture in the tileset.
		/// </summary>
		public int TileWidth { get; private set; }

		internal OgmoTileset(ContentReader reader)
		{
			Name = reader.ReadString();
			Texture = reader.ReadExternalReference<Texture2D>();
			TextureFile = reader.ReadString();
			TileHeight = reader.ReadInt32();
			TileWidth = reader.ReadInt32();
			if (Texture != null)
			{
				CreateSources();
			}
		}

		internal void CreateSources()
		{
			for (int i = 0; i < Texture.Height; i += TileHeight)
			{
				for (int j = 0; j < Texture.Width; j += TileWidth)
				{
					sources.Add(new Rectangle(j, i, TileWidth, TileHeight));
				}
			}
		}
	}
}
