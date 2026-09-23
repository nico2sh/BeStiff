using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor string value object.
	/// </summary>
	public class OgmoStringValue : OgmoValue<string>
	{
		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoStringValue" />.
		/// </summary>
		public OgmoStringValue()
		{
		}

		internal OgmoStringValue(ContentReader reader)
			: base(reader)
		{
			base.Value = reader.ReadString();
		}
	}
}
