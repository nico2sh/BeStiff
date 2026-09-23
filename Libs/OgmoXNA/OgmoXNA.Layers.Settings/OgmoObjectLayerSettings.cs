using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers.Settings
{
	/// <summary>
	/// Contains settings for an Ogmo Editor object layer.
	/// </summary>
	public sealed class OgmoObjectLayerSettings : OgmoLayerSettings
	{
		internal OgmoObjectLayerSettings(ContentReader reader)
			: base(reader)
		{
		}
	}
}
