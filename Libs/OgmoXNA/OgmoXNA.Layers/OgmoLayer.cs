using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers
{
	/// <summary>
	/// Contains data about an Ogmo Editor layer.
	/// </summary>
	public abstract class OgmoLayer
	{
		/// <summary>
		/// Gets the name of the layer.
		/// </summary>
		public string Name { get; private set; }

		internal OgmoLayer(ContentReader reader)
		{
			Name = reader.ReadString();
		}
	}
}
