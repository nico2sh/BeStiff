namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which fades the opacity of particles from one to zero. This is faster than using
	/// an OpacityModifier to achieve the same result, but you cannot alter the opacity levels.
	/// </summary>
	public sealed class OpacityFastFadeModifier : Modifier
	{
		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			return new OpacityFastFadeModifier();
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				ptr->Colour.W = 1f - ptr->Age;
			}
		}
	}
}
