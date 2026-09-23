using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which applies a force to a Particle when it enters a rectangular area.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RectangleForceModifierTypeConverter, ProjectMercury.Design")]
	public sealed class RectangleForceModifier : Modifier
	{
		/// <summary>
		/// Gets or sets the position of the centre of the rectangular force area.
		/// </summary>
		public Vector2 Position;

		private float HalfWidth;

		private float HalfHeight;

		/// <summary>
		/// Gets or sets the force vector.
		/// </summary>
		public Vector2 Force;

		/// <summary>
		/// Gets or sets the strength of the force.
		/// </summary>
		public float Strength;

		/// <summary>
		/// Gets or sets the width of the rectangular force area.
		/// </summary>
		public float Width
		{
			get
			{
				return HalfWidth + HalfWidth;
			}
			set
			{
				HalfWidth = value * 0.5f;
			}
		}

		/// <summary>
		/// Gets or sets the height of the rectangular force area.
		/// </summary>
		public float Height
		{
			get
			{
				return HalfHeight + HalfHeight;
			}
			set
			{
				HalfHeight = value * 0.5f;
			}
		}

		/// <summary>
		/// Gets the position of the left edge of the rectangle.
		/// </summary>
		public float Left => Position.X - HalfWidth;

		/// <summary>
		/// Gets the position of the right edge of the rectangle.
		/// </summary>
		public float Right => Position.X + HalfWidth;

		/// <summary>
		/// Gets the position of the top edge of the rectangle.
		/// </summary>
		public float Top => Position.Y - HalfHeight;

		/// <summary>
		/// Gets the position of the bottom edge of the rectangle.
		/// </summary>
		public float Bottom => Position.Y + HalfHeight;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RectangleForceModifier rectangleForceModifier = new RectangleForceModifier();
			rectangleForceModifier.Position = Position;
			rectangleForceModifier.Width = Width;
			rectangleForceModifier.Height = Height;
			rectangleForceModifier.Force = Force;
			rectangleForceModifier.Strength = Strength;
			return rectangleForceModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float num = Force.X * (Strength * dt);
			float num2 = Force.Y * (Strength * dt);
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				if (ptr->Position.X > Left && ptr->Position.X < Right && ptr->Position.Y > Top && ptr->Position.Y < Bottom)
				{
					ptr->Velocity.X += num;
					ptr->Velocity.Y += num2;
				}
			}
		}
	}
}
