using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// A shape is used for collision detection. You can create a shape however you like.
	/// Shapes used for simulation in World are created automatically when a Fixture
	/// is created. Shapes may encapsulate a one or more child shapes.
	/// </summary>
	public abstract class Shape
	{
		private static int _shapeIdCounter;

		public MassData MassData;

		public int ShapeId;

		internal float _density;

		internal float _radius;

		/// <summary>
		/// Get the type of this shape.
		/// </summary>
		/// <value>The type of the shape.</value>
		public ShapeType ShapeType { get; internal set; }

		/// <summary>
		/// Get the number of child primitives.
		/// </summary>
		/// <value></value>
		public abstract int ChildCount { get; }

		/// <summary>
		/// Gets or sets the density.
		/// </summary>
		/// <value>The density.</value>
		public float Density
		{
			get
			{
				return _density;
			}
			set
			{
				_density = value;
				ComputeProperties();
			}
		}

		/// <summary>
		/// Radius of the Shape
		/// </summary>
		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				ComputeProperties();
			}
		}

		protected Shape(float density)
		{
			_density = density;
			ShapeType = ShapeType.Unknown;
			ShapeId = _shapeIdCounter++;
		}

		/// <summary>
		/// Clone the concrete shape
		/// </summary>
		/// <returns>A clone of the shape</returns>
		public abstract Shape Clone();

		/// <summary>
		/// Test a point for containment in this shape. This only works for convex shapes.
		/// </summary>
		/// <param name="transform">The shape world transform.</param>
		/// <param name="point">a point in world coordinates.</param>
		/// <returns>True if the point is inside the shape</returns>
		public abstract bool TestPoint(ref Transform transform, ref Vector2 point);

		/// <summary>
		/// Cast a ray against a child shape.
		/// </summary>
		/// <param name="output">The ray-cast results.</param>
		/// <param name="input">The ray-cast input parameters.</param>
		/// <param name="transform">The transform to be applied to the shape.</param>
		/// <param name="childIndex">The child shape index.</param>
		/// <returns>True if the ray-cast hits the shape</returns>
		public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex);

		/// <summary>
		/// Given a transform, compute the associated axis aligned bounding box for a child shape.
		/// </summary>
		/// <param name="aabb">The aabb results.</param>
		/// <param name="transform">The world transform of the shape.</param>
		/// <param name="childIndex">The child shape index.</param>
		public abstract void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex);

		/// <summary>
		/// Compute the mass properties of this shape using its dimensions and density.
		/// The inertia tensor is computed about the local origin, not the centroid.
		/// </summary>
		public abstract void ComputeProperties();

		public bool CompareTo(Shape shape)
		{
			if (shape is PolygonShape && this is PolygonShape)
			{
				return ((PolygonShape)this).CompareTo((PolygonShape)shape);
			}
			if (shape is CircleShape && this is CircleShape)
			{
				return ((CircleShape)this).CompareTo((CircleShape)shape);
			}
			if (shape is EdgeShape && this is EdgeShape)
			{
				return ((EdgeShape)this).CompareTo((EdgeShape)shape);
			}
			return false;
		}

		public abstract float ComputeSubmergedArea(Vector2 normal, float offset, Transform xf, out Vector2 sc);
	}
}
