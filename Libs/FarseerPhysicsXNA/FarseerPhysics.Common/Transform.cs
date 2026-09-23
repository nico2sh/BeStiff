using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	/// <summary>
	/// A transform contains translation and rotation. It is used to represent
	/// the position and orientation of rigid frames.
	/// </summary>
	public struct Transform
	{
		public Vector2 Position;

		public Mat22 R;

		/// <summary>
		/// Calculate the angle that the rotation matrix represents.
		/// </summary>
		/// <value></value>
		public float Angle => (float)Math.Atan2(R.Col1.Y, R.Col1.X);

		/// <summary>
		/// Initialize using a position vector and a rotation matrix.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="r">The r.</param>
		public Transform(ref Vector2 position, ref Mat22 r)
		{
			Position = position;
			R = r;
		}

		/// <summary>
		/// Set this to the identity transform.
		/// </summary>
		public void SetIdentity()
		{
			Position = Vector2.Zero;
			R.SetIdentity();
		}

		/// <summary>
		/// Set this based on the position and angle.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="angle">The angle.</param>
		public void Set(Vector2 position, float angle)
		{
			Position = position;
			R.Set(angle);
		}
	}
}
