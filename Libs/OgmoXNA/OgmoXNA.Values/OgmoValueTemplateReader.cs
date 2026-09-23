using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	internal static class OgmoValueTemplateReader
	{
		internal static OgmoValueTemplate Read(ContentReader reader)
		{
			OgmoValueTemplate result = null;
			switch (reader.ReadString())
			{
			case "b":
				result = new OgmoBooleanValueTemplate(reader);
				break;
			case "i":
				result = new OgmoIntegerValueTemplate(reader);
				break;
			case "n":
				result = new OgmoNumberValueTemplate(reader);
				break;
			case "s":
				result = new OgmoStringValueTemplate(reader);
				break;
			}
			return result;
		}
	}
}
