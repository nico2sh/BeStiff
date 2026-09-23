using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines an Emitter which releases particles in a beam which gradually becomes wider.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.ConeEmitterTypeConverter, ProjectMercury.Design")]
	public class ConeEmitter : Emitter
	{
		private float _direction;

		private float HalfConeAngle;

		private Vector2 ConeExtents;

		/// <summary>
		/// The angle (in radians) that the ConeEmitters beam is facing.
		/// </summary>
		public float Direction
		{
			get
			{
				return _direction;
			}
			set
			{
				_direction = value;
				CalculateConeExtents();
			}
		}

		/// <summary>
		/// The angle (in radians) from edge to edge of the ConeEmitters beam.
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the specified value is either
		/// too small or too large.</exception>
		public float ConeAngle
		{
			get
			{
				return HalfConeAngle + HalfConeAngle;
			}
			set
			{
				HalfConeAngle = value * 0.5f;
				CalculateConeExtents();
			}
		}

		/// <summary>
		/// Calculates the extents of the cone based on the direction and cone angle.
		/// </summary>
		private void CalculateConeExtents()
		{
			ConeExtents = new Vector2
			{
				X = Direction - HalfConeAngle,
				Y = Direction + HalfConeAngle
			};
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			ConeEmitter coneEmitter = new ConeEmitter();
			coneEmitter.ConeAngle = ConeAngle;
			coneEmitter.Direction = Direction;
			Emitter emitter = coneEmitter;
			CopyBaseFields(emitter);
			return emitter;
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected override void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			offset = Vector2.Zero;
			float value = RandomHelper.NextFloat(ConeExtents.X, ConeExtents.Y);
			force = new Vector2
			{
				X = Calculator.Cos(value),
				Y = Calculator.Sin(value)
			};
		}
	}
}
