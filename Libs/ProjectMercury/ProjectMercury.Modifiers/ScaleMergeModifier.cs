using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which merges the scale of particles towards a single scale over their lifetime. Works best
	/// when Particles are being released with random scale, where you require the particles to have a uniform scale
	/// at the end of their lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ScaleMergeModifierTypeConverter, ProjectMercury.Design")]
	public sealed class ScaleMergeModifier : Modifier
	{
		private float _mergeScale;

		/// <summary>
		/// Gets or sets the final scale of Particles when they are retired.
		/// </summary>
		/// <value>The merge scale.</value>
		public float MergeScale
		{
			get
			{
				return _mergeScale;
			}
			set
			{
				_mergeScale = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ScaleMergeModifier scaleMergeModifier = new ScaleMergeModifier();
			scaleMergeModifier.MergeScale = MergeScale;
			return scaleMergeModifier;
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
				float num = ptr->Age * 0.07f;
				float num2 = 1f - num;
				ptr->Scale = ptr->Scale * num2 + MergeScale * num;
			}
		}
	}
}
