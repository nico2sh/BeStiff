using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	/// <summary>
	/// This lets us treate and edge shape and a polygon in the same
	/// way in the SAT collider.
	/// </summary>
	public class EPProxy
	{
		public Vector2 Centroid;

		public int Count;

		public Vector2[] Normals = new Vector2[Settings.MaxPolygonVertices];

		public Vector2[] Vertices = new Vector2[Settings.MaxPolygonVertices];
	}
}
