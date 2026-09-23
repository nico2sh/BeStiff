using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which adjusts the opacity of Particles based on a sine wave.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.OpacityOscillatorTypeConverter, ProjectMercury.Design")]
	public class OpacityOscillator : Modifier
	{
		private float TotalSeconds;

		private float _frequency;

		private float _minimum;

		private float _maximum;

		/// <summary>
		/// Gets or sets the oscillator frequency (the number of cycles per second).
		/// </summary>
		public float Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				_frequency = value;
			}
		}

		/// <summary>
		/// Gets or sets the minimum opacity (the opacity of Particles at the negetive peak of the sine wave).
		/// </summary>
		/// <value>The minimum opacity.</value>
		public float MinimumOpacity
		{
			get
			{
				return _minimum;
			}
			set
			{
				_minimum = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum opacity (the opacity of Particles at the positive peak of the sine wave).
		/// </summary>
		/// <value>The maximum opacity.</value>
		public float MaximumOpacity
		{
			get
			{
				return _maximum;
			}
			set
			{
				_maximum = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			OpacityOscillator opacityOscillator = new OpacityOscillator();
			opacityOscillator.Frequency = Frequency;
			opacityOscillator.MinimumOpacity = MinimumOpacity;
			opacityOscillator.MaximumOpacity = MaximumOpacity;
			return opacityOscillator;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			TotalSeconds += dt;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				float num = TotalSeconds - ptr->Inception;
				float num2 = Calculator.Sin(num * (Frequency * 3f));
				ptr->Colour.W = (MaximumOpacity - MinimumOpacity) * num2 + MinimumOpacity;
			}
		}
	}
}
