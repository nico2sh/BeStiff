using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which applies a force to a Particle when it enters a circular area.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RadialForceModifierTypeConverter, ProjectMercury.Design")]
	public class RadialForceModifier : Modifier
	{
		/// <summary>
		/// Gets or sets the position of the force.
		/// </summary>
		public Vector2 Position;

		private float _radius;

		private float SquareRadius;

		/// <summary>
		/// Gets or sets the force vector.
		/// </summary>
		public Vector2 Force;

		/// <summary>
		/// Gets or sets the strength of the force.
		/// </summary>
		public float Strength;

		/// <summary>
		/// Gets or sets the radius of the force.
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the specified value is negetive or zero</exception>
		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				SquareRadius = value * value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RadialForceModifier radialForceModifier = new RadialForceModifier();
			radialForceModifier.Position = Position;
			radialForceModifier.Radius = Radius;
			radialForceModifier.Force = Force;
			radialForceModifier.Strength = Strength;
			return radialForceModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float num = Strength * dt;
			float num2 = Force.X * num;
			float num3 = Force.Y * num;
			Vector2 vector = default(Vector2);
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				vector.X = Position.X - ptr->Position.X;
				vector.Y = Position.Y - ptr->Position.Y;
				float num4 = vector.X * vector.X + vector.Y * vector.Y;
				if (num4 < SquareRadius)
				{
					ptr->Velocity.X += num2;
					ptr->Velocity.Y += num3;
				}
			}
		}
	}
}
