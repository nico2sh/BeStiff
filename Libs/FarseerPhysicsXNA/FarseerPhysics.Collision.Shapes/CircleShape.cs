using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public class CircleShape : Shape
	{
		internal Vector2 _position;

		public override int ChildCount => 1;

		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				ComputeProperties();
			}
		}

		public CircleShape(float radius, float density)
			: base(density)
		{
			base.ShapeType = ShapeType.Circle;
			_radius = radius;
			_position = Vector2.Zero;
			ComputeProperties();
		}

		internal CircleShape()
			: base(0f)
		{
			base.ShapeType = ShapeType.Circle;
			_radius = 0f;
			_position = Vector2.Zero;
		}

		public override Shape Clone()
		{
			CircleShape circleShape = new CircleShape();
			circleShape._radius = base.Radius;
			circleShape._density = _density;
			circleShape._position = _position;
			circleShape.ShapeType = base.ShapeType;
			circleShape.MassData = MassData;
			return circleShape;
		}

		/// <summary>
		/// Test a point for containment in this shape. This only works for convex shapes.
		/// </summary>
		/// <param name="transform">The shape world transform.</param>
		/// <param name="point">a point in world coordinates.</param>
		/// <returns>True if the point is inside the shape</returns>
		public override bool TestPoint(ref Transform transform, ref Vector2 point)
		{
			Vector2 vector = transform.Position + MathUtils.Multiply(ref transform.R, Position);
			Vector2 vector2 = point - vector;
			return Vector2.Dot(vector2, vector2) <= base.Radius * base.Radius;
		}

		/// <summary>
		/// Cast a ray against a child shape.
		/// </summary>
		/// <param name="output">The ray-cast results.</param>
		/// <param name="input">The ray-cast input parameters.</param>
		/// <param name="transform">The transform to be applied to the shape.</param>
		/// <param name="childIndex">The child shape index.</param>
		/// <returns>True if the ray-cast hits the shape</returns>
		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			Vector2 vector = transform.Position + MathUtils.Multiply(ref transform.R, Position);
			Vector2 vector2 = input.Point1 - vector;
			float num = Vector2.Dot(vector2, vector2) - base.Radius * base.Radius;
			Vector2 vector3 = input.Point2 - input.Point1;
			float num2 = Vector2.Dot(vector2, vector3);
			float num3 = Vector2.Dot(vector3, vector3);
			float num4 = num2 * num2 - num3 * num;
			if (num4 < 0f || num3 < 1.1920929E-07f)
			{
				return false;
			}
			float num5 = 0f - (num2 + (float)Math.Sqrt(num4));
			if (0f <= num5 && num5 <= input.MaxFraction * num3)
			{
				Vector2 normal = vector2 + (output.Fraction = num5 / num3) * vector3;
				normal.Normalize();
				output.Normal = normal;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Given a transform, compute the associated axis aligned bounding box for a child shape.
		/// </summary>
		/// <param name="aabb">The aabb results.</param>
		/// <param name="transform">The world transform of the shape.</param>
		/// <param name="childIndex">The child shape index.</param>
		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			Vector2 vector = transform.Position + MathUtils.Multiply(ref transform.R, Position);
			aabb.LowerBound = new Vector2(vector.X - base.Radius, vector.Y - base.Radius);
			aabb.UpperBound = new Vector2(vector.X + base.Radius, vector.Y + base.Radius);
		}

		/// <summary>
		/// Compute the mass properties of this shape using its dimensions and density.
		/// The inertia tensor is computed about the local origin, not the centroid.
		/// </summary>
		public sealed override void ComputeProperties()
		{
			float num = (float)Math.PI * base.Radius * base.Radius;
			MassData.Area = num;
			MassData.Mass = base.Density * num;
			MassData.Centroid = Position;
			MassData.Inertia = MassData.Mass * (0.5f * base.Radius * base.Radius + Vector2.Dot(Position, Position));
		}

		public bool CompareTo(CircleShape shape)
		{
			if (base.Radius == shape.Radius)
			{
				return Position == shape.Position;
			}
			return false;
		}

		public override float ComputeSubmergedArea(Vector2 normal, float offset, Transform xf, out Vector2 sc)
		{
			sc = Vector2.Zero;
			Vector2 vector = MathUtils.Multiply(ref xf, Position);
			float num = 0f - (Vector2.Dot(normal, vector) - offset);
			if (num < 0f - base.Radius + 1.1920929E-07f)
			{
				return 0f;
			}
			if (num > base.Radius)
			{
				sc = vector;
				return (float)Math.PI * base.Radius * base.Radius;
			}
			float num2 = base.Radius * base.Radius;
			float num3 = num * num;
			float num4 = num2 * (float)(Math.Asin(num / base.Radius) + 1.5707963705062866 + (double)num * Math.Sqrt(num2 - num3));
			float num5 = -2f / 3f * (float)Math.Pow(num2 - num3, 1.5) / num4;
			sc.X = vector.X + normal.X * num5;
			sc.Y = vector.Y + normal.Y * num5;
			return num4;
		}
	}
}
