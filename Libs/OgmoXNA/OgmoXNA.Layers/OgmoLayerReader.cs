using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Layers
{
	internal static class OgmoLayerReader
	{
		internal static OgmoLayer Read(ContentReader reader, OgmoLevel level)
		{
			OgmoLayer result = null;
			switch (reader.ReadString())
			{
			case "g":
				result = new OgmoGridLayer(reader, level);
				break;
			case "t":
				result = new OgmoTileLayer(reader, level);
				break;
			case "o":
				result = new OgmoObjectLayer(reader, level);
				break;
			}
			return result;
		}
	}
}
