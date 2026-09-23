using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers.Settings
{
	internal static class OgmoLayerSettingsReader
	{
		internal static OgmoLayerSettings Read(ContentReader reader)
		{
			OgmoLayerSettings result = null;
			switch (reader.ReadString())
			{
			case "g":
				result = new OgmoGridLayerSettings(reader);
				break;
			case "o":
				result = new OgmoObjectLayerSettings(reader);
				break;
			case "t":
				result = new OgmoTileLayerSettings(reader);
				break;
			}
			return result;
		}
	}
}
