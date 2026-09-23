using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which alters the rotation of a Particle over its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RotationModifierTypeConverter, ProjectMercury.Design")]
	public sealed class RotationModifier : Modifier
	{
		/// <summary>
		/// The rate of rotation in radians per second.
		/// </summary>
		public float RotationRate;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RotationModifier rotationModifier = new RotationModifier();
			rotationModifier.RotationRate = RotationRate;
			return rotationModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float radians = RotationRate * dt;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				ptr->Rotate(radians);
			}
		}
	}
}
