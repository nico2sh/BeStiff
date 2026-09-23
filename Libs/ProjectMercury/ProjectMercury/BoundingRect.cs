using Microsoft.Xna.Framework;

namespace ProjectMercury
{
	/// <summary>
	/// Defines a a bounding rectangle (similar to an XNA BoundingBox but in two dimensions).
	/// </summary>
	public struct BoundingRect
	{
		/// <summary>
		/// Gets or sets the minimum point in the rectangle.
		/// </summary>
		public Vector2 Min;

		/// <summary>
		/// Gets or sets the maximum point in the rectangle.
		/// </summary>
		public Vector2 Max;

		/// <summary>
		/// Gets the top of the rectangle.
		/// </summary>
		public float Top => Min.Y;

		/// <summary>
		/// Gets the left position of the rectangle.
		/// </summary>
		public float Left => Min.X;

		/// <summary>
		/// Gets the right position of the rectangle.
		/// </summary>
		public float Right => Max.X;

		/// <summary>
		/// Gets the bottom position of the rectangle.
		/// </summary>
		public float Bottom => Max.Y;

		/// <summary>
		/// Gets the width of the rectangle.
		/// </summary>
		public float Width => Max.X - Min.X;

		/// <summary>
		/// Gets the height of the rectangle.
		/// </summary>
		public float Height => Max.Y - Min.Y;

		/// <summary>
		/// Gets the centre point of the rectangle.
		/// </summary>
		public Vector2 Centre => new Vector2
		{
			X = Width / 2f,
			Y = Height / 2f
		};

		/// <summary>
		/// Gets a 3 dimensional bounding box for the rectangle.
		/// </summary>
		/// <param name="z">The minimum position of the rectangle on the z axis.</param>
		/// <param name="depth">The required depth of the bounding box.</param>
		/// <returns>A bounding box containing the bounding rect, with the specified Z axis position and depth.</returns>
		public BoundingBox ToBoundingBox(float z, float depth)
		{
			return new BoundingBox
			{
				Min = new Vector3(Min, z),
				Max = new Vector3(Max, z + depth)
			};
		}
	}
}
