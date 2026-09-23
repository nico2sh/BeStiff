using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data about an Ogmo Editor object node.
	/// </summary>
	public class OgmoNode
	{
		/// <summary>
		/// Gets or sets the position of the node.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="T:OgmoXNA.OgmoNode" />.
		/// </summary>
		/// <param name="position">The position of the node.</param>
		public OgmoNode(Vector2 position)
		{
			Position = position;
		}

		internal OgmoNode(ContentReader reader)
		{
			Position = reader.ReadVector2();
		}
	}
}
