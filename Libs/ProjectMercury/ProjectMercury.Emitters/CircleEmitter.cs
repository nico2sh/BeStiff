using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines an Emitter which releases Particles in a circle or ring shape.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.CircleEmitterTypeConverter, ProjectMercury.Design")]
	public class CircleEmitter : Emitter
	{
		private float _radius;

		/// <summary>
		/// True if particles should be spawned only on the edge of the circle, else false.
		/// </summary>
		public bool Ring;

		/// <summary>
		/// True if particles should radiate away from the center of the circle, else false.
		/// </summary>
		public bool Radiate;

		/// <summary>
		/// Defines the radius of the circle.
		/// </summary>
		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
			}
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			CircleEmitter circleEmitter = new CircleEmitter();
			circleEmitter.Radius = Radius;
			circleEmitter.Radiate = Radiate;
			circleEmitter.Ring = Ring;
			CircleEmitter circleEmitter2 = circleEmitter;
			CopyBaseFields(circleEmitter2);
			return circleEmitter2;
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected override void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			Vector2 vector = RandomHelper.NextUnitVector();
			float num = (Ring ? Radius : (Radius * RandomHelper.NextFloat()));
			offset = new Vector2
			{
				X = vector.X * num,
				Y = vector.Y * num
			};
			force = (Radiate ? vector : RandomHelper.NextUnitVector());
		}
	}
}
