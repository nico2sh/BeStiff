using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// Called for each fixture found in the query. You control how the ray cast
	/// proceeds by returning a float:
	/// <returns>-1 to filter, 0 to terminate, fraction to clip the ray for closest hit, 1 to continue</returns>
	/// </summary>
	public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);
}
