using System;
using Microsoft.Xna.Framework;

namespace Krypton
{
	public class ShadowHull
	{
		public Vector2 Position;

		public float Angle;

		public float MaxRadius;

		public ShadowHullPoint[] Points;

		public int NumPoints;

		public int[] Indicies;

		public int NumIndicies;

		public bool Visible = true;

		public Vector2 Scale = Vector2.One;

		private ShadowHull()
		{
		}

		public static ShadowHull CreateRectangle(Vector2 size)
		{
			ShadowHull shadowHull = new ShadowHull();
			size *= 0.5f;
			shadowHull.MaxRadius = (float)Math.Sqrt(size.X * size.X + size.Y * size.Y);
			shadowHull.NumPoints = 8;
			int num = shadowHull.NumPoints - 2;
			shadowHull.NumIndicies = num * 3;
			shadowHull.Points = new ShadowHullPoint[shadowHull.NumPoints];
			shadowHull.Indicies = new int[shadowHull.NumIndicies];
			Vector2 position = new Vector2(size.X, size.Y);
			Vector2 position2 = new Vector2(size.X, 0f - size.Y);
			Vector2 position3 = new Vector2(0f - size.X, 0f - size.Y);
			Vector2 position4 = new Vector2(0f - size.X, size.Y);
			ref ShadowHullPoint reference = ref shadowHull.Points[0];
			reference = new ShadowHullPoint(position, Vector2.UnitX);
			ref ShadowHullPoint reference2 = ref shadowHull.Points[1];
			reference2 = new ShadowHullPoint(position2, Vector2.UnitX);
			ref ShadowHullPoint reference3 = ref shadowHull.Points[2];
			reference3 = new ShadowHullPoint(position2, -Vector2.UnitY);
			ref ShadowHullPoint reference4 = ref shadowHull.Points[3];
			reference4 = new ShadowHullPoint(position3, -Vector2.UnitY);
			ref ShadowHullPoint reference5 = ref shadowHull.Points[4];
			reference5 = new ShadowHullPoint(position3, -Vector2.UnitX);
			ref ShadowHullPoint reference6 = ref shadowHull.Points[5];
			reference6 = new ShadowHullPoint(position4, -Vector2.UnitX);
			ref ShadowHullPoint reference7 = ref shadowHull.Points[6];
			reference7 = new ShadowHullPoint(position4, Vector2.UnitY);
			ref ShadowHullPoint reference8 = ref shadowHull.Points[7];
			reference8 = new ShadowHullPoint(position, Vector2.UnitY);
			for (int i = 0; i < num; i++)
			{
				shadowHull.Indicies[i * 3] = 0;
				shadowHull.Indicies[i * 3 + 1] = i + 1;
				shadowHull.Indicies[i * 3 + 2] = i + 2;
			}
			return shadowHull;
		}

		public static ShadowHull CreateCircle(float radius, int sides)
		{
			if (sides < 3)
			{
				throw new ArgumentException("Shadow hull must have at least 3 sides.");
			}
			ShadowHull shadowHull = new ShadowHull();
			shadowHull.MaxRadius = radius;
			shadowHull.NumPoints = sides * 2;
			int num = shadowHull.NumPoints - 2;
			shadowHull.NumIndicies = num * 3;
			shadowHull.Points = new ShadowHullPoint[shadowHull.NumPoints];
			shadowHull.Indicies = new int[shadowHull.NumIndicies];
			float num2 = (float)Math.PI * -2f / (float)sides;
			float num3 = num2 / 2f;
			for (int i = 0; i < sides; i++)
			{
				ShadowHullPoint shadowHullPoint = default(ShadowHullPoint);
				ShadowHullPoint shadowHullPoint2 = default(ShadowHullPoint);
				shadowHullPoint.Position.X = (float)Math.Cos(num2 * (float)i) * radius;
				shadowHullPoint.Position.Y = (float)Math.Sin(num2 * (float)i) * radius;
				shadowHullPoint2.Position.X = (float)Math.Cos(num2 * (float)(i + 1)) * radius;
				shadowHullPoint2.Position.Y = (float)Math.Sin(num2 * (float)(i + 1)) * radius;
				shadowHullPoint.Normal.X = (float)Math.Cos(num2 * (float)i + num3);
				shadowHullPoint.Normal.Y = (float)Math.Sin(num2 * (float)i + num3);
				shadowHullPoint2.Normal.X = (float)Math.Cos(num2 * (float)i + num3);
				shadowHullPoint2.Normal.Y = (float)Math.Sin(num2 * (float)i + num3);
				shadowHull.Points[i * 2] = shadowHullPoint;
				shadowHull.Points[i * 2 + 1] = shadowHullPoint2;
			}
			for (int j = 0; j < num; j++)
			{
				shadowHull.Indicies[j * 3] = 0;
				shadowHull.Indicies[j * 3 + 1] = j + 1;
				shadowHull.Indicies[j * 3 + 2] = j + 2;
			}
			return shadowHull;
		}

		public static ShadowHull CreateConvex(ref Vector2[] points)
		{
			if (points == null)
			{
				throw new ArgumentNullException("Points cannot be null.");
			}
			if (points.Length < 3)
			{
				throw new ArgumentException("Need at least 3 points to create shadow hull.");
			}
			int num = points.Length;
			ShadowHull shadowHull = new ShadowHull();
			shadowHull.NumPoints = num * 2;
			int num2 = shadowHull.NumPoints - 2;
			shadowHull.NumIndicies = num2 * 3;
			shadowHull.Points = new ShadowHullPoint[shadowHull.NumPoints];
			shadowHull.Indicies = new int[shadowHull.NumIndicies];
			_ = points[0];
			_ = points[0];
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = points[i % num];
				Vector2 vector2 = points[(i + 1) % num];
				shadowHull.MaxRadius = Math.Max(shadowHull.MaxRadius, vector.Length());
				Vector2 vector3 = vector2 - vector;
				Vector2 normal = new Vector2(0f - vector3.Y, vector3.X);
				normal.Normalize();
				ref ShadowHullPoint reference = ref shadowHull.Points[i * 2];
				reference = new ShadowHullPoint(vector, normal);
				ref ShadowHullPoint reference2 = ref shadowHull.Points[i * 2 + 1];
				reference2 = new ShadowHullPoint(vector2, normal);
			}
			for (int j = 0; j < num2; j++)
			{
				shadowHull.Indicies[j * 3] = 0;
				shadowHull.Indicies[j * 3 + 1] = j + 1;
				shadowHull.Indicies[j * 3 + 2] = j + 2;
			}
			return shadowHull;
		}
	}
}
