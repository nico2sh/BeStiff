using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA.Layers;

namespace Be_Stiff.Graphics
{
	internal class TextureRectangles
	{
		private Dictionary<Rectangle, List<Rectangle>> rectangles;

		private Texture2D texture;

		public TextureRectangles(OgmoTileLayer tileLayer)
		{
			texture = tileLayer.Tilesets[0].Texture;
			rectangles = new Dictionary<Rectangle, List<Rectangle>>();
			OgmoTile[] tiles = tileLayer.Tiles;
			bool dump = DebugFlags.Dump;
			if (dump) System.Console.Error.WriteLine($"TileLayer {tileLayer.Name}: {tiles.Length} tiles, tileset tex {texture.Width}x{texture.Height}, sources={tileLayer.Tilesets[0].Sources.Count}");
			foreach (OgmoTile ogmoTile in tiles)
			{
				Rectangle key = ogmoTile.Tileset.Sources[ogmoTile.SourceIndex];
				if (dump) System.Console.Error.WriteLine($"  tile pos={ogmoTile.Position} size={ogmoTile.Width}x{ogmoTile.Height} srcIdx={ogmoTile.SourceIndex} src={key}");
				if (rectangles.TryGetValue(key, out var value))
				{
					if (0 == 0)
					{
						value.Add(new Rectangle((int)ogmoTile.Position.X, (int)ogmoTile.Position.Y, ogmoTile.Width, ogmoTile.Height));
					}
				}
				else
				{
					value = new List<Rectangle>
					{
						new Rectangle((int)ogmoTile.Position.X, (int)ogmoTile.Position.Y, ogmoTile.Width, ogmoTile.Height)
					};
					rectangles.Add(key, value);
				}
			}
		}

		public void Draw()
		{
			Rectangle visible = GameElementsControl.Camera.VisibleArea(16);
			foreach (KeyValuePair<Rectangle, List<Rectangle>> pair in rectangles)
			{
				foreach (Rectangle item in pair.Value)
				{
					if (visible.Intersects(item))
					{
						GameElementsControl.ScreenManager.SpriteBatch.Draw(texture, item, pair.Key, Color.White);
					}
				}
			}
		}
	}
}
