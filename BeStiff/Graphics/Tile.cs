using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA.Layers;

namespace Be_Stiff.Graphics
{
	internal class Tile
	{
		public bool Empty { get; set; }

		public Vector2 Position { get; set; }

		public Rectangle Source { get; set; }

		public Texture2D Texture { get; set; }

		public Color Tint { get; set; }

		public Tile(OgmoTile tile, bool useSourceIndex)
		{
			Tint = Color.White;
			Position = new Vector2((int)tile.Position.X, (int)tile.Position.Y);
			Texture = tile.Tileset.Texture;
			if (useSourceIndex)
			{
				Source = tile.Tileset.Sources[tile.SourceIndex];
			}
			else
			{
				Source = new Rectangle(tile.TextureOffset.X, tile.TextureOffset.Y, tile.Tileset.TileWidth, tile.Tileset.TileHeight);
			}
			Empty = false;
		}

		public Tile(Vector2 position)
		{
			Position = position;
			Texture = null;
			Source = new Rectangle(0, 0, 0, 0);
			Tint = Color.White;
			Empty = true;
		}
	}
}
