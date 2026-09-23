using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which changes the scale of particles based on a linear interpolation over three values.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ScaleInterpolatorModifierTypeConverter, ProjectMercury.Design")]
	public class ScaleInterpolatorModifier : Modifier
	{
		private float _initialScale;

		private float _middleScale;

		private float _middlePosition;

		private float _finalScale;

		/// <summary>
		/// Gets or sets the initial scale.
		/// </summary>
		public float InitialScale
		{
			get
			{
				return _initialScale;
			}
			set
			{
				_initialScale = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle scale.
		/// </summary>
		public float MiddleScale
		{
			get
			{
				return _middleScale;
			}
			set
			{
				_middleScale = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle scale position.
		/// </summary>
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
		/// Gets or sets the final scale.
		/// </summary>
		public float FinalScale
		{
			get
			{
				return _finalScale;
			}
			set
			{
				_finalScale = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ScaleInterpolatorModifier scaleInterpolatorModifier = new ScaleInterpolatorModifier();
			scaleInterpolatorModifier.InitialScale = InitialScale;
			scaleInterpolatorModifier.MiddleScale = MiddleScale;
			scaleInterpolatorModifier.MiddlePosition = MiddlePosition;
			scaleInterpolatorModifier.FinalScale = FinalScale;
			return scaleInterpolatorModifier;
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
					ptr->Scale = ptr2->Scale;
				}
				else if (ptr->Age < MiddlePosition)
				{
					ptr->Scale = InitialScale + (MiddleScale - InitialScale) * (ptr->Age / MiddlePosition);
				}
				else
				{
					ptr->Scale = MiddleScale + (FinalScale - MiddleScale) * ((ptr->Age - MiddlePosition) / (1f - MiddlePosition));
				}
			}
		}
	}
}
