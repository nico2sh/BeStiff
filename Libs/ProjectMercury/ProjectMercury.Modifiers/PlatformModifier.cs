using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a Modifier which freezes a particle when it comes into contact with a bounding box.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Modifiers.PlatformModifierTypeConverter, ProjectMercury.Design")]
	public sealed class PlatformModifier : Modifier
	{
		/// <summary>
		/// The list of platforms.
		/// </summary>
		public List<BoundingBox> Platforms { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.Modifiers.PlatformModifier" /> class.
		/// </summary>
		public PlatformModifier()
		{
			Platforms = new List<BoundingBox>();
		}

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			PlatformModifier platformModifier = new PlatformModifier();
			platformModifier.Platforms.AddRange(Platforms);
			return platformModifier;
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
				Vector3 point = new Vector3(ptr->Position, 0f);
				for (int j = 0; j < Platforms.Count; j++)
				{
					Platforms[j].Contains(ref point, out var result);
					if (result == ContainmentType.Contains)
					{
						ptr->Momentum = Vector2.Zero;
						return;
					}
				}
			}
		}
	}
}
