using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// This holds the mass data computed for a shape.
	/// </summary>
	public struct MassData : IEquatable<MassData>
	{
		/// <summary>
		/// The area of the shape
		/// </summary>
		public float Area;

		/// <summary>
		/// The position of the shape's centroid relative to the shape's origin.
		/// </summary>
		public Vector2 Centroid;

		/// <summary>
		/// The rotational inertia of the shape about the local origin.
		/// </summary>
		public float Inertia;

		/// <summary>
		/// The mass of the shape, usually in kilograms.
		/// </summary>
		public float Mass;

		public bool Equals(MassData other)
		{
			return this == other;
		}

		public static bool operator ==(MassData left, MassData right)
		{
			if (left.Area == right.Area && left.Mass == right.Mass && left.Centroid == right.Centroid)
			{
				return left.Inertia == right.Inertia;
			}
			return false;
		}

		public static bool operator !=(MassData left, MassData right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (obj.GetType() != typeof(MassData))
			{
				return false;
			}
			return Equals((MassData)obj);
		}

		public override int GetHashCode()
		{
			int hashCode = Area.GetHashCode();
			hashCode = (hashCode * 397) ^ Centroid.GetHashCode();
			hashCode = (hashCode * 397) ^ Inertia.GetHashCode();
			return (hashCode * 397) ^ Mass.GetHashCode();
		}
	}
}
