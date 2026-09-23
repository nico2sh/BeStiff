using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	internal static class OgmoValueReader
	{
		internal static OgmoValue Read(ContentReader reader)
		{
			OgmoValue result = null;
			switch (reader.ReadString())
			{
			case "b":
				result = new OgmoBooleanValue(reader);
				break;
			case "i":
				result = new OgmoIntegerValue(reader);
				break;
			case "n":
				result = new OgmoNumberValue(reader);
				break;
			case "s":
				result = new OgmoStringValue(reader);
				break;
			}
			return result;
		}
	}
}
