using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which constrains &amp; deflects particles inside a rectangle.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.RectangleConstraintDeflectorTypeConverter, ProjectMercury.Design")]
	public sealed class RectangleConstraintDeflector : Modifier
	{
		/// <summary>
		/// Defines the position of the rectangle boundary constraint.
		/// </summary>
		public Vector2 Position;

		private float _width;

		private float _height;

		private VariableFloat _restitutionCoefficient;

		/// <summary>
		/// Gets or sets the width of the rectangle deflector.
		/// </summary>
		/// <value>The width of the rectangle deflector.</value>
		public float Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of the rectangle.
		/// </summary>
		/// <value>The height of the rectangle.</value>
		public float Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		/// <summary>
		/// Gets or sets the restitution coefficient (bounce factor) of Particles when the hit the deflector.
		/// </summary>
		public VariableFloat RestitutionCoefficient
		{
			get
			{
				return _restitutionCoefficient;
			}
			set
			{
				_restitutionCoefficient = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			RectangleConstraintDeflector rectangleConstraintDeflector = new RectangleConstraintDeflector();
			rectangleConstraintDeflector.Height = Height;
			rectangleConstraintDeflector.Position = Position;
			rectangleConstraintDeflector.RestitutionCoefficient = RestitutionCoefficient;
			rectangleConstraintDeflector.Width = Width;
			return rectangleConstraintDeflector;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="dt">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float dt, Particle* particleArray, int count)
		{
			float x = Position.X;
			float num = Position.X + Width;
			float y = Position.Y;
			float num2 = Position.Y + Height;
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particleArray + i;
				_ = ptr->Scale;
				if (ptr->Position.X < x)
				{
					ptr->Position.X = x;
					float num3 = RestitutionCoefficient.Sample();
					ptr->Momentum.X *= 0f - num3;
				}
				else if (ptr->Position.X > num)
				{
					ptr->Position.X = num;
					float num4 = RestitutionCoefficient.Sample();
					ptr->Momentum.X *= 0f - num4;
				}
				if (ptr->Position.Y < y)
				{
					ptr->Position.Y = y;
					float num5 = RestitutionCoefficient.Sample();
					ptr->Momentum.Y *= 0f - num5;
				}
				else if (ptr->Position.Y > num2)
				{
					ptr->Position.Y = num2;
					float num6 = RestitutionCoefficient.Sample();
					ptr->Momentum.Y *= 0f - num6;
				}
			}
		}
	}
}
