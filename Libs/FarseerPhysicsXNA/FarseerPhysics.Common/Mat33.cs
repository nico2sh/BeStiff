using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	/// <summary>
	/// A 3-by-3 matrix. Stored in column-major order.
	/// </summary>
	public struct Mat33
	{
		public Vector3 Col1;

		public Vector3 Col2;

		public Vector3 Col3;

		/// <summary>
		/// Construct this matrix using columns.
		/// </summary>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		/// <param name="c3">The c3.</param>
		public Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
		{
			Col1 = c1;
			Col2 = c2;
			Col3 = c3;
		}

		/// <summary>
		/// Set this matrix to all zeros.
		/// </summary>
		public void SetZero()
		{
			Col1 = Vector3.Zero;
			Col2 = Vector3.Zero;
			Col3 = Vector3.Zero;
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector3 Solve33(Vector3 b)
		{
			float num = Vector3.Dot(Col1, Vector3.Cross(Col2, Col3));
			if (num != 0f)
			{
				num = 1f / num;
			}
			return new Vector3(num * Vector3.Dot(b, Vector3.Cross(Col2, Col3)), num * Vector3.Dot(Col1, Vector3.Cross(b, Col3)), num * Vector3.Dot(Col1, Vector3.Cross(Col2, b)));
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases. Solve only the upper
		/// 2-by-2 matrix equation.
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public Vector2 Solve22(Vector2 b)
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
	}
}
