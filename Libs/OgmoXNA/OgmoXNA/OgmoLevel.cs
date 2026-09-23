using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using OgmoXNA.Layers;
using OgmoXNA.Layers.Settings;
using OgmoXNA.Values;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data about an Ogmo Editor level.
	/// </summary>
	public class OgmoLevel
	{
		private Dictionary<string, OgmoLayer> layers = new Dictionary<string, OgmoLayer>();

		private Dictionary<string, OgmoValue> values = new Dictionary<string, OgmoValue>();

		/// <summary>
		/// Gets the height (in pixels) of the level.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the level's layers.
		/// </summary>
		public OgmoLayer[] Layers => layers.Values.ToArray();

		/// <summary>
		/// Gets the <see cref="T:OgmoXNA.OgmoProject" /> associated with the level.
		/// </summary>
		public OgmoProject Project { get; private set; }

		/// <summary>
		/// Gets the level's value objects.
		/// </summary>
		public OgmoValue[] Values => values.Values.ToArray();

		/// <summary>
		/// Gets the width (in pixels) of the level.
		/// </summary>
		public int Width { get; private set; }

		internal OgmoLevel(ContentReader reader)
		{
			Project = reader.ReadExternalReference<OgmoProject>();
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
			Height = reader.ReadInt32();
			Width = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			if (num2 <= 0)
			{
				return;
			}
			for (int j = 0; j < num2; j++)
			{
				OgmoLayer ogmoLayer = OgmoLayerReader.Read(reader, this);
				if (ogmoLayer != null)
				{
					layers.Add(ogmoLayer.Name, ogmoLayer);
				}
			}
			ApplyLegacyScale();
		}

		/// <summary>
		/// Grid ratio of a level authored against an older project whose grid
		/// was smaller than the current one (1 when the level is current).
		/// </summary>
		public int LegacyScale { get; private set; } = 1;

		/// <summary>
		/// Some shipped levels were exported with an older project whose grid
		/// (24 px) was half the current one (48 px). Such a level is detected
		/// by its collision bitmap not matching the project grid. The level
		/// keeps its pixel coordinates (only the characters grew in the newer
		/// project); tile layers placed on the finer grid are drawn with the
		/// smaller tile size so tiles do not overlap.
		/// </summary>
		private void ApplyLegacyScale()
		{
			int factor = 0;
			foreach (OgmoLayer layer in layers.Values)
			{
				if (layer is OgmoGridLayer grid && grid.EffectiveGridSize > 0)
				{
					OgmoGridLayerSettings settings = Project.GetLayerSettings<OgmoGridLayerSettings>(grid.Name);
					if (settings != null && settings.GridSize > grid.EffectiveGridSize && settings.GridSize % grid.EffectiveGridSize == 0)
					{
						factor = settings.GridSize / grid.EffectiveGridSize;
					}
				}
			}
			if (factor <= 1)
			{
				return;
			}
			System.Console.Error.WriteLine($"OgmoLevel: legacy level detected (grid ratio {factor}); tile layers on the fine grid use smaller tiles, object templates halved");
			LegacyScale = factor;
			foreach (OgmoLayer layer in layers.Values)
			{
				if (layer is OgmoObjectLayer objects)
				{
					objects.ShrinkLegacyTemplates(factor, Project);
				}
				if (layer is OgmoTileLayer tiles)
				{
					OgmoTileLayerSettings settings = Project.GetLayerSettings<OgmoTileLayerSettings>(tiles.Name);
					int projectGrid = settings != null ? settings.GridSize : 0;
					if (projectGrid > 0 && tiles.IsOnFinerGridThan(projectGrid))
					{
						tiles.ScaleLegacy(1f / factor);
					}
				}
			}
		}

		public T GetLayer<T>(string name) where T : OgmoLayer
		{
			OgmoLayer value = null;
			if (layers.TryGetValue(name, out value))
			{
				return value as T;
			}
			return null;
		}

		/// <summary>
		/// Gets the specified level value.
		/// </summary>
		/// <typeparam name="T">The type of value to retrieve.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <returns>Returns the specified value if found; otherwise, <c>null</c>.</returns>
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
