using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	/// <summary>
	/// A 2-by-2 matrix. Stored in column-major order.
	/// </summary>
	public struct Mat22
	{
		public Vector2 Col1;

		public Vector2 Col2;

		/// <summary>
		/// Extract the angle from this matrix (assumed to be
		/// a rotation matrix).
		/// </summary>
		/// <value></value>
		public float Angle => (float)Math.Atan2(Col1.Y, Col1.X);

		public Mat22 Inverse
		{
			get
			{
				float x = Col1.X;
				float x2 = Col2.X;
				float y = Col1.Y;
				float y2 = Col2.Y;
				float num = x * y2 - x2 * y;
				if (num != 0f)
				{
					num = 1f / num;
				}
				return new Mat22
				{
					Col1 = 
					{
						X = num * y2,
						Y = (0f - num) * y
					},
					Col2 = 
					{
						X = (0f - num) * x2,
						Y = num * x
					}
				};
			}
		}

		/// <summary>
		/// Construct this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		public Mat22(Vector2 c1, Vector2 c2)
		{
			Col1 = c1;
			Col2 = c2;
		}

		/// <summary>
		/// Construct this matrix using scalars.
		/// </summary>
		/// <param name="a11">The a11.</param>
		/// <param name="a12">The a12.</param>
		/// <param name="a21">The a21.</param>
		/// <param name="a22">The a22.</param>
		public Mat22(float a11, float a12, float a21, float a22)
		{
			Col1 = new Vector2(a11, a21);
			Col2 = new Vector2(a12, a22);
		}

		/// <summary>
		/// Construct this matrix using an angle. This matrix becomes
		/// an orthonormal rotation matrix.
		/// </summary>
		/// <param name="angle">The angle.</param>
		public Mat22(float angle)
		{
			float num = (float)Math.Cos(angle);
			float num2 = (float)Math.Sin(angle);
			Col1 = new Vector2(num, num2);
			Col2 = new Vector2(0f - num2, num);
		}

		/// <summary>
		/// Initialize this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		public void Set(Vector2 c1, Vector2 c2)
		{
			Col1 = c1;
			Col2 = c2;
		}

		/// <summary>
		/// Initialize this matrix using an angle. This matrix becomes
		/// an orthonormal rotation matrix.
		/// </summary>
		/// <param name="angle">The angle.</param>
		public void Set(float angle)
		{
			float num = (float)Math.Cos(angle);
			float num2 = (float)Math.Sin(angle);
			Col1.X = num;
			Col2.X = 0f - num2;
			Col1.Y = num2;
			Col2.Y = num;
		}

		/// <summary>
		/// Set this to the identity matrix.
		/// </summary>
		public void SetIdentity()
		{
			Col1.X = 1f;
			Col2.X = 0f;
			Col1.Y = 0f;
			Col2.Y = 1f;
		}

		/// <summary>
		/// Set this matrix to all zeros.
		/// </summary>
		public void SetZero()
		{
			Col1.X = 0f;
			Col2.X = 0f;
			Col1.Y = 0f;
			Col2.Y = 0f;
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector2 Solve(Vector2 b)
		{
			float x = Col1.X;
			float x2 = Col2.X;
			float y = Col1.Y;
			float y2 = Col2.Y;
			float num = x * y2 - x2 * y;
			if (num != 0f)
			{
				num = 1f / num;
			}
			return new Vector2(num * (y2 * b.X - x2 * b.Y), num * (x * b.Y - y * b.X));
		}

		public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
		{
			R.Col1 = A.Col1 + B.Col1;
			R.Col2 = A.Col2 + B.Col2;
		}
	}
}
