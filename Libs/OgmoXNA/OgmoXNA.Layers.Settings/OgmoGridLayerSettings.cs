using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers.Settings
{
	/// <summary>
	/// Contains settings for an Ogmo Editor grid layer.
	/// </summary>
	public sealed class OgmoGridLayerSettings : OgmoLayerSettings
	{
		/// <summary>
		/// Gets whether the grid data should be exported as a series of rectangles (true) or as a 
		/// series of individual cells (false).
		/// </summary>
		public bool ExportAsObjects { get; private set; }

		/// <summary>
		/// Gets the string value that delimits cell data if <see cref="P:OgmoXNA.Layers.Settings.OgmoGridLayerSettings.ExportAsObjects" /> is false.
		/// </summary>
		public string NewLine { get; private set; }

		internal OgmoGridLayerSettings(ContentReader reader)
			: base(reader)
		{
			ExportAsObjects = reader.ReadBoolean();
			NewLine = reader.ReadString();
		}
	}
}
