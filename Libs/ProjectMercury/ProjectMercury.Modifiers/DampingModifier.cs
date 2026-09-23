using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which applies a damping force to a Particle over its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.DampingModifierTypeConverter, ProjectMercury.Design")]
	public sealed class DampingModifier : Modifier
	{
		/// <summary>
		/// The damping coefficient.
		/// </summary>
		public float DampingCoefficient;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			DampingModifier dampingModifier = new DampingModifier();
			dampingModifier.DampingCoefficient = DampingCoefficient;
			return dampingModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float num = DampingCoefficient * dt * -1f;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				ptr->Velocity.X += ptr->Momentum.X * num;
				ptr->Velocity.Y += ptr->Momentum.Y * num;
			}
		}
	}
}
