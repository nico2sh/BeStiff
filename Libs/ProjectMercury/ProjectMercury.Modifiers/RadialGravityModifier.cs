using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which pulls Particles towards it.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RadialGravityModifierTypeConverter, ProjectMercury.Design")]
	public sealed class RadialGravityModifier : Modifier
	{
		/// <summary>
		/// The position of the gravity well.
		/// </summary>
		public Vector2 Position;

		private float _radius;

		private float SquareRadius;

		private float _innerRadius;

		private float SquareInnerRadius;

		/// <summary>
		/// The strength of the gravity well.
		/// </summary>
		public float Strength;

		/// <summary>
		/// Gets or sets the radius of the gravity well.
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the specified value is negetive or zero.</exception>
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
		/// Gets or sets the inner radius of the gravity well, within which Particles will not be attracted.
		/// </summary>
		public float InnerRadius
		{
			get
			{
				return _innerRadius;
			}
			set
			{
				_innerRadius = value;
				SquareInnerRadius = value * value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RadialGravityModifier radialGravityModifier = new RadialGravityModifier();
			radialGravityModifier.Radius = Radius;
			radialGravityModifier.InnerRadius = InnerRadius;
			radialGravityModifier.Position = Position;
			radialGravityModifier.Strength = Strength;
			return radialGravityModifier;
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
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				Vector2 vector = new Vector2
				{
					X = Position.X - ptr->Position.X,
					Y = Position.Y - ptr->Position.Y
				};
				float num2 = vector.X * vector.X + vector.Y * vector.Y;
				if (num2 < SquareRadius && num2 > SquareInnerRadius)
				{
					float num3 = Calculator.Sqrt(num2);
					Vector2 vector2 = new Vector2
					{
						X = vector.X / num3,
						Y = vector.Y / num3
					};
					float num4 = SquareRadius / num2;
					vector2.X *= num4;
					vector2.Y *= num4;
					vector2.X *= num;
					vector2.Y *= num;
					ptr->Velocity.X += vector2.X;
					ptr->Velocity.Y += vector2.Y;
				}
			}
		}
	}
}
