using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	/// <summary>
	/// Emits particles in the shape of a polygon defined with the Points property.
	/// </summary>
	[TypeConverter("ProjectMercury.Design.Emitters.PolygonEmitterTypeConverter, ProjectMercury.Design")]
	public class PolygonEmitter : Emitter
	{
		/// <summary>
		/// Gets or sets a value indicating wether or not the polygon should be closed.
		/// </summary>
		public bool Close;

		private float AngleCos = 1f;

		private float AngleSin;

		private float _scale;

		/// <summary>
		/// Polygon points.
		/// </summary>
		public PolygonPointCollection Points { get; set; }

		/// <summary>
		/// Gets or sets the origin of the point collection.
		/// </summary>
		/// <value>The origin.</value>
		public PolygonOrigin Origin
		{
			get
			{
				return Points.Origin;
			}
			set
			{
				Points.Origin = value;
			}
		}

		/// <summary>
		/// Polygon rotation.
		/// </summary>
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
		/// Polygon scale.
		/// </summary>
		public float Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ProjectMercury.Emitters.PolygonEmitter" /> class.
		/// </summary>
		public PolygonEmitter()
		{
			Close = true;
			Points = new PolygonPointCollection();
			Scale = 1f;
		}

		/// <summary>
		/// Returns an unitialised deep copy of the Emitter.
		/// </summary>
		/// <returns>A deep copy of the Emitter.</returns>
		public override Emitter DeepCopy()
		{
			PolygonEmitter polygonEmitter = new PolygonEmitter();
			polygonEmitter.Close = Close;
			polygonEmitter.Origin = Origin;
			polygonEmitter.Points = new PolygonPointCollection();
			polygonEmitter.Rotation = Rotation;
			polygonEmitter.Scale = Scale;
			PolygonEmitter polygonEmitter2 = polygonEmitter;
			polygonEmitter2.Points.AddRange(Points);
			CopyBaseFields(polygonEmitter2);
			return polygonEmitter2;
		}

		/// <summary>
		/// Generates an offset vector and force vector for a Particle when it is released.
		/// </summary>
		/// <param name="offset">The offset of the Particle from the trigger location.</param>
		/// <param name="force">A unit vector defining the initial force of the Particle.</param>
		protected override void GenerateOffsetAndForce(out Vector2 offset, out Vector2 force)
		{
			if (Points.Count == 0)
			{
				offset = Vector2.Zero;
			}
			else if (Points.Count == 1)
			{
				offset = Points[0];
			}
			else if (Points.Count == 2)
			{
				Vector2 vector = Points[0];
				Vector2 vector2 = Points[1];
				float num = RandomHelper.NextFloat();
				offset.X = vector.X + (vector2.X - vector.X) * num;
				offset.Y = vector.Y + (vector2.Y - vector.Y) * num;
			}
			else
			{
				int num2 = (Close ? RandomHelper.NextInt(0, Points.Count) : RandomHelper.NextInt(0, Points.Count - 1));
				Vector2 vector3 = Points[num2];
				Vector2 vector4 = Points[(num2 + 1) % Points.Count];
				float num3 = RandomHelper.NextFloat();
				offset.X = vector3.X + (vector4.X - vector3.X) * num3;
				offset.Y = vector3.Y + (vector4.Y - vector3.Y) * num3;
			}
			offset.X *= Scale;
			offset.Y *= Scale;
			Vector2 vector5 = offset;
			offset.X = vector5.X * AngleCos + vector5.Y * (0f - AngleSin);
			offset.Y = vector5.X * AngleSin + vector5.Y * AngleCos;
			offset.X += Points.TranslationOffset.X;
			offset.Y += Points.TranslationOffset.Y;
			force = RandomHelper.NextUnitVector();
		}
	}
}
