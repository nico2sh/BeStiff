using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor integer value object.
	/// </summary>
	public class OgmoIntegerValueTemplate : OgmoValueTemplate<int>
	{
		/// <summary>
		/// Gets the max integer value.
		/// </summary>
		public int Max { get; private set; }

		/// <summary>
		/// Gets the min integer value.
		/// </summary>
		public int Min { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoIntegerValue" />.
		/// </summary>
		public OgmoIntegerValueTemplate()
		{
		}

		internal OgmoIntegerValueTemplate(ContentReader reader)
			: base(reader)
		{
			base.Default = reader.ReadInt32();
			Max = reader.ReadInt32();
			Min = reader.ReadInt32();
		}
	}
}
