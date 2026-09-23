using System;
using System.Collections.Generic;
using FarseerPhysics.Common.PolygonManipulation;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
	/// <summary>
	/// Ported from jBox2D. Original author: ewjordan 
	/// Triangulates a polygon using simple ear-clipping algorithm.
	///
	/// Only works on simple polygons.
	///
	/// Triangles may be degenerate, especially if you have identical points
	/// in the input to the algorithm.  Check this before you use them.
	/// </summary>
	public static class EarclipDecomposer
	{
		private const float Tol = 0.001f;

		/// <summary>
		/// Decomposes a non-convex polygon into a number of convex polygons, up
		/// to maxPolys (remaining pieces are thrown out).
		///
		/// Each resulting polygon will have no more than Settings.MaxPolygonVertices
		/// vertices.
		///
		/// Warning: Only works on simple polygons
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <returns></returns>
		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			return ConvexPartition(vertices, int.MaxValue, 0f);
		}

		/// <summary>
		/// Decomposes a non-convex polygon into a number of convex polygons, up
		/// to maxPolys (remaining pieces are thrown out).
		/// Each resulting polygon will have no more than Settings.MaxPolygonVertices
		/// vertices.
		/// Warning: Only works on simple polygons
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="maxPolys">The maximum number of polygons.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static List<Vertices> ConvexPartition(Vertices vertices, int maxPolys, float tolerance)
		{
			if (vertices.Count < 3)
			{
				List<Vertices> list = new List<Vertices>();
				list.Add(vertices);
				return list;
			}
			List<Triangle> list2;
			if (vertices.IsCounterClockWise())
			{
				Vertices vertices2 = new Vertices(vertices);
				vertices2.Reverse();
				list2 = TriangulatePolygon(vertices2);
			}
			else
			{
				list2 = TriangulatePolygon(vertices);
			}
			if (list2.Count < 1)
			{
				throw new Exception("Can't triangulate your polygon.");
			}
			List<Vertices> list3 = PolygonizeTriangles(list2, maxPolys, tolerance);
			for (int i = 0; i < list3.Count; i++)
			{
				list3[i] = SimplifyTools.CollinearSimplify(list3[i], 0f);
			}
			for (int num = list3.Count - 1; num >= 0; num--)
			{
				if (list3[num].Count == 0)
				{
					list3.RemoveAt(num);
				}
			}
			return list3;
		}

		/// <summary>
		/// Turns a list of triangles into a list of convex polygons. Very simple
		/// method - start with a seed triangle, keep adding triangles to it until
		/// you can't add any more without making the polygon non-convex.
		///
		/// Returns an integer telling how many polygons were created.  Will fill
		/// polys array up to polysLength entries, which may be smaller or larger
		/// than the return value.
		///
		/// Takes O(N///P) where P is the number of resultant polygons, N is triangle
		/// count.
		///
		/// The final polygon list will not necessarily be minimal, though in
		/// practice it works fairly well.
		/// </summary>
		/// <param name="triangulated">The triangulated.</param>
		///             <param name="maxPolys">The maximun number of polygons</param>
		///             <param name="tolerance">The tolerance</param>
		///             <returns></returns>
		public static List<Vertices> PolygonizeTriangles(List<Triangle> triangulated, int maxPolys, float tolerance)
		{
			List<Vertices> list = new List<Vertices>(50);
			int num = 0;
			if (triangulated.Count <= 0)
			{
				return list;
			}
			bool[] array = new bool[triangulated.Count];
			for (int i = 0; i < triangulated.Count; i++)
			{
				array[i] = false;
				if ((triangulated[i].X[0] == triangulated[i].X[1] && triangulated[i].Y[0] == triangulated[i].Y[1]) || (triangulated[i].X[1] == triangulated[i].X[2] && triangulated[i].Y[1] == triangulated[i].Y[2]) || (triangulated[i].X[0] == triangulated[i].X[2] && triangulated[i].Y[0] == triangulated[i].Y[2]))
				{
					array[i] = true;
				}
			}
			bool flag = true;
			while (flag)
			{
				int num2 = -1;
				for (int j = 0; j < triangulated.Count; j++)
				{
					if (!array[j])
					{
						num2 = j;
						break;
					}
				}
				if (num2 == -1)
				{
					flag = false;
					continue;
				}
				Vertices vertices = new Vertices(3);
				for (int k = 0; k < 3; k++)
				{
					vertices.Add(new Vector2(triangulated[num2].X[k], triangulated[num2].Y[k]));
				}
				array[num2] = true;
				int num3 = 0;
				int num4 = 0;
				while (num4 < 2 * triangulated.Count)
				{
					while (num3 >= triangulated.Count)
					{
						num3 -= triangulated.Count;
					}
					if (!array[num3])
					{
						Vertices vertices2 = AddTriangle(triangulated[num3], vertices);
						if (vertices2 != null && vertices2.Count <= Settings.MaxPolygonVertices && vertices2.IsConvex())
						{
							vertices = new Vertices(vertices2);
							array[num3] = true;
						}
					}
					num4++;
					num3++;
				}
				if (num < maxPolys && vertices.Count >= 3)
				{
					list.Add(new Vertices(vertices));
				}
				if (vertices.Count >= 3)
				{
					num++;
				}
			}
			return list;
		}

		/// <summary>
		/// Triangulates a polygon using simple ear-clipping algorithm. Returns
		/// size of Triangle array unless the polygon can't be triangulated.
		/// This should only happen if the polygon self-intersects,
		/// though it will not _always_ return null for a bad polygon - it is the
		/// caller's responsibility to check for self-intersection, and if it
		/// doesn't, it should at least check that the return value is non-null
		/// before using. You're warned!
		///
		/// Triangles may be degenerate, especially if you have identical points
		/// in the input to the algorithm.  Check this before you use them.
		///
		/// This is totally unoptimized, so for large polygons it should not be part
		/// of the simulation loop.
		///
		/// Warning: Only works on simple polygons.
		/// </summary>
		/// <returns></returns>
		public static List<Triangle> TriangulatePolygon(Vertices vertices)
		{
			List<Triangle> list = new List<Triangle>();
			if (vertices.Count < 3)
			{
				return new List<Triangle>();
			}
			Vertices pin = new Vertices(vertices);
			if (ResolvePinchPoint(pin, out var poutA, out var poutB))
			{
				List<Triangle> list2 = TriangulatePolygon(poutA);
				List<Triangle> list3 = TriangulatePolygon(poutB);
				if (list2.Count == -1 || list3.Count == -1)
				{
					throw new Exception("Can't triangulate your polygon.");
				}
				for (int i = 0; i < list2.Count; i++)
				{
					list.Add(new Triangle(list2[i]));
				}
				for (int j = 0; j < list3.Count; j++)
				{
					list.Add(new Triangle(list3[j]));
				}
				return list;
			}
			Triangle[] array = new Triangle[vertices.Count - 2];
			int num = 0;
			float[] array2 = new float[vertices.Count];
			float[] array3 = new float[vertices.Count];
			for (int k = 0; k < vertices.Count; k++)
			{
				array2[k] = vertices[k].X;
				array3[k] = vertices[k].Y;
			}
			int num2 = vertices.Count;
			while (num2 > 3)
			{
				int num3 = -1;
				float num4 = -10f;
				for (int l = 0; l < num2; l++)
				{
					if (IsEar(l, array2, array3, num2))
					{
						int num5 = Remainder(l - 1, num2);
						int num6 = Remainder(l + 1, num2);
						Vector2 a = new Vector2(array2[num6] - array2[l], array3[num6] - array3[l]);
						Vector2 b = new Vector2(array2[l] - array2[num5], array3[l] - array3[num5]);
						Vector2 b2 = new Vector2(array2[num5] - array2[num6], array3[num5] - array3[num6]);
						a.Normalize();
						b.Normalize();
						b2.Normalize();
						MathUtils.Cross(ref a, ref b, out var c);
						c = Math.Abs(c);
						MathUtils.Cross(ref b, ref b2, out var c2);
						c2 = Math.Abs(c2);
						MathUtils.Cross(ref b2, ref a, out var c3);
						c3 = Math.Abs(c3);
						float num7 = Math.Min(c, Math.Min(c2, c3));
						if (num7 > num4)
						{
							num3 = l;
							num4 = num7;
						}
					}
				}
				if (num3 == -1)
				{
					for (int m = 0; m < num; m++)
					{
						list.Add(new Triangle(array[m]));
					}
					return list;
				}
				num2--;
				float[] array4 = new float[num2];
				float[] array5 = new float[num2];
				int num8 = 0;
				for (int n = 0; n < num2; n++)
				{
					if (num8 == num3)
					{
						num8++;
					}
					array4[n] = array2[num8];
					array5[n] = array3[num8];
					num8++;
				}
				int num9 = ((num3 == 0) ? num2 : (num3 - 1));
				int num10 = ((num3 != num2) ? (num3 + 1) : 0);
				Triangle triangle = new Triangle(array2[num3], array3[num3], array2[num10], array3[num10], array2[num9], array3[num9]);
				array[num] = triangle;
				num++;
				array2 = array4;
				array3 = array5;
			}
			Triangle triangle2 = new Triangle(array2[1], array3[1], array2[2], array3[2], array2[0], array3[0]);
			array[num] = triangle2;
			num++;
			for (int num11 = 0; num11 < num; num11++)
			{
				list.Add(new Triangle(array[num11]));
			}
			return list;
		}

		/// <summary>
		/// Finds and fixes "pinch points," points where two polygon
		/// vertices are at the same point.
		///
		/// If a pinch point is found, pin is broken up into poutA and poutB
		/// and true is returned; otherwise, returns false.
		///
		/// Mostly for internal use.
		///
		/// O(N^2) time, which sucks...
		/// </summary>
		/// <param name="pin">The pin.</param>
		/// <param name="poutA">The pout A.</param>
		/// <param name="poutB">The pout B.</param>
		/// <returns></returns>
		private static bool ResolvePinchPoint(Vertices pin, out Vertices poutA, out Vertices poutB)
		{
			poutA = new Vertices();
			poutB = new Vertices();
			if (pin.Count < 3)
			{
				return false;
			}
			bool flag = false;
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < pin.Count; i++)
			{
				for (int j = i + 1; j < pin.Count; j++)
				{
					if (Math.Abs(pin[i].X - pin[j].X) < 0.001f && Math.Abs(pin[i].Y - pin[j].Y) < 0.001f && j != i + 1)
					{
						num = i;
						num2 = j;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				int num3 = num2 - num;
				if (num3 == pin.Count)
				{
					return false;
				}
				for (int k = 0; k < num3; k++)
				{
					int index = Remainder(num + k, pin.Count);
					poutA.Add(pin[index]);
				}
				int num4 = pin.Count - num3;
				for (int l = 0; l < num4; l++)
				{
					int index2 = Remainder(num2 + l, pin.Count);
					poutB.Add(pin[index2]);
				}
			}
			return flag;
		}

		/// <summary>
		/// Fix for obnoxious behavior for the % operator for negative numbers...
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="modulus">The modulus.</param>
		/// <returns></returns>
		private static int Remainder(int x, int modulus)
		{
			int i;
			for (i = x % modulus; i < 0; i += modulus)
			{
			}
			return i;
		}

		private static Vertices AddTriangle(Triangle t, Vertices vertices)
		{
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (t.X[0] == vertices[i].X && t.Y[0] == vertices[i].Y)
				{
					if (num == -1)
					{
						num = i;
						num2 = 0;
					}
					else
					{
						num3 = i;
						num4 = 0;
					}
				}
				else if (t.X[1] == vertices[i].X && t.Y[1] == vertices[i].Y)
				{
					if (num == -1)
					{
						num = i;
						num2 = 1;
					}
					else
					{
						num3 = i;
						num4 = 1;
					}
				}
				else if (t.X[2] == vertices[i].X && t.Y[2] == vertices[i].Y)
				{
					if (num == -1)
					{
						num = i;
						num2 = 2;
					}
					else
					{
						num3 = i;
						num4 = 2;
					}
				}
			}
			if (num == 0 && num3 == vertices.Count - 1)
			{
				num = vertices.Count - 1;
				num3 = 0;
			}
			if (num3 == -1)
			{
				return null;
			}
			int num5 = 0;
			if (num5 == num2 || num5 == num4)
			{
				num5 = 1;
			}
			if (num5 == num2 || num5 == num4)
			{
				num5 = 2;
			}
			Vertices vertices2 = new Vertices(vertices.Count + 1);
			for (int j = 0; j < vertices.Count; j++)
			{
				vertices2.Add(vertices[j]);
				if (j == num)
				{
					vertices2.Add(new Vector2(t.X[num5], t.Y[num5]));
				}
			}
			return vertices2;
		}

		/// <summary>
		/// Checks if vertex i is the tip of an ear in polygon defined by xv[] and
		/// yv[].
		///
		/// Assumes clockwise orientation of polygon...ick
		/// </summary>
		/// <param name="i">The i.</param>
		/// <param name="xv">The xv.</param>
		/// <param name="yv">The yv.</param>
		/// <param name="xvLength">Length of the xv.</param>
		/// <returns>
		/// 	<c>true</c> if the specified i is ear; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsEar(int i, float[] xv, float[] yv, int xvLength)
		{
			if (i >= xvLength || i < 0 || xvLength < 3)
			{
				return false;
			}
			int num = i + 1;
			int num2 = i - 1;
			float num3;
			float num4;
			float num5;
			float num6;
			if (i == 0)
			{
				num3 = xv[0] - xv[xvLength - 1];
				num4 = yv[0] - yv[xvLength - 1];
				num5 = xv[1] - xv[0];
				num6 = yv[1] - yv[0];
				num2 = xvLength - 1;
			}
			else if (i == xvLength - 1)
			{
				num3 = xv[i] - xv[i - 1];
				num4 = yv[i] - yv[i - 1];
				num5 = xv[0] - xv[i];
				num6 = yv[0] - yv[i];
				num = 0;
			}
			else
			{
				num3 = xv[i] - xv[i - 1];
				num4 = yv[i] - yv[i - 1];
				num5 = xv[i + 1] - xv[i];
				num6 = yv[i + 1] - yv[i];
			}
			float num7 = num3 * num6 - num5 * num4;
			if (num7 > 0f)
			{
				return false;
			}
			Triangle triangle = new Triangle(xv[i], yv[i], xv[num], yv[num], xv[num2], yv[num2]);
			for (int j = 0; j < xvLength; j++)
			{
				if (j != i && j != num2 && j != num && triangle.IsInside(xv[j], yv[j]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
