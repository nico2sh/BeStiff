using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which gradually changes the opacity of a Particle over its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.OpacityModifierTypeConverter, ProjectMercury.Design")]
	public sealed class OpacityModifier : Modifier
	{
		private float _initial;

		private float _ultimate;

		/// <summary>
		/// Gets or sets the initial opacity of Particles as they are released.
		/// </summary>
		public float Initial
		{
			get
			{
				return _initial;
			}
			set
			{
				_initial = value;
			}
		}

		/// <summary>
		/// Gets or sets the ultimate opacity of Particles as they are retired.
		/// </summary>
		public float Ultimate
		{
			get
			{
				return _ultimate;
			}
			set
			{
				_ultimate = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			OpacityModifier opacityModifier = new OpacityModifier();
			opacityModifier.Initial = Initial;
			opacityModifier.Ultimate = Ultimate;
			return opacityModifier;
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
				ptr->Colour.W = Initial + (Ultimate - Initial) * ptr->Age;
			}
		}
	}
}
