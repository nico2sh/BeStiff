using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor integer value object.
	/// </summary>
	public class OgmoIntegerValue : OgmoValue<int>
	{
		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoIntegerValue" />.
		/// </summary>
		public OgmoIntegerValue()
		{
		}

		internal OgmoIntegerValue(ContentReader reader)
			: base(reader)
		{
			base.Value = reader.ReadInt32();
		}
	}
}
