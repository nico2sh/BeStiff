using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which changes the rotation rate of particles over their lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RotationRateModifierTypeConverter, ProjectMercury.Design")]
	public sealed class RotationRateModifier : Modifier
	{
		/// <summary>
		/// Gets or sets the initial rotation rate in radians per second.
		/// </summary>
		public float InitialRate;

		/// <summary>
		/// Gets or sets the final rotation rate in radians per second.
		/// </summary>
		public float FinalRate;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RotationRateModifier rotationRateModifier = new RotationRateModifier();
			rotationRateModifier.InitialRate = InitialRate;
			rotationRateModifier.FinalRate = FinalRate;
			return rotationRateModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particle">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particle, int count)
		{
			float num = InitialRate * dt;
			float num2 = FinalRate * dt;
			for (int i = 0; i < count; i++)
			{
				float radians = num + (num2 - num) * particle->Age;
				particle->Rotate(radians);
				particle++;
			}
		}
	}
}
