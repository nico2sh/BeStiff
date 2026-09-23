namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Enumerates the origin options for a polygon shape.
	/// </summary>
	public enum PolygonOrigin : byte
	{
		/// <summary>
		/// No origin is specified, the translation vector will not be set.
		/// </summary>
		Default,
		/// <summary>
		/// The translation vector will be set to move the origin into the centre of the shape.
		/// </summary>
		Center,
		/// <summary>
		/// The translation vector will be set to move the origin to the first point in this shape.
		/// </summary>
		Origin
	}
}
