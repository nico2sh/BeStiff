using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor string value object.
	/// </summary>
	public class OgmoStringValueTemplate : OgmoValueTemplate<string>
	{
		/// <summary>
		/// Gets the max number of chars in the string value.
		/// </summary>
		public int MaxChars { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoStringValue" />.
		/// </summary>
		public OgmoStringValueTemplate()
		{
		}

		internal OgmoStringValueTemplate(ContentReader reader)
			: base(reader)
		{
			base.Default = reader.ReadString();
			MaxChars = reader.ReadInt32();
		}
	}
}
