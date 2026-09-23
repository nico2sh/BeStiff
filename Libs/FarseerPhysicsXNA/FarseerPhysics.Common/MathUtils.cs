using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public static class MathUtils
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatConverter
		{
			[FieldOffset(0)]
			public float x;

			[FieldOffset(0)]
			public int i;
		}

		public static float Cross(Vector2 a, Vector2 b)
		{
			return a.X * b.Y - a.Y * b.X;
		}

		public static Vector2 Cross(Vector2 a, float s)
		{
			return new Vector2(s * a.Y, (0f - s) * a.X);
		}

		public static Vector2 Cross(float s, Vector2 a)
		{
			return new Vector2((0f - s) * a.Y, s * a.X);
		}

		public static Vector2 Abs(Vector2 v)
		{
			return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
		}

		public static Vector2 Multiply(ref Mat22 A, Vector2 v)
		{
			return Multiply(ref A, ref v);
		}

		public static Vector2 Multiply(ref Mat22 A, ref Vector2 v)
		{
			return new Vector2(A.Col1.X * v.X + A.Col2.X * v.Y, A.Col1.Y * v.X + A.Col2.Y * v.Y);
		}

		public static Vector2 MultiplyT(ref Mat22 A, Vector2 v)
		{
			return MultiplyT(ref A, ref v);
		}

		public static Vector2 MultiplyT(ref Mat22 A, ref Vector2 v)
		{
			return new Vector2(v.X * A.Col1.X + v.Y * A.Col1.Y, v.X * A.Col2.X + v.Y * A.Col2.Y);
		}

		public static Vector2 Multiply(ref Transform T, Vector2 v)
		{
			return Multiply(ref T, ref v);
		}

		public static Vector2 Multiply(ref Transform T, ref Vector2 v)
		{
			return new Vector2(T.Position.X + T.R.Col1.X * v.X + T.R.Col2.X * v.Y, T.Position.Y + T.R.Col1.Y * v.X + T.R.Col2.Y * v.Y);
		}

		public static Vector2 MultiplyT(ref Transform T, Vector2 v)
		{
			return MultiplyT(ref T, ref v);
		}

		public static Vector2 MultiplyT(ref Transform T, ref Vector2 v)
		{
			Vector2 v2 = Vector2.Zero;
			v2.X = v.X - T.Position.X;
			v2.Y = v.Y - T.Position.Y;
			return MultiplyT(ref T.R, ref v2);
		}

		public static void MultiplyT(ref Mat22 A, ref Mat22 B, out Mat22 C)
		{
			C = default(Mat22);
			C.Col1.X = A.Col1.X * B.Col1.X + A.Col1.Y * B.Col1.Y;
			C.Col1.Y = A.Col2.X * B.Col1.X + A.Col2.Y * B.Col1.Y;
			C.Col2.X = A.Col1.X * B.Col2.X + A.Col1.Y * B.Col2.Y;
			C.Col2.Y = A.Col2.X * B.Col2.X + A.Col2.Y * B.Col2.Y;
		}

		public static void MultiplyT(ref Transform A, ref Transform B, out Transform C)
		{
			C = default(Transform);
			MultiplyT(ref A.R, ref B.R, out C.R);
			C.Position.X = B.Position.X - A.Position.X;
			C.Position.Y = B.Position.Y - A.Position.Y;
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T val = a;
			a = b;
			b = val;
		}

		/// <summary>
		/// This function is used to ensure that a floating point number is
		/// not a NaN or infinity.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns>
		/// 	<c>true</c> if the specified x is valid; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValid(float x)
		{
			if (float.IsNaN(x))
			{
				return false;
			}
			return !float.IsInfinity(x);
		}

		public static bool IsValid(this Vector2 x)
		{
			if (IsValid(x.X))
			{
				return IsValid(x.Y);
			}
			return false;
		}

		/// <summary>
		/// This is a approximate yet fast inverse square-root.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns></returns>
		public static float InvSqrt(float x)
		{
			FloatConverter floatConverter = default(FloatConverter);
			floatConverter.x = x;
			float num = 0.5f * x;
			floatConverter.i = 1597463007 - (floatConverter.i >> 1);
			x = floatConverter.x;
			x *= 1.5f - num * x * x;
			return x;
		}

		public static int Clamp(int a, int low, int high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static float Clamp(float a, float low, float high)
		{
			return Math.Max(low, Math.Min(a, high));
		}

		public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
		{
			return Vector2.Max(low, Vector2.Min(a, high));
		}

		public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
		{
			c = a.X * b.Y - a.Y * b.X;
		}

		/// <summary>
		/// Return the angle between two vectors on a plane
		/// The angle is from vector 1 to vector 2, positive anticlockwise
		/// The result is between -pi -&gt; pi
		/// </summary>
		public static double VectorAngle(ref Vector2 p1, ref Vector2 p2)
		{
			double num = Math.Atan2(p1.Y, p1.X);
			double num2 = Math.Atan2(p2.Y, p2.X);
			double num3;
			for (num3 = num2 - num; num3 > Math.PI; num3 -= Math.PI * 2.0)
			{
			}
			for (; num3 < -Math.PI; num3 += Math.PI * 2.0)
			{
			}
			return num3;
		}

		public static double VectorAngle(Vector2 p1, Vector2 p2)
		{
			return VectorAngle(ref p1, ref p2);
		}

		/// <summary>
		/// Returns a positive number if c is to the left of the line going from a to b.
		/// </summary>
		/// <returns>Positive number if point is left, negative if point is right, 
		/// and 0 if points are collinear.</returns>
		public static float Area(Vector2 a, Vector2 b, Vector2 c)
		{
			return Area(ref a, ref b, ref c);
		}

		/// <summary>
		/// Returns a positive number if c is to the left of the line going from a to b.
		/// </summary>
		/// <returns>Positive number if point is left, negative if point is right, 
		/// and 0 if points are collinear.</returns>
		public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
		}

		/// <summary>
		/// Determines if three vertices are collinear (ie. on a straight line)
		/// </summary>
		/// <param name="a">First vertex</param>
		/// <param name="b">Second vertex</param>
		/// <param name="c">Third vertex</param>
		/// <returns></returns>
		public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			return Collinear(ref a, ref b, ref c, 0f);
		}

		public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance)
		{
			return FloatInRange(Area(ref a, ref b, ref c), 0f - tolerance, tolerance);
		}

		public static void Cross(float s, ref Vector2 a, out Vector2 b)
		{
			b = new Vector2((0f - s) * a.Y, s * a.X);
		}

		public static bool FloatEquals(float value1, float value2)
		{
			return Math.Abs(value1 - value2) <= 1.1920929E-07f;
		}

		/// <summary>
		/// Checks if a floating point Value is equal to another,
		/// within a certain tolerance.
		/// </summary>
		/// <param name="value1">The first floating point Value.</param>
		/// <param name="value2">The second floating point Value.</param>
		/// <param name="delta">The floating point tolerance.</param>
		/// <returns>True if the values are "equal", false otherwise.</returns>
		public static bool FloatEquals(float value1, float value2, float delta)
		{
			return FloatInRange(value1, value2 - delta, value2 + delta);
		}

		/// <summary>
		/// Checks if a floating point Value is within a specified
		/// range of values (inclusive).
		/// </summary>
		/// <param name="value">The Value to check.</param>
		/// <param name="min">The minimum Value.</param>
		/// <param name="max">The maximum Value.</param>
		/// <returns>True if the Value is within the range specified,
		/// false otherwise.</returns>
		public static bool FloatInRange(float value, float min, float max)
		{
			if (value >= min)
			{
				return value <= max;
			}
			return false;
		}
	}
}
