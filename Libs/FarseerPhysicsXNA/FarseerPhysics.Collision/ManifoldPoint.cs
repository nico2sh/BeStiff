using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	/// <summary>
	/// A manifold point is a contact point belonging to a contact
	/// manifold. It holds details related to the geometry and dynamics
	/// of the contact points.
	/// The local point usage depends on the manifold type:
	/// -ShapeType.Circles: the local center of circleB
	/// -SeparationFunction.FaceA: the local center of cirlceB or the clip point of polygonB
	/// -SeparationFunction.FaceB: the clip point of polygonA
	/// This structure is stored across time steps, so we keep it small.
	/// Note: the impulses are used for internal caching and may not
	/// provide reliable contact forces, especially for high speed collisions.
	/// </summary>
	public struct ManifoldPoint
	{
		/// <summary>
		/// Uniquely identifies a contact point between two Shapes
		/// </summary>
		public ContactID Id;

		public Vector2 LocalPoint;

		public float NormalImpulse;

		public float TangentImpulse;
	}
}
