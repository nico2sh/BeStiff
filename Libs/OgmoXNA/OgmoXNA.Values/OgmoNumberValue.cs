using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor number (float) value object.
	/// </summary>
	public class OgmoNumberValue : OgmoValue<float>
	{
		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoNumberValue" />.
		/// </summary>
		internal void ScaleLegacy(int factor)
		{
			Value *= factor;
		}

		public OgmoNumberValue()
		{
		}

		internal OgmoNumberValue(ContentReader reader)
			: base(reader)
		{
			base.Value = reader.ReadSingle();
		}
	}
}
