using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which merges the colour of particles towards a single colour over their lifetime. Works best
	/// when Particles are being released with random colours, where you require the particles to have a uniform colour
	/// at the end of their lifetime.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.ColourMergeModifierTypeConverter, ProjectMercury.Design")]
	public sealed class ColourMergeModifier : Modifier
	{
		/// <summary>
		/// The final colour of Particles when they are retired.
		/// </summary>
		public Vector3 MergeColour;

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			ColourMergeModifier colourMergeModifier = new ColourMergeModifier();
			colourMergeModifier.MergeColour = MergeColour;
			return colourMergeModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="elapsedSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particle">A pointer to an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float elapsedSeconds, Particle* particle, int count)
		{
			for (int i = 0; i < count; i++)
			{
				float num = particle->Age * 0.07f;
				float num2 = 1f - num;
				particle->Colour.X = particle->Colour.X * num2 + MergeColour.X * num;
				particle->Colour.Y = particle->Colour.Y * num2 + MergeColour.Y * num;
				particle->Colour.Z = particle->Colour.Z * num2 + MergeColour.Z * num;
				particle++;
			}
		}
	}
}
