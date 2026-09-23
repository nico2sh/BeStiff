using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Defines an Emitter which releases particles in a rectangle shape.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.RectEmitterTypeConverter, ProjectMercury.Design")]
	public class RectEmitter : Emitter
	{
		private float HalfWidth;

		private float HalfHeight;

		private float AngleCos = Calculator.Cos(0f);

		private float AngleSin = Calculator.Sin(0f);

		/// <summary>
		/// True if the Particles should be released only from the edge of the rectangle, else false.
		/// </summary>
		public bool Frame;

		/// <summary>
		/// Gets or sets the width of the rectangle.
		/// </summary>
		/// <value>The width of the rectangle.</value>
		public float Width
		{
			get
			{
				return HalfWidth + HalfWidth;
			}
			set
			{
				HalfWidth = value * 0.5f;
			}
		}

		/// <summary>
		/// Gets or sets the height of the rectangle..
		/// </summary>
		/// <value>The height of the rectangle.</value>
		public float Height
		{
			get
			{
				return HalfHeight + HalfHeight;
			}
			set
			{
				HalfHeight = value * 0.5f;
			}
		}

		/// <summary>
		/// Gets or sets the rotation of the rectangle.
		/// </summary>
		/// <value>The rotation of the rectangle measured in radians.</value>
		public float Rotation
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
			RectEmitter rectEmitter = new RectEmitter();
			rectEmitter.Frame = Frame;
			rectEmitter.Height = Height;
			rectEmitter.Rotation = Rotation;
			rectEmitter.Width = Width;
			Emitter emitter = rectEmitter;
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
			if (Frame)
			{
				if (RandomHelper.NextBool())
				{
					offset.X = RandomHelper.ChooseOne(0f - HalfWidth, HalfWidth);
					offset.Y = RandomHelper.NextFloat(0f - HalfHeight, HalfHeight);
				}
				else
				{
					offset.X = RandomHelper.NextFloat(0f - HalfWidth, HalfWidth);
					offset.Y = RandomHelper.ChooseOne(0f - HalfHeight, HalfHeight);
				}
			}
			else
			{
				offset.X = RandomHelper.NextFloat(0f - HalfWidth, HalfWidth);
				offset.Y = RandomHelper.NextFloat(0f - HalfHeight, HalfHeight);
			}
			Vector2 vector = offset;
			offset.X = vector.X * AngleCos + vector.Y * (0f - AngleSin);
			offset.Y = vector.X * AngleSin + vector.Y * AngleCos;
			force = RandomHelper.NextUnitVector();
		}
	}
}
