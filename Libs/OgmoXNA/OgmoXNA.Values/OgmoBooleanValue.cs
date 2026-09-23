using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor boolean value.
	/// </summary>
	public class OgmoBooleanValue : OgmoValue<bool>
	{
		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoBooleanValue" />.
		/// </summary>
		public OgmoBooleanValue()
		{
		}

		internal OgmoBooleanValue(ContentReader reader)
			: base(reader)
		{
			base.Value = reader.ReadBoolean();
		}
	}
}
