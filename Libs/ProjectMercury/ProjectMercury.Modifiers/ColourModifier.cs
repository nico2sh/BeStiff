using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which gradually changes the colour of a Particle over the course of its lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ColourModifierTypeConverter, ProjectMercury.Design")]
	public sealed class ColourModifier : Modifier
	{
		/// <summary>
		/// The initial colour of Particles when they are released.
		/// </summary>
		public Vector3 InitialColour;

		/// <summary>
		/// The ultimate colour of Particles when they are retired.
		/// </summary>
		public Vector3 UltimateColour;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ColourModifier colourModifier = new ColourModifier();
			colourModifier.InitialColour = InitialColour;
			colourModifier.UltimateColour = UltimateColour;
			return colourModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="elapsedSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particle">A pointer to the first item in an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float elapsedSeconds, Particle* particle, int count)
		{
			particle->Colour.X = InitialColour.X + (UltimateColour.X - InitialColour.X) * particle->Age;
			particle->Colour.Y = InitialColour.Y + (UltimateColour.Y - InitialColour.Y) * particle->Age;
			particle->Colour.Z = InitialColour.Z + (UltimateColour.Z - InitialColour.Z) * particle->Age;
			Particle* ptr = particle;
			particle++;
			for (int i = 1; i < count; i++)
			{
				if (particle->Age < ptr->Age)
				{
					particle->Colour.X = InitialColour.X + (UltimateColour.X - InitialColour.X) * particle->Age;
					particle->Colour.Y = InitialColour.Y + (UltimateColour.Y - InitialColour.Y) * particle->Age;
					particle->Colour.Z = InitialColour.Z + (UltimateColour.Z - InitialColour.Z) * particle->Age;
				}
				else
				{
					particle->Colour.X = ptr->Colour.X;
					particle->Colour.Y = ptr->Colour.Y;
					particle->Colour.Z = ptr->Colour.Z;
				}
				ptr++;
				particle++;
			}
		}
	}
}
