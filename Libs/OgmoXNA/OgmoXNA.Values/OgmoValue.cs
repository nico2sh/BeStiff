using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor value object.
	/// </summary>
	public abstract class OgmoValue
	{
		/// <summary>
		/// Gets the name of the value.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoValue" />.
		/// </summary>
		protected OgmoValue()
		{
		}

		internal OgmoValue(ContentReader reader)
		{
			Name = reader.ReadString();
		}
	}
	/// <summary>
	/// Contains strongly-typed data for an Ogmo Editor value object.
	/// </summary>
	/// <typeparam name="T">The value type of the object.</typeparam>
	public abstract class OgmoValue<T> : OgmoValue
	{
		/// <summary>
		/// Gets the current value.
		/// </summary>
		public T Value { get; protected set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoValue" />.
		/// </summary>
		protected OgmoValue()
		{
		}

		internal OgmoValue(ContentReader reader)
			: base(reader)
		{
		}
	}
}
