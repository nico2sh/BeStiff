using Microsoft.Xna.Framework.Content;

namespace OgmoXNA
{
	/// <summary>
	/// Reads <see cref="T:OgmoXNA.OgmoProject" /> objects from the content pipeline.
	/// </summary>
	public class OgmoProjectReader : ContentTypeReader<OgmoProject>
	{
		/// <summary>
		/// Reads binary data into an <see cref="T:OgmoXNA.OgmoProject" /> object.
		/// </summary>
		/// <param name="input">The <see cref="T:Microsoft.Xna.Framework.Content.ContentReader" /> to read from.</param>
		/// <param name="existingInstance">An existing instance of <see cref="T:OgmoXNA.OgmoProject" />.</param>
		/// <returns>Returns a configured <see cref="T:OgmoXNA.OgmoLevel" /> object.</returns>
		protected override OgmoProject Read(ContentReader input, OgmoProject existingInstance)
		{
			return new OgmoProject(input);
		}
	}
}
