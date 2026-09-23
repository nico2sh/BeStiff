using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data about Ogmo Editor number (float) value objects.
	/// </summary>
	public class OgmoNumberValueTemplate : OgmoValueTemplate<float>
	{
		/// <summary>
		/// Gets the max number (float) value.
		/// </summary>
		public float Max { get; private set; }

		/// <summary>
		/// Gets the min number (float) value.
		/// </summary>
		public float Min { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoNumberValue" />.
		/// </summary>
		public OgmoNumberValueTemplate()
		{
		}

		internal OgmoNumberValueTemplate(ContentReader reader)
			: base(reader)
		{
			base.Default = reader.ReadSingle();
			Max = reader.ReadSingle();
			Min = reader.ReadSingle();
		}
	}
}
