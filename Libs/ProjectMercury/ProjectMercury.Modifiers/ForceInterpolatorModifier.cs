using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which changes the Force of particles based on a linear interpolation over three values.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ForceInterpolatorModifierTypeConverter, ProjectMercury.Design")]
	public class ForceInterpolatorModifier : Modifier
	{
		private Vector2 _initialForce;

		private Vector2 _middleForce;

		private float _middlePosition;

		private Vector2 _finalForce;

		/// <summary>
		/// Gets or sets the initial force vector.
		/// </summary>
		/// <value>The initial force vector.</value>
		public Vector2 InitialForce
		{
			get
			{
				return _initialForce;
			}
			set
			{
				_initialForce = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle force vector.
		/// </summary>
		/// <value>The middle force vector.</value>
		public Vector2 MiddleForce
		{
			get
			{
				return _middleForce;
			}
			set
			{
				_middleForce = value;
			}
		}

		/// <summary>
		/// Gets or sets the middle force position.
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
		/// Gets or sets the final force vector.
		/// </summary>
		/// <value>The final force vector.</value>
		public Vector2 FinalForce
		{
			get
			{
				return _finalForce;
			}
			set
			{
				_finalForce = value;
			}
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ForceInterpolatorModifier forceInterpolatorModifier = new ForceInterpolatorModifier();
			forceInterpolatorModifier.InitialForce = InitialForce;
			forceInterpolatorModifier.MiddleForce = MiddleForce;
			forceInterpolatorModifier.MiddlePosition = MiddlePosition;
			forceInterpolatorModifier.FinalForce = FinalForce;
			return forceInterpolatorModifier;
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
					ptr->Velocity.X = ptr2->Velocity.X;
					ptr->Velocity.Y = ptr2->Velocity.Y;
				}
				else if (ptr->Age < MiddlePosition)
				{
					float num = ptr->Age / MiddlePosition;
					ptr->Velocity.X += InitialForce.X + (MiddleForce.X - InitialForce.X) * num;
					ptr->Velocity.Y += InitialForce.Y + (MiddleForce.Y - InitialForce.Y) * num;
				}
				else
				{
					float num2 = (ptr->Age - MiddlePosition) / (1f - MiddlePosition);
					ptr->Velocity.X += MiddleForce.X + (FinalForce.X - MiddleForce.X) * num2;
					ptr->Velocity.Y += MiddleForce.Y + (FinalForce.Y - MiddleForce.Y) * num2;
				}
			}
		}
	}
}
