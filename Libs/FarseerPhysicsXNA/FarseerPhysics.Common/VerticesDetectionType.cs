namespace FarseerPhysics.Common
{
	/// <summary>
	/// The detection type affects the resulting polygon data.
	/// </summary>
	public enum VerticesDetectionType
	{
		/// <summary>
		/// Holes are integrated into the main polygon.
		/// </summary>
		Integrated,
		/// <summary>
		/// The data of the main polygon and hole polygons is returned separately.
		/// </summary>
		Separated
	}
}
