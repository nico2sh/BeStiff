using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	/// <summary>
	/// Ray-cast output data.  The ray hits at p1 + fraction * (p2 - p1), where p1 and p2
	/// come from RayCastInput. 
	/// </summary>
	public struct RayCastOutput
	{
		public float Fraction;

		public Vector2 Normal;
	}
}
