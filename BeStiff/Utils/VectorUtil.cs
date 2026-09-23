using Microsoft.Xna.Framework;

namespace Be_Stiff.Utils
{
	internal static class VectorUtil
	{
		/// <summary>
		/// Normalizes a vector, returning zero instead of NaN for a zero-length
		/// input. Used by kinematic bodies that steer towards a destination
		/// they may already be sitting on.
		/// </summary>
		public static Vector2 SafeNormalize(Vector2 v)
		{
			float lengthSquared = v.LengthSquared();
			if (lengthSquared < 1e-12f)
			{
				return Vector2.Zero;
			}
			return v / (float)System.Math.Sqrt(lengthSquared);
		}

		public static float GetAngle(this Vector2 v)
		{
			return (float)System.Math.Atan2(v.Y, v.X);
		}

		public static Vector2 CreateVector2(float angle, float length)
		{
			return new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * length;
		}
	}
}
