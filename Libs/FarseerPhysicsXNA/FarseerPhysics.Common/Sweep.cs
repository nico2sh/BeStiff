using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	/// <summary>
	/// This describes the motion of a body/shape for TOI computation.
	/// Shapes are defined with respect to the body origin, which may
	/// no coincide with the center of mass. However, to support dynamics
	/// we must interpolate the center of mass position.
	/// </summary>
	public struct Sweep
	{
		/// <summary>
		/// World angles
		/// </summary>
		public float A;

		public float A0;

		/// <summary>
		/// Fraction of the current time step in the range [0,1]
		/// c0 and a0 are the positions at alpha0.
		/// </summary>
		public float Alpha0;

		/// <summary>
		/// Center world positions
		/// </summary>
		public Vector2 C;

		public Vector2 C0;

		/// <summary>
		/// Local center of mass position
		/// </summary>
		public Vector2 LocalCenter;

		/// <summary>
		/// Get the interpolated transform at a specific time.
		/// </summary>
		/// <param name="xf">The transform.</param>
		/// <param name="beta">beta is a factor in [0,1], where 0 indicates alpha0.</param>
		public void GetTransform(out Transform xf, float beta)
		{
			xf = default(Transform);
			xf.Position.X = (1f - beta) * C0.X + beta * C.X;
			xf.Position.Y = (1f - beta) * C0.Y + beta * C.Y;
			float angle = (1f - beta) * A0 + beta * A;
			xf.R.Set(angle);
			xf.Position -= MathUtils.Multiply(ref xf.R, ref LocalCenter);
		}

		/// <summary>
		/// Advance the sweep forward, yielding a new initial state.
		/// </summary>
		/// <param name="alpha">new initial time..</param>
		public void Advance(float alpha)
		{
			float num = (alpha - Alpha0) / (1f - Alpha0);
			C0.X = (1f - num) * C0.X + num * C.X;
			C0.Y = (1f - num) * C0.Y + num * C.Y;
			A0 = (1f - num) * A0 + num * A;
			Alpha0 = alpha;
		}

		/// <summary>
		/// Normalize the angles.
		/// </summary>
		public void Normalize()
		{
			float num = (float)Math.PI * 2f * (float)Math.Floor(A0 / ((float)Math.PI * 2f));
			A0 -= num;
			A -= num;
		}
	}
}
