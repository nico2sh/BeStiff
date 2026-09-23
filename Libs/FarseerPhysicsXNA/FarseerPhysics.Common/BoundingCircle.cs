using System;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public struct BoundingCircle : IEquatable<BoundingCircle>
	{
		public Vector2 Position;

		public float Radius;

		public float Area => (float)Math.PI * Radius * Radius;

		public float Perimeter => (float)Math.PI * 2f * Radius;

		public static BoundingCircle FromRectangle(AABB rect)
		{
			FromRectangle(ref rect, out var result);
			return result;
		}

		public static void FromRectangle(ref AABB rect, out BoundingCircle result)
		{
			result.Position = rect.Center;
			float num = (rect.UpperBound.X - rect.LowerBound.X) * 0.5f;
			float num2 = (rect.UpperBound.Y - rect.LowerBound.Y) * 0.5f;
			result.Radius = (float)Math.Sqrt(num * num + num2 * num2);
		}

		public static BoundingCircle FromVectors(Vertices vertices)
		{
			FromVectors(vertices, out var result);
			return result;
		}

		public static void FromVectors(Vertices vertices, out BoundingCircle result)
		{
			result.Position = vertices.GetCentroid();
			result.Radius = -1f;
			for (int i = 0; i < vertices.Count; i++)
			{
				float num = Vector2.DistanceSquared(result.Position, vertices[i]);
				if (result.Radius == -1f || num < result.Radius)
				{
					result.Radius = num;
				}
			}
			result.Radius = (float)Math.Sqrt(result.Radius);
		}

		public BoundingCircle(Vector2 position, float radius)
		{
			Position = position;
			Radius = radius;
		}

		public BoundingCircle(float x, float y, float radius)
		{
			Position.X = x;
			Position.Y = y;
			Radius = radius;
		}

		public float GetDistance(Vector2 point)
		{
			GetDistance(ref point, out var result);
			return result;
		}

		public void GetDistance(ref Vector2 point, out float result)
		{
			Vector2.Subtract(ref point, ref Position, out var result2);
			result = result2.Length();
			result -= Radius;
		}

		public ContainmentType Contains(Vector2 point)
		{
			GetDistance(ref point, out var result);
			if (!(result <= 0f))
			{
				return ContainmentType.Disjoint;
			}
			return ContainmentType.Contains;
		}

		public void Contains(ref Vector2 point, out ContainmentType result)
		{
			GetDistance(ref point, out var result2);
			result = ((result2 <= 0f) ? ContainmentType.Contains : ContainmentType.Disjoint);
		}

		public ContainmentType Contains(BoundingCircle circle)
		{
			GetDistance(ref circle.Position, out var result);
			if (0f - result >= circle.Radius)
			{
				return ContainmentType.Contains;
			}
			if (result > circle.Radius)
			{
				return ContainmentType.Disjoint;
			}
			return ContainmentType.Intersects;
		}

		public void Contains(ref BoundingCircle circle, out ContainmentType result)
		{
			GetDistance(ref circle.Position, out var result2);
			if (0f - result2 >= circle.Radius)
			{
				result = ContainmentType.Contains;
			}
			else if (result2 > circle.Radius)
			{
				result = ContainmentType.Disjoint;
			}
			else
			{
				result = ContainmentType.Intersects;
			}
		}

		public ContainmentType Contains(AABB rect)
		{
			Contains(ref rect, out var result);
			return result;
		}

		public void Contains(ref AABB rect, out ContainmentType result)
		{
			Vector2 vector = default(Vector2);
			vector.X = MathHelper.Max(rect.UpperBound.X - Position.X, Position.X - rect.LowerBound.X);
			Vector2 vector2 = default(Vector2);
			vector2.X = MathHelper.Min(rect.UpperBound.X - Position.X, Position.X - rect.LowerBound.X);
			vector.Y = MathHelper.Max(rect.UpperBound.Y - Position.Y, Position.Y - rect.LowerBound.Y);
			vector2.Y = MathHelper.Min(rect.UpperBound.Y - Position.Y, Position.Y - rect.LowerBound.Y);
			float num = vector.Length();
			if (num <= Radius)
			{
				result = ContainmentType.Contains;
				return;
			}
			num = vector2.Length();
			if (num <= Radius)
			{
				result = ContainmentType.Intersects;
			}
			else
			{
				result = ContainmentType.Disjoint;
			}
		}

		public ContainmentType Contains(Vertices polygon, Vector2 worldPos)
		{
			Contains(ref polygon, ref worldPos, out var result);
			return result;
		}

		private void GetDistance(Vector2 vertex1, Vector2 vertex2, Vector2 point, out float result)
		{
			Vector2.Subtract(ref point, ref vertex2, out var result2);
			Vector2.Subtract(ref vertex1, ref vertex2, out var result3);
			float num = (float)Math.Sqrt(result3.X * result3.X + result3.Y * result3.Y);
			result3 = Vector2.Normalize(result3);
			float num2 = result2.Y * result3.X - result2.X * result3.Y;
			float num3 = result2.X * result3.X + result2.Y * result3.Y;
			if (num3 < 0f)
			{
				result = (float)Math.Sqrt(num3 * num3 + num2 * num2);
			}
			else if (num3 > num)
			{
				num3 -= num;
				result = (float)Math.Sqrt(num3 * num3 + num2 * num2);
			}
			else
			{
				result = Math.Abs(num2);
			}
		}

		public void Contains(ref Vertices vertices, ref Vector2 worldPos, out ContainmentType result)
		{
			if (vertices == null)
			{
				throw new ArgumentNullException("Not a polygon");
			}
			result = ContainmentType.Disjoint;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (result == ContainmentType.Intersects)
				{
					break;
				}
				Vector2 point = vertices[i] + worldPos;
				Contains(ref point, out var result2);
				result |= result2;
			}
			if (result != ContainmentType.Disjoint)
			{
				return;
			}
			bool flag = false;
			for (int j = 0; j < vertices.Count; j++)
			{
				int index = (j + 1) % vertices.Count;
				GetDistance(vertices[j] + worldPos, vertices[index] + worldPos, Position, out var result3);
				if (result3 <= Radius)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				result = ContainmentType.Intersects;
			}
		}

		public bool Intersects(AABB rect)
		{
			Intersects(ref rect, out var result);
			return result;
		}

		public bool Intersects(BoundingCircle circle)
		{
			Intersects(ref circle, out var result);
			return result;
		}

		public void Intersects(ref AABB rect, out bool result)
		{
			Vector2.Clamp(ref Position, ref rect.LowerBound, ref rect.UpperBound, out var result2);
			Vector2.DistanceSquared(ref Position, ref result2, out var result3);
			result = result3 <= Radius * Radius;
		}

		public void Intersects(ref BoundingCircle circle, out bool result)
		{
			Vector2.DistanceSquared(ref Position, ref circle.Position, out var result2);
			result = result2 <= Radius * Radius + circle.Radius * circle.Radius;
		}

		public override string ToString()
		{
			return $"P: {Position} R: {Radius}";
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode() ^ Radius.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is BoundingCircle)
			{
				return Equals((BoundingCircle)obj);
			}
			return false;
		}

		public bool Equals(BoundingCircle other)
		{
			return Equals(ref this, ref other);
		}

		public static bool Equals(BoundingCircle circle1, BoundingCircle circle2)
		{
			return Equals(ref circle1, ref circle2);
		}

		public static bool Equals(ref BoundingCircle circle1, ref BoundingCircle circle2)
		{
			if (object.Equals(circle1.Position, circle2.Position))
			{
				return circle1.Radius == circle2.Radius;
			}
			return false;
		}

		public static bool operator ==(BoundingCircle circle1, BoundingCircle circle2)
		{
			return Equals(ref circle1, ref circle2);
		}

		public static bool operator !=(BoundingCircle circle1, BoundingCircle circle2)
		{
			return !Equals(ref circle1, ref circle2);
		}
	}
}
