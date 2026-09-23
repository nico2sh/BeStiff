using Microsoft.Xna.Framework.Content;

namespace OgmoXNA
{
	/// <summary>
	/// Reads <see cref="T:OgmoXNA.OgmoLevel" /> objects from the content pipeline.
	/// </summary>
	public class OgmoLevelReader : ContentTypeReader<OgmoLevel>
	{
		/// <summary>
		/// Reads binary data into an <see cref="T:OgmoXNA.OgmoLevel" /> object.
		/// </summary>
		/// <param name="input">The <see cref="T:Microsoft.Xna.Framework.Content.ContentReader" /> to read from.</param>
		/// <param name="existingInstance">An existing instance of <see cref="T:OgmoXNA.OgmoLevel" />.</param>
		/// <returns>Returns a configured <see cref="T:OgmoXNA.OgmoLevel" /> object.</returns>
		protected override OgmoLevel Read(ContentReader input, OgmoLevel existingInstance)
		{
			return new OgmoLevel(input);
		}
	}
}
