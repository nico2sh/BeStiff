using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Layers;

namespace Be_Stiff.Graphics
{
	internal class TileGrid
	{
		private Dictionary<OgmoTileset, Tile[,]> tiles;

		private int width;

		private int height;

		public TileGrid(OgmoTileLayer tileLayer, int levelWidth, int levelHeight)
		{
			height = levelHeight;
			width = levelWidth;
			tiles = new Dictionary<OgmoTileset, Tile[,]>();
			OgmoTile[] array = tileLayer.Tiles;
			foreach (OgmoTile ogmoTile in array)
			{
				if (tiles.ContainsKey(ogmoTile.Tileset))
				{
					int num = (int)ogmoTile.Position.X / ogmoTile.Tileset.TileWidth;
					int num2 = (int)ogmoTile.Position.Y / ogmoTile.Tileset.TileHeight;
					if (num < tiles[ogmoTile.Tileset].GetLength(0) && num2 < tiles[ogmoTile.Tileset].GetLength(1))
					{
						tiles[ogmoTile.Tileset][num, num2] = new Tile(ogmoTile, useSourceIndex: true);
					}
					continue;
				}
				CreateEmptyTileSet(ogmoTile.Tileset);
				int num3 = (int)ogmoTile.Position.X / ogmoTile.Tileset.TileWidth;
				int num4 = (int)ogmoTile.Position.Y / ogmoTile.Tileset.TileHeight;
				if (num3 < tiles[ogmoTile.Tileset].GetLength(0) && num4 < tiles[ogmoTile.Tileset].GetLength(1))
				{
					tiles[ogmoTile.Tileset][num3, num4] = new Tile(ogmoTile, useSourceIndex: true);
				}
			}
		}

		public void CreateEmptyTileSet(OgmoTileset tileSet)
		{
			int num = width / tileSet.TileWidth + 1;
			int num2 = height / tileSet.TileHeight + 1;
			Tile[,] array = new Tile[num, num2];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					Vector2 position = new Vector2(i * tileSet.TileWidth, j * tileSet.TileHeight);
					Tile tile = new Tile(position);
					array[i, j] = tile;
				}
			}
			tiles.Add(tileSet, array);
		}

		public void Draw()
		{
			Vector2 position = GameElementsControl.Camera.Position;
			Vector2 vector = new Vector2(GameElementsControl.Camera.CurrentSize.X / 2f, GameElementsControl.Camera.CurrentSize.Y / 2f);
			foreach (OgmoTileset key in tiles.Keys)
			{
				Tile[,] array = tiles[key];
				int num = Math.Max((int)((position.X - vector.X) / (float)key.TileWidth) - 1, 0);
				int num2 = Math.Max((int)((position.Y - vector.Y) / (float)key.TileHeight) - 1, 0);
				int num3 = (int)(Math.Min(position.X + vector.X, width) / (float)key.TileWidth) + 1;
				int num4 = (int)(Math.Min(position.Y + vector.Y, height) / (float)key.TileHeight) + 1;
				for (int i = num; i < num3; i++)
				{
					for (int j = num2; j < num4; j++)
					{
						if (!array[i, j].Empty)
						{
							GameElementsControl.ScreenManager.SpriteBatch.Draw(array[i, j].Texture, array[i, j].Position, array[i, j].Source, array[i, j].Tint);
						}
					}
				}
			}
		}
	}
}
