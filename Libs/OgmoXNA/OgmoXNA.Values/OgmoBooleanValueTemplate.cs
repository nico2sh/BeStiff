using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data about Ogmo Editor boolean value objects.
	/// </summary>
	public class OgmoBooleanValueTemplate : OgmoValueTemplate<bool>
	{
		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoBooleanValue" />.
		/// </summary>
		public OgmoBooleanValueTemplate()
		{
		}

		internal OgmoBooleanValueTemplate(ContentReader reader)
			: base(reader)
		{
			base.Default = reader.ReadBoolean();
		}
	}
}
