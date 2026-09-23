using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	/// <summary>
	/// Ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
	/// </summary>
	public struct RayCastInput
	{
		public float MaxFraction;

		public Vector2 Point1;

		public Vector2 Point2;
	}
}
