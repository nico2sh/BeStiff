using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	/// <summary>
	/// Output for ComputeDistance.
	/// </summary>
	public struct DistanceOutput
	{
		public float Distance;

		/// <summary>
		/// Number of GJK iterations used
		/// </summary>
		public int Iterations;

		/// <summary>
		/// Closest point on shapeA
		/// </summary>
		public Vector2 PointA;

		/// <summary>
		/// Closest point on shapeB
		/// </summary>
		public Vector2 PointB;
	}
}
