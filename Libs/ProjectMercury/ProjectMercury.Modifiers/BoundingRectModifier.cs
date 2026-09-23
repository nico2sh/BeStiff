using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	/// <summary>
	/// Defines a modifier which maintains a bounding rectangle around an Emitter.
	/// </summary>
	public sealed class BoundingRectModifier : Modifier
	{
		/// <summary>
		/// Gets a bounding rectangle which encompasses each of the particles of an emitter.
		/// </summary>
		public BoundingRect BoundingRect { get; private set; }

		/// <summary>
		/// Gets or sets a padding value to add to the bounding rectangle.
		/// </summary>
		public float Padding { get; set; }

		/// <summary>
		/// Returns a deep copy of the Modifier implementation.
		/// </summary>
		/// <returns></returns>
		public override Modifier DeepCopy()
		{
			BoundingRectModifier boundingRectModifier = new BoundingRectModifier();
			boundingRectModifier.Padding = Padding;
			return boundingRectModifier;
		}

		/// <summary>
		/// Processes the particles.
		/// </summary>
		/// <param name="elapsedSeconds">Elapsed time in whole and fractional seconds.</param>
		/// <param name="particle">A pointer to the first particle in an array of particles.</param>
		/// <param name="count">The number of particles which need to be processed.</param>
		protected internal unsafe override void Process(float elapsedSeconds, Particle* particle, int count)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < count; i++)
			{
				num = ((particle->Position.X < num) ? particle->Position.X : num);
				num3 = ((particle->Position.X > num3) ? particle->Position.X : num3);
				num2 = ((particle->Position.Y < num2) ? particle->Position.Y : num2);
				num4 = ((particle->Position.Y > num4) ? particle->Position.Y : num4);
				particle++;
			}
			BoundingRect = new BoundingRect
			{
				Min = new Vector2
				{
					X = num - Padding,
					Y = num2 - Padding
				},
				Max = new Vector2
				{
					X = num3 + Padding,
					Y = num4 + Padding
				}
			};
		}
	}
}
