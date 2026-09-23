using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using OgmoXNA.Layers.Settings;

namespace OgmoXNA.Layers
{
	/// <summary>
	/// Contains data about an Ogmo Editor grid layer.
	/// </summary>
	public sealed class OgmoGridLayer : OgmoLayer
	{
		private int[,] rawData;

		private List<Rectangle> rectData;

		/// <summary>
		/// Gets the raw grid data for the layer.  This property is only populated when 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoGridLayerSettings.ExportAsObjects" /> is <c>false</c>; otherwise, it returns <c>null</c>.
		/// </summary>
		public int[,] RawData => rawData;

		/// <summary>Grid cell size in pixels actually used to decode RawData.</summary>
		public int EffectiveGridSize { get; private set; }

		/// <summary>
		/// Gets the rectangle data for the layer.  This propery is only populated when 
		/// <see cref="P:OgmoXNA.Layers.Settings.OgmoGridLayerSettings.ExportAsObjects" /> is <c>true</c>; otherwise, it returns <c>null</c>.
		/// </summary>
		public Rectangle[] RectangleData => rectData.ToArray();

		internal OgmoGridLayer(ContentReader reader, OgmoLevel level)
			: base(reader)
		{
			OgmoGridLayerSettings layerSettings = level.Project.GetLayerSettings<OgmoGridLayerSettings>(base.Name);
			if (layerSettings.ExportAsObjects)
			{
				rectData = new List<Rectangle>();
				int num = reader.ReadInt32();
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						Rectangle empty = Rectangle.Empty;
						empty.X = reader.ReadInt32();
						empty.Y = reader.ReadInt32();
						empty.Width = reader.ReadInt32();
						empty.Height = reader.ReadInt32();
						rectData.Add(empty);
					}
				}
				return;
			}
			byte[] array = Convert.FromBase64String(reader.ReadString());
			string text = Encoding.UTF8.GetString(array, 0, array.Length);
			if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null)
			{
				Console.Error.WriteLine($"GridRaw {base.Name} level={level.Width}x{level.Height} gridSize={layerSettings.GridSize} len={text.Length}");
				Console.Error.WriteLine("GridRawData " + text.Replace("\n", "|").Replace("\r", ""));
			}
			int gridSize = layerSettings.GridSize;
			// Some levels were exported with a different grid size than the
			// project file declares (e.g. Tutorial2 uses 24 px while the project
			// says 48). Infer the real grid size from the bitmap length so the
			// collision data is not sliced into the wrong shape.
			if (text.Length != (level.Width / gridSize) * (level.Height / gridSize))
			{
				foreach (int candidate in new[] { 8, 12, 16, 24, 32, 48, 64, 96 })
				{
					if (text.Length == (level.Width / candidate) * (level.Height / candidate))
					{
						Console.Error.WriteLine($"OgmoGridLayer '{base.Name}': project grid size {gridSize} does not match data, using {candidate}");
						gridSize = candidate;
						break;
					}
				}
			}
			EffectiveGridSize = gridSize;
			int num2 = level.Width / gridSize;
			int num3 = level.Height / gridSize;
			rawData = new int[num2, num3];
			for (int j = 0; j < num3; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					rawData[k, j] = int.Parse(text[j * num2 + k].ToString(), CultureInfo.InvariantCulture);
				}
			}
		}
	}
}
