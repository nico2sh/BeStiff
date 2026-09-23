using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which adjusts the scale of a Particle over its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ScaleModifierTypeConverter, ProjectMercury.Design")]
	public class ScaleModifier : Modifier
	{
		/// <summary>
		/// The initial scale of the Particle in pixels.
		/// </summary>
		public float InitialScale;

		/// <summary>
		/// The ultimate scale of the Particle in pixels.
		/// </summary>
		public float UltimateScale;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ScaleModifier scaleModifier = new ScaleModifier();
			scaleModifier.InitialScale = InitialScale;
			scaleModifier.UltimateScale = UltimateScale;
			return scaleModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particle">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particle, int count)
		{
			for (int i = 0; i < count; i++)
			{
				particle->Scale = InitialScale + (UltimateScale - InitialScale) * particle->Age;
				particle++;
			}
		}
	}
}
