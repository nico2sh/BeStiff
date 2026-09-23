using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers
{
	/// <summary>
	/// Contains data about an Ogmo Editor tile layer.
	/// </summary>
	public class OgmoTileLayer : OgmoLayer
	{
		private List<OgmoTile> tiles = new List<OgmoTile>();

		private Dictionary<string, OgmoTileset> tilesets = new Dictionary<string, OgmoTileset>();

		/// <summary>
		/// Gets the height (in pixels) of tiles in the layer.  Only valid if 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.ExportTileSize" /> is <c>true</c>, and 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.MultipleTilesets" /> is <c>false</c>.
		/// </summary>
		public int TileHeight { get; private set; }

		/// <summary>
		/// Gets the layer's tiles.
		/// </summary>
		public OgmoTile[] Tiles => tiles.ToArray();

		/// <summary>
		/// Gets the layer's tilesets.
		/// </summary>
		public OgmoTileset[] Tilesets => tilesets.Values.ToArray();

		/// <summary>
		/// Gets the width (in pixels) of tiles in the layer.  Only valid if 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.ExportTileSize" /> is <c>true</c>, and 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoTileLayerSettings.MultipleTilesets" /> is <c>false</c>.
		/// </summary>
		public int TileWidth { get; private set; }

		internal OgmoTileLayer(ContentReader reader, OgmoLevel level)
			: base(reader)
		{
			TileHeight = reader.ReadInt32();
			TileWidth = reader.ReadInt32();
			int num = reader.ReadInt32();
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					string name = reader.ReadString();
					OgmoTileset tileset = level.Project.GetTileset(name);
					tilesets.Add(tileset.Name, tileset);
				}
			}
			int num2 = reader.ReadInt32();
			if (num2 > 0)
			{
				for (int j = 0; j < num2; j++)
				{
					tiles.Add(new OgmoTile(reader, this));
				}
			}
		}

		/// <summary>
		/// Gets the specified tileset.
		/// </summary>
		/// <param name="name">The name of the tileset.</param>
		/// <returns>Returns the specified tileset if it exists; otherwise, <c>null</c>.</returns>
		/// <summary>True if any tile sits off the given grid (i.e. was placed on a finer one).</summary>
		internal bool IsOnFinerGridThan(int grid)
		{
			foreach (OgmoTile tile in tiles)
			{
				if ((int)tile.Position.X % grid != 0 || (int)tile.Position.Y % grid != 0)
				{
					return true;
				}
			}
			return false;
		}

		internal void ScaleLegacy(float sizeFactor)
		{
			TileWidth = (int)(TileWidth * sizeFactor);
			TileHeight = (int)(TileHeight * sizeFactor);
			foreach (OgmoTile tile in tiles)
			{
				tile.ScaleLegacy(sizeFactor);
			}
		}

		public OgmoTileset GetTileset(string name)
		{
			OgmoTileset value = null;
			if (tilesets.TryGetValue(name, out value))
			{
				return value;
			}
			return null;
		}
	}
}
