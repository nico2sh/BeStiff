using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier that applies a constant force vector to Particles over their lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.LinearGravityModifierTypeConverter, ProjectMercury.Design")]
	public sealed class LinearGravityModifier : Modifier
	{
		/// <summary>
		/// Gets or sets the gravity vector.
		/// </summary>
		public Vector2 Gravity;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			LinearGravityModifier linearGravityModifier = new LinearGravityModifier();
			linearGravityModifier.Gravity = Gravity;
			return linearGravityModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float num = Gravity.X * dt;
			float num2 = Gravity.Y * dt;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				ptr->Velocity.X += num;
				ptr->Velocity.Y += num2;
			}
		}
	}
}
