using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which changes the colour of particles based on a linear interpolation over three values.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ColourInterpolatorModifierTypeConverter, ProjectMercury.Design")]
	public class ColourInterpolatorModifier : Modifier
	{
		private float _middlePosition;

		/// <summary>
		/// Gets or sets the initial colour.
		/// </summary>
		/// <value>The initial colour.</value>
		public Vector3 InitialColour { get; set; }

		/// <summary>
		/// Gets or sets the middle colour.
		/// </summary>
		/// <value>The middle colour.</value>
		public Vector3 MiddleColour { get; set; }

		/// <summary>
		/// Gets or sets the middle colour position.
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
		/// Gets or sets the final colour.
		/// </summary>
		/// <value>The final colour.</value>
		public Vector3 FinalColour { get; set; }

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ColourInterpolatorModifier colourInterpolatorModifier = new ColourInterpolatorModifier();
			colourInterpolatorModifier.FinalColour = FinalColour;
			colourInterpolatorModifier.InitialColour = InitialColour;
			colourInterpolatorModifier.MiddleColour = MiddleColour;
			colourInterpolatorModifier.MiddlePosition = MiddlePosition;
			return colourInterpolatorModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="elapsedSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particleArray">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float elapsedSeconds, Particle* particle, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Particle* ptr = particle - 1;
				if (particle->Age == ptr->Age)
				{
					particle->Colour.X = ptr->Colour.X;
					particle->Colour.Y = ptr->Colour.Y;
					particle->Colour.Z = ptr->Colour.Z;
				}
				else
				{
					Vector3 vector;
					Vector3 vector2;
					float num;
					if (particle->Age < MiddlePosition)
					{
						vector = InitialColour;
						vector2 = MiddleColour;
						num = particle->Age / MiddlePosition;
					}
					else
					{
						vector = MiddleColour;
						vector2 = FinalColour;
						num = (particle->Age - MiddlePosition) / (1f - MiddlePosition);
					}
					particle->Colour.X = vector.X + (vector2.X - vector.X) * num;
					particle->Colour.Y = vector.Y + (vector2.Y - vector.Y) * num;
					particle->Colour.Z = vector.Z + (vector2.Z - vector.Z) * num;
				}
				particle++;
			}
		}
	}
}
