using Microsoft.Xna.Framework.Content;

namespace OgmoXNA
{
	/// <summary>
	/// Contains settings for an Ogmo Editor project.
	/// </summary>
	public class OgmoProjectSettings
	{
		/// <summary>
		/// Gets the default height (in pixels) of project levels.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the max height (in pixels) of project levels.
		/// </summary>
		public int MaxHeight { get; private set; }

		/// <summary>
		/// Gets the max width (in pixels) of project levels.
		/// </summary>
		public int MaxWidth { get; private set; }

		/// <summary>
		/// Gets the min height (in pixels) of project levels.
		/// </summary>
		public int MinHeight { get; private set; }

		/// <summary>
		/// Gets the min width (in pixels) of project levels.
		/// </summary>
		public int MinWidth { get; private set; }

		/// <summary>
		/// Gets the default width (in pixels) of project levels.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the working directory of project assets relative to the project file.
		/// </summary>
		public string WorkingDirectory { get; private set; }

		internal OgmoProjectSettings(ContentReader reader)
		{
			Height = reader.ReadInt32();
			MaxHeight = reader.ReadInt32();
			MaxWidth = reader.ReadInt32();
			MinHeight = reader.ReadInt32();
			MinWidth = reader.ReadInt32();
			Width = reader.ReadInt32();
			WorkingDirectory = reader.ReadString();
		}
	}
}
