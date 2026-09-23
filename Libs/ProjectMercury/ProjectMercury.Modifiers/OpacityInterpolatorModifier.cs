using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which changes the opacity of particles based on a linear interpolation over three values.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.OpacityInterpolatorModifierTypeConverter, ProjectMercury.Design")]
	public class OpacityInterpolatorModifier : Modifier
	{
		private float _initialOpacity;

		private float _middleOpacity;

		private float _middlePosition;

		private float _finalOpacity;

		/// <summary>
		/// Gets or sets the initial opacity.
		/// </summary>
		/// <value>The initial opacity.</value>
		public float InitialOpacity
		{
			get
			{
				return _initialOpacity;
			}
			set
			{
				_initialOpacity = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle opacity.
		/// </summary>
		/// <value>The middle opacity.</value>
		public float MiddleOpacity
		{
			get
			{
				return _middleOpacity;
			}
			set
			{
				_middleOpacity = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle opacity position.
		/// </summary>
		/// <value>The middle position.</value>
		public float MiddlePosition
		{
			get
			{
				return _middlePosition;
			}
			set
			{
				_middlePosition = value;
			}
		}

		/// <summary>
		/// Gets or sets the final opacity.
		/// </summary>
		/// <value>The final opacity.</value>
		public float FinalOpacity
		{
			get
			{
				return _finalOpacity;
			}
			set
			{
				_finalOpacity = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			OpacityInterpolatorModifier opacityInterpolatorModifier = new OpacityInterpolatorModifier();
			opacityInterpolatorModifier.InitialOpacity = InitialOpacity;
			opacityInterpolatorModifier.MiddleOpacity = MiddleOpacity;
			opacityInterpolatorModifier.MiddlePosition = MiddlePosition;
			opacityInterpolatorModifier.FinalOpacity = FinalOpacity;
			return opacityInterpolatorModifier;
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
				Particle* ptr2 = ptr - 1;
				if (ptr->Age == ptr2->Age)
				{
					ptr->Colour.W = ptr2->Colour.W;
				}
				else if (ptr->Age < MiddlePosition)
				{
					ptr->Colour.W = InitialOpacity + (MiddleOpacity - InitialOpacity) * (ptr->Age / MiddlePosition);
				}
				else
				{
					ptr->Colour.W = MiddleOpacity + (FinalOpacity - MiddleOpacity) * ((ptr->Age - MiddlePosition) / (1f - MiddlePosition));
				}
			}
		}
	}
}
