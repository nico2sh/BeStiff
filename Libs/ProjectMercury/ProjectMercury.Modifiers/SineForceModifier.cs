using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which applies a sine wave force to a Particle over the course of its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.SineForceModifierTypeConverter, ProjectMercury.Design")]
	public sealed class SineForceModifier : Modifier
	{
		private float TotalSeconds;

		/// <summary>
		/// Gets or sets the frequency of the sine wave.
		/// </summary>
		public float Frequency;

		/// <summary>
		/// Gets or sets the amplitude of the sine wave.
		/// </summary>
		public float Amplitude;

		private float AngleCos;

		private float AngleSin;

		/// <summary>
		/// Gets or sets the rotation of the sine force.
		/// </summary>
		/// <value>The rotation angle in radians.</value>
		public float Rotation
		{
			get
			{
				return Calculator.Atan2(AngleSin, AngleCos);
			}
			set
			{
				AngleCos = Calculator.Cos(value);
				AngleSin = Calculator.Sin(value);
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			SineForceModifier sineForceModifier = new SineForceModifier();
			sineForceModifier.Amplitude = Amplitude;
			sineForceModifier.Frequency = Frequency;
			sineForceModifier.Rotation = Rotation;
			return sineForceModifier;
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
			float num = Amplitude * dt;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				float num2 = TotalSeconds - ptr->Inception;
				float x = Calculator.Cos(num2 * Frequency);
				Vector2 vector = new Vector2(x, 0f);
				vector.X = vector.X * AngleCos + vector.Y * (0f - AngleSin);
				vector.Y = vector.X * AngleSin + vector.Y * AngleCos;
				vector.X *= num;
				vector.Y *= num;
				ptr->Velocity.X += vector.X;
				ptr->Velocity.Y += vector.Y;
			}
		}
	}
}
