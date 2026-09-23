using Microsoft.Xna.Framework.Content;

namespace OgmoXNA.Values
{
	/// <summary>
	/// Contains data for an Ogmo Editor value object.
	/// </summary>
	public abstract class OgmoValueTemplate
	{
		/// <summary>
		/// Gets the name of the value.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoValue" />.
		/// </summary>
		protected OgmoValueTemplate()
		{
		}

		internal OgmoValueTemplate(ContentReader reader)
		{
			Name = reader.ReadString();
		}
	}
	/// <summary>
	/// Contains strongly-typed data for an Ogmo Editor value object.
	/// </summary>
	/// <typeparam name="T">The value type of the object.</typeparam>
	public abstract class OgmoValueTemplate<T> : OgmoValueTemplate
	{
		/// <summary>
		/// Gets the default value of the object.
		/// </summary>
		public T Default { get; protected set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.Values.OgmoValue" />.
		/// </summary>
		protected OgmoValueTemplate()
		{
		}

		internal OgmoValueTemplate(ContentReader reader)
			: base(reader)
		{
		}
	}
}
