using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which limits the velocity of Particles to a specified value.
	/// </summary>
	/// <remarks>For best results insert this Modifier after any other Modifiers which may alter
	/// the velocity of the Particle.</remarks>
	[TypeConverter("ProjectMercury.Design.Modifiers.VelocityClampModifierTypeConverter, ProjectMercury.Design")]
	public class VelocityClampModifier : Modifier
	{
		private float _maximumVelocity;

		private float SquareMaximumVelocity;

		/// <summary>
		/// Gets or sets the maximum velocity of Particles..
		/// </summary>
		/// <value>The maximum velocity of Particles..</value>
		public float MaximumVelocity
		{
			get
			{
				return _maximumVelocity;
			}
			set
			{
				_maximumVelocity = value;
				SquareMaximumVelocity = value * value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			VelocityClampModifier velocityClampModifier = new VelocityClampModifier();
			velocityClampModifier.MaximumVelocity = MaximumVelocity;
			return velocityClampModifier;
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
				float num = ptr->Velocity.X * ptr->Velocity.X + ptr->Velocity.Y * ptr->Velocity.Y;
				if (num > SquareMaximumVelocity)
				{
					float num2 = Calculator.Sqrt(num);
					ptr->Velocity.X = ptr->Velocity.X / num2 * MaximumVelocity;
					ptr->Velocity.Y = ptr->Velocity.Y / num2 * MaximumVelocity;
				}
			}
		}
	}
}
