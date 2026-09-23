using System.ComponentModel;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which adjusts the rotation of a Particle to follow its trajectory.
	/// </summary>
	/// <remarks>Ideally this modifier should be added *after* any other physics modifiers.</remarks>
	[TypeConverter("ProjectMercury.Design.Modifiers.TrajectoryRotationModifierTypeConverter, ProjectMercury.Design")]
	public sealed class TrajectoryRotationModifier : Modifier
	{
		/// <summary>
		/// The rotation offset to add to the calculated trajectory rotation.
		/// </summary>
		public float RotationOffset;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			TrajectoryRotationModifier trajectoryRotationModifier = new TrajectoryRotationModifier();
			trajectoryRotationModifier.RotationOffset = RotationOffset;
			return trajectoryRotationModifier;
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
				Particle* ptr = particle - 1;
				if (particle->Momentum == ptr->Momentum)
				{
					particle->Rotation = ptr->Rotation;
					continue;
				}
				float num = Calculator.Atan2(particle->Momentum.Y, particle->Momentum.X);
				particle->Rotation = num + RotationOffset;
				if (particle->Rotation > 3.141593f)
				{
					particle->Rotation -= 6.283185f;
				}
				else if (particle->Rotation < -3.141593f)
				{
					particle->Rotation += 6.283185f;
				}
				particle++;
			}
		}
	}
}
