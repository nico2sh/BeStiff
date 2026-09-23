using System;
using Microsoft.Xna.Framework;

namespace ProjectMercury
{
	/// <summary>
	/// Defines the data structure for a particle.
	/// </summary>
	public struct Particle
	{
		/// <summary>
		/// Gets or sets the position of the particle.
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// Gets or sets the scale of the particle.
		/// </summary>
		public float Scale;

		/// <summary>
		/// Gets or sets the rotation of the particle in radians.
		/// </summary>
		public float Rotation;

		/// <summary>
		/// Gets or sets the colour of the particle. The W component is opacity.
		/// </summary>
		public Vector4 Colour;

		/// <summary>
		///  Gets or sets the current momentum of the particle.
		/// </summary>
		public Vector2 Momentum;

		/// <summary>
		/// Gets or sets the sum of the forces which are currently acting on the particle.
		/// </summary>
		public Vector2 Velocity;

		/// <summary>
		/// Gets or sets the time at which the particle was released.
		/// </summary>
		public float Inception;

		/// <summary>
		/// Gets or sets the age of the particle in the range 0-1.
		/// </summary>
		public float Age;

		/// <summary>
		/// Applies a force to the particle.
		/// </summary>
		/// <param name="force">A vector describing the force and direction.</param>
		public void ApplyForce(ref Vector2 force)
		{
			Velocity.X += force.X;
			Velocity.Y += force.Y;
		}

		/// <summary>
		/// Applies a rotation to the Particle.
		/// </summary>
		/// <param name="radians">The angle to rotate in radians.</param>
		public void Rotate(float radians)
		{
			Rotation += radians;
			if (Rotation > 3.141593f)
			{
				Rotation -= 6.283185f;
			}
			else if (Rotation < -3.141593f)
			{
				Rotation += 6.283185f;
			}
		}

		/// <summary>
		/// Updates the particle.
		/// </summary>
		/// <param name="deltaSeconds">Elapsed seconds since the last update.</param>
		/// <remarks>This method has been manually inlined in the Emitter base class Update method,
		/// its implementation has been left here for reference &amp; clarity.</remarks>
		[Obsolete("No longer used!")]
		internal void Update(float deltaSeconds)
		{
			Momentum.X += Velocity.X;
			Momentum.Y += Velocity.Y;
			Velocity.X = (Velocity.Y = 0f);
			Vector2 vector = default(Vector2);
			vector.X = Momentum.X * deltaSeconds;
			vector.Y = Momentum.Y * deltaSeconds;
			Position.X += vector.X;
			Position.Y += vector.Y;
		}
	}
}
