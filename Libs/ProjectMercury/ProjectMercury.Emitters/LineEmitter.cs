using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines an Emitter which releases Particles at a random point along a line.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.LineEmitterTypeConverter, ProjectMercury.Design")]
	public class LineEmitter : Emitter
	{
		private float HalfLength;

		private float AngleCos = Calculator.Cos(0f);

		private float AngleSin = Calculator.Sin(0f);

		/// <summary>
		/// If true, will emit particles perpendicular to the angle of the line.
		/// </summary>
		public bool Rectilinear;

		/// <summary>
		/// If true, will emit particles both ways. Only work when Rectilinear is enabled.
		/// </summary>
		public bool EmitBothWays;

		/// <summary>
		/// Gets or sets the length of the line.
		/// </summary>
		public float Length
		{
			get
			{
				return HalfLength + HalfLength;
			}
			set
			{
				HalfLength = value * 0.5f;
			}
		}

		/// <summary>
		/// Gets or sets the rotation of the line around its middle point.
		/// </summary>
		public float Angle
		{
			get
			{
				return Calculator.Atan2(AngleSin, AngleCos);
			}
			set
			{
				AngleCos = Calculator.Cos(value);
				AngleSin = Calculator.Sin(value);
			}
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			LineEmitter lineEmitter = new LineEmitter();
			lineEmitter.Angle = Angle;
			lineEmitter.Length = Length;
			lineEmitter.Rectilinear = Rectilinear;
			lineEmitter.EmitBothWays = EmitBothWays;
			Emitter emitter = lineEmitter;
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
			float num = RandomHelper.NextFloat(0f - HalfLength, HalfLength);
			offset = new Vector2
			{
				X = num * AngleCos,
				Y = num * AngleSin
			};
			if (Rectilinear)
			{
				force = new Vector2
				{
					X = AngleSin,
					Y = 0f - AngleCos
				};
				if (EmitBothWays && RandomHelper.NextBool())
				{
					force.X *= -1f;
					force.Y *= -1f;
				}
			}
			else
			{
				force = RandomHelper.NextUnitVector();
			}
		}
	}
}
