using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public static class PolygonTools
	{
		/// <summary>
		/// Build vertices to represent an axis-aligned box.
		/// </summary>
		/// <param name="hx">the half-width.</param>
		/// <param name="hy">the half-height.</param>
		public static Vertices CreateRectangle(float hx, float hy)
		{
			Vertices vertices = new Vertices(4);
			vertices.Add(new Vector2(0f - hx, 0f - hy));
			vertices.Add(new Vector2(hx, 0f - hy));
			vertices.Add(new Vector2(hx, hy));
			vertices.Add(new Vector2(0f - hx, hy));
			return vertices;
		}

		/// <summary>
		/// Build vertices to represent an oriented box.
		/// </summary>
		/// <param name="hx">the half-width.</param>
		/// <param name="hy">the half-height.</param>
		/// <param name="center">the center of the box in local coordinates.</param>
		/// <param name="angle">the rotation of the box in local coordinates.</param>
		public static Vertices CreateRectangle(float hx, float hy, Vector2 center, float angle)
		{
			Vertices vertices = CreateRectangle(hx, hy);
			Transform T = default(Transform);
			T.Position = center;
			T.R.Set(angle);
			for (int i = 0; i < 4; i++)
			{
				vertices[i] = MathUtils.Multiply(ref T, vertices[i]);
			}
			return vertices;
		}

		/// <summary>
		/// Creates a rounded rectangle with the specified width and height.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="xRadius">The rounding X radius.</param>
		/// <param name="yRadius">The rounding Y radius.</param>
		/// <param name="segments">The number of segments to subdivide the edges.</param>
		/// <returns></returns>
		public static Vertices CreateRoundedRectangle(float width, float height, float xRadius, float yRadius, int segments)
		{
			if (yRadius > height / 2f || xRadius > width / 2f)
			{
				throw new Exception("Rounding amount can't be more than half the height and width respectively.");
			}
			if (segments < 0)
			{
				throw new Exception("Segments must be zero or more.");
			}
			Vertices vertices = new Vertices();
			if (segments == 0)
			{
				vertices.Add(new Vector2(width * 0.5f - xRadius, (0f - height) * 0.5f));
				vertices.Add(new Vector2(width * 0.5f, (0f - height) * 0.5f + yRadius));
				vertices.Add(new Vector2(width * 0.5f, height * 0.5f - yRadius));
				vertices.Add(new Vector2(width * 0.5f - xRadius, height * 0.5f));
				vertices.Add(new Vector2((0f - width) * 0.5f + xRadius, height * 0.5f));
				vertices.Add(new Vector2((0f - width) * 0.5f, height * 0.5f - yRadius));
				vertices.Add(new Vector2((0f - width) * 0.5f, (0f - height) * 0.5f + yRadius));
				vertices.Add(new Vector2((0f - width) * 0.5f + xRadius, (0f - height) * 0.5f));
			}
			else
			{
				int num = segments * 4 + 8;
				float num2 = (float)Math.PI * 2f / (float)(num - 4);
				int num3 = num / 4;
				Vector2 vector = new Vector2(width / 2f - xRadius, height / 2f - yRadius);
				vertices.Add(vector + new Vector2(xRadius, 0f - yRadius + yRadius));
				short num4 = 0;
				for (int i = 1; i < num; i++)
				{
					if (i - num3 == 0 || i - num3 * 3 == 0)
					{
						vector.X *= -1f;
						num4--;
					}
					else if (i - num3 * 2 == 0)
					{
						vector.Y *= -1f;
						num4--;
					}
					vertices.Add(vector + new Vector2(xRadius * (float)Math.Cos(num2 * (float)(-(i + num4))), (0f - yRadius) * (float)Math.Sin(num2 * (float)(-(i + num4)))));
				}
			}
			return vertices;
		}

		/// <summary>
		/// Set this as a single edge.
		/// </summary>
		/// <param name="start">The first point.</param>
		/// <param name="end">The second point.</param>
		public static Vertices CreateLine(Vector2 start, Vector2 end)
		{
			Vertices vertices = new Vertices(2);
			vertices.Add(start);
			vertices.Add(end);
			return vertices;
		}

		/// <summary>
		/// Creates a circle with the specified radius and number of edges.
		/// </summary>
		/// <param name="radius">The radius.</param>
		/// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles a circle</param>
		/// <returns></returns>
		public static Vertices CreateCircle(float radius, int numberOfEdges)
		{
			return CreateEllipse(radius, radius, numberOfEdges);
		}

		/// <summary>
		/// Creates a ellipse with the specified width, height and number of edges.
		/// </summary>
		/// <param name="xRadius">Width of the ellipse.</param>
		/// <param name="yRadius">Height of the ellipse.</param>
		/// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles an ellipse</param>
		/// <returns></returns>
		public static Vertices CreateEllipse(float xRadius, float yRadius, int numberOfEdges)
		{
			Vertices vertices = new Vertices();
			float num = (float)Math.PI * 2f / (float)numberOfEdges;
			vertices.Add(new Vector2(xRadius, 0f));
			for (int num2 = numberOfEdges - 1; num2 > 0; num2--)
			{
				vertices.Add(new Vector2(xRadius * (float)Math.Cos(num * (float)num2), (0f - yRadius) * (float)Math.Sin(num * (float)num2)));
			}
			return vertices;
		}

		public static Vertices CreateArc(float radians, int sides, float radius)
		{
			Vertices vertices = new Vertices();
			float num = radians / (float)sides;
			for (int num2 = sides - 1; num2 > 0; num2--)
			{
				vertices.Add(new Vector2(radius * (float)Math.Cos(num * (float)num2), radius * (float)Math.Sin(num * (float)num2)));
			}
			return vertices;
		}

		/// <summary>
		/// Creates an capsule with the specified height, radius and number of edges.
		/// A capsule has the same form as a pill capsule.
		/// </summary>
		/// <param name="height">Height (inner height + 2 * radius) of the capsule.</param>
		/// <param name="endRadius">Radius of the capsule ends.</param>
		/// <param name="edges">The number of edges of the capsule ends. The more edges, the more it resembles an capsule</param>
		/// <returns></returns>
		public static Vertices CreateCapsule(float height, float endRadius, int edges)
		{
			if (endRadius >= height / 2f)
			{
				throw new ArgumentException("The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.", "endRadius");
			}
			return CreateCapsule(height, endRadius, edges, endRadius, edges);
		}

		/// <summary>
		/// Creates an capsule with the specified  height, radius and number of edges.
		/// A capsule has the same form as a pill capsule.
		/// </summary>
		/// <param name="height">Height (inner height + radii) of the capsule.</param>
		/// <param name="topRadius">Radius of the top.</param>
		/// <param name="topEdges">The number of edges of the top. The more edges, the more it resembles an capsule</param>
		/// <param name="bottomRadius">Radius of bottom.</param>
		/// <param name="bottomEdges">The number of edges of the bottom. The more edges, the more it resembles an capsule</param>
		/// <returns></returns>
		public static Vertices CreateCapsule(float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges)
		{
			if (height <= 0f)
			{
				throw new ArgumentException("Height must be longer than 0", "height");
			}
			if (topRadius <= 0f)
			{
				throw new ArgumentException("The top radius must be more than 0", "topRadius");
			}
			if (topEdges <= 0)
			{
				throw new ArgumentException("Top edges must be more than 0", "topEdges");
			}
			if (bottomRadius <= 0f)
			{
				throw new ArgumentException("The bottom radius must be more than 0", "bottomRadius");
			}
			if (bottomEdges <= 0)
			{
				throw new ArgumentException("Bottom edges must be more than 0", "bottomEdges");
			}
			if (topRadius >= height / 2f)
			{
				throw new ArgumentException("The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.", "topRadius");
			}
			if (bottomRadius >= height / 2f)
			{
				throw new ArgumentException("The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.", "bottomRadius");
			}
			Vertices vertices = new Vertices();
			float num = (height - topRadius - bottomRadius) * 0.5f;
			vertices.Add(new Vector2(topRadius, num));
			float num2 = (float)Math.PI / (float)topEdges;
			for (int i = 1; i < topEdges; i++)
			{
				vertices.Add(new Vector2(topRadius * (float)Math.Cos(num2 * (float)i), topRadius * (float)Math.Sin(num2 * (float)i) + num));
			}
			vertices.Add(new Vector2(0f - topRadius, num));
			vertices.Add(new Vector2(0f - bottomRadius, 0f - num));
			num2 = (float)Math.PI / (float)bottomEdges;
			for (int j = 1; j < bottomEdges; j++)
			{
				vertices.Add(new Vector2((0f - bottomRadius) * (float)Math.Cos(num2 * (float)j), (0f - bottomRadius) * (float)Math.Sin(num2 * (float)j) - num));
			}
			vertices.Add(new Vector2(bottomRadius, 0f - num));
			return vertices;
		}

		/// <summary>
		/// Creates a gear shape with the specified radius and number of teeth.
		/// </summary>
		/// <param name="radius">The radius.</param>
		/// <param name="numberOfTeeth">The number of teeth.</param>
		/// <param name="tipPercentage">The tip percentage.</param>
		/// <param name="toothHeight">Height of the tooth.</param>
		/// <returns></returns>
		public static Vertices CreateGear(float radius, int numberOfTeeth, float tipPercentage, float toothHeight)
		{
			Vertices vertices = new Vertices();
			float num = (float)Math.PI * 2f / (float)numberOfTeeth;
			tipPercentage /= 100f;
			MathHelper.Clamp(tipPercentage, 0f, 1f);
			float num2 = num / 2f * tipPercentage;
			float num3 = (num - num2 * 2f) / 2f;
			for (int num4 = numberOfTeeth - 1; num4 >= 0; num4--)
			{
				if (num2 > 0f)
				{
					vertices.Add(new Vector2(radius * (float)Math.Cos(num * (float)num4 + num3 * 2f + num2), (0f - radius) * (float)Math.Sin(num * (float)num4 + num3 * 2f + num2)));
					vertices.Add(new Vector2((radius + toothHeight) * (float)Math.Cos(num * (float)num4 + num3 + num2), (0f - (radius + toothHeight)) * (float)Math.Sin(num * (float)num4 + num3 + num2)));
				}
				vertices.Add(new Vector2((radius + toothHeight) * (float)Math.Cos(num * (float)num4 + num3), (0f - (radius + toothHeight)) * (float)Math.Sin(num * (float)num4 + num3)));
				vertices.Add(new Vector2(radius * (float)Math.Cos(num * (float)num4), (0f - radius) * (float)Math.Sin(num * (float)num4)));
			}
			return vertices;
		}

		/// <summary>
		/// Detects the vertices by analyzing the texture data.
		/// </summary>
		/// <param name="data">The texture data.</param>
		/// <param name="width">The texture width.</param>
		/// <returns></returns>
		public static Vertices CreatePolygon(uint[] data, int width)
		{
			return TextureConverter.DetectVertices(data, width);
		}

		/// <summary>
		/// Detects the vertices by analyzing the texture data.
		/// </summary>
		/// <param name="data">The texture data.</param>
		/// <param name="width">The texture width.</param>
		/// <param name="holeDetection">if set to <c>true</c> it will perform hole detection.</param>
		/// <returns></returns>
		public static Vertices CreatePolygon(uint[] data, int width, bool holeDetection)
		{
			return TextureConverter.DetectVertices(data, width, holeDetection);
		}

		/// <summary>
		/// Detects the vertices by analyzing the texture data.
		/// </summary>
		/// <param name="data">The texture data.</param>
		/// <param name="width">The texture width.</param>
		/// <param name="hullTolerance">The hull tolerance.</param>
		/// <param name="alphaTolerance">The alpha tolerance.</param>
		/// <param name="multiPartDetection">if set to <c>true</c> it will perform multi part detection.</param>
		/// <param name="holeDetection">if set to <c>true</c> it will perform hole detection.</param>
		/// <returns></returns>
		public static List<Vertices> CreatePolygon(uint[] data, int width, float hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
		{
			return TextureConverter.DetectVertices(data, width, hullTolerance, alphaTolerance, multiPartDetection, holeDetection);
		}
	}
}
