using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	/// <summary>
	/// Collection of helper methods for misc collisions.
	/// Does float tolerance and line collisions with lines and AABBs.
	/// </summary>
	public static class LineTools
	{
		public static float DistanceBetweenPointAndPoint(ref Vector2 point1, ref Vector2 point2)
		{
			Vector2.Subtract(ref point1, ref point2, out var result);
			return result.Length();
		}

		public static float DistanceBetweenPointAndLineSegment(ref Vector2 point, ref Vector2 lineEndPoint1, ref Vector2 lineEndPoint2)
		{
			Vector2 vector = Vector2.Subtract(lineEndPoint2, lineEndPoint1);
			Vector2 value = Vector2.Subtract(point, lineEndPoint1);
			float num = Vector2.Dot(value, vector);
			if (num <= 0f)
			{
				return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint1);
			}
			float num2 = Vector2.Dot(vector, vector);
			if (num2 <= num)
			{
				return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint2);
			}
			float scaleFactor = num / num2;
			Vector2 point2 = Vector2.Add(lineEndPoint1, Vector2.Multiply(vector, scaleFactor));
			return DistanceBetweenPointAndPoint(ref point, ref point2);
		}

		/// <summary>
		///             Check if the lines a0-&gt;a1 and b0-&gt;b1 cross.
		///             If they do, intersectionPoint will be filled
		///             with the point of crossing.
		///
		///             Grazing lines should not return true.
		///
		/// </summary>
		/// <param name="a0"></param>
		/// <param name="a1"></param>
		/// <param name="b0"></param>
		/// <param name="b1"></param>
		/// <param name="intersectionPoint"></param>
		/// <returns></returns>
		public static bool LineIntersect2(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersectionPoint)
		{
			intersectionPoint = Vector2.Zero;
			if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
			{
				return false;
			}
			float x = a0.X;
			float y = a0.Y;
			float x2 = a1.X;
			float y2 = a1.Y;
			float x3 = b0.X;
			float y3 = b0.Y;
			float x4 = b1.X;
			float y4 = b1.Y;
			if (Math.Max(x, x2) < Math.Min(x3, x4) || Math.Max(x3, x4) < Math.Min(x, x2))
			{
				return false;
			}
			if (Math.Max(y, y2) < Math.Min(y3, y4) || Math.Max(y3, y4) < Math.Min(y, y2))
			{
				return false;
			}
			float num = (x4 - x3) * (y - y3) - (y4 - y3) * (x - x3);
			float num2 = (x2 - x) * (y - y3) - (y2 - y) * (x - x3);
			float num3 = (y4 - y3) * (x2 - x) - (x4 - x3) * (y2 - y);
			if (Math.Abs(num3) < 1.1920929E-07f)
			{
				return false;
			}
			num /= num3;
			num2 /= num3;
			if (0f < num && num < 1f && 0f < num2 && num2 < 1f)
			{
				intersectionPoint.X = x + num * (x2 - x);
				intersectionPoint.Y = y + num * (y2 - y);
				return true;
			}
			return false;
		}

		public static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
		{
			Vector2 zero = Vector2.Zero;
			float num = p2.Y - p1.Y;
			float num2 = p1.X - p2.X;
			float num3 = num * p1.X + num2 * p1.Y;
			float num4 = q2.Y - q1.Y;
			float num5 = q1.X - q2.X;
			float num6 = num4 * q1.X + num5 * q1.Y;
			float num7 = num * num5 - num4 * num2;
			if (!MathUtils.FloatEquals(num7, 0f))
			{
				zero.X = (num5 * num3 - num2 * num6) / num7;
				zero.Y = (num * num6 - num4 * num3) / num7;
			}
			return zero;
		}

		/// <summary>
		/// This method detects if two line segments (or lines) intersect,
		/// and, if so, the point of intersection. Use the <paramref name="firstIsSegment" /> and
		/// <paramref name="secondIsSegment" /> parameters to set whether the intersection point
		/// must be on the first and second line segments. Setting these
		/// both to true means you are doing a line-segment to line-segment
		/// intersection. Setting one of them to true means you are doing a
		/// line to line-segment intersection test, and so on.
		/// Note: If two line segments are coincident, then 
		/// no intersection is detected (there are actually
		/// infinite intersection points).
		/// Author: Jeremy Bell
		/// </summary>
		/// <param name="point1">The first point of the first line segment.</param>
		/// <param name="point2">The second point of the first line segment.</param>
		/// <param name="point3">The first point of the second line segment.</param>
		/// <param name="point4">The second point of the second line segment.</param>
		/// <param name="point">This is set to the intersection
		/// point if an intersection is detected.</param>
		/// <param name="firstIsSegment">Set this to true to require that the 
		/// intersection point be on the first line segment.</param>
		/// <param name="secondIsSegment">Set this to true to require that the
		/// intersection point be on the second line segment.</param>
		/// <returns>True if an intersection is detected, false otherwise.</returns>
		public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 point)
		{
			point = default(Vector2);
			float num = point4.Y - point3.Y;
			float num2 = point2.X - point1.X;
			float num3 = point4.X - point3.X;
			float num4 = point2.Y - point1.Y;
			float num5 = num * num2 - num3 * num4;
			if (!(num5 >= -1.1920929E-07f) || !(num5 <= 1.1920929E-07f))
			{
				float num6 = point1.Y - point3.Y;
				float num7 = point1.X - point3.X;
				float num8 = 1f / num5;
				float num9 = num3 * num6 - num * num7;
				num9 *= num8;
				if (!firstIsSegment || (num9 >= 0f && num9 <= 1f))
				{
					float num10 = num2 * num6 - num4 * num7;
					num10 *= num8;
					if ((!secondIsSegment || (num10 >= 0f && num10 <= 1f)) && (num9 != 0f || num10 != 0f))
					{
						point.X = point1.X + num9 * num2;
						point.Y = point1.Y + num9 * num4;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// This method detects if two line segments (or lines) intersect,
		/// and, if so, the point of intersection. Use the <paramref name="firstIsSegment" /> and
		/// <paramref name="secondIsSegment" /> parameters to set whether the intersection point
		/// must be on the first and second line segments. Setting these
		/// both to true means you are doing a line-segment to line-segment
		/// intersection. Setting one of them to true means you are doing a
		/// line to line-segment intersection test, and so on.
		/// Note: If two line segments are coincident, then 
		/// no intersection is detected (there are actually
		/// infinite intersection points).
		/// Author: Jeremy Bell
		/// </summary>
		/// <param name="point1">The first point of the first line segment.</param>
		/// <param name="point2">The second point of the first line segment.</param>
		/// <param name="point3">The first point of the second line segment.</param>
		/// <param name="point4">The second point of the second line segment.</param>
		/// <param name="intersectionPoint">This is set to the intersection
		/// point if an intersection is detected.</param>
		/// <param name="firstIsSegment">Set this to true to require that the 
		/// intersection point be on the first line segment.</param>
		/// <param name="secondIsSegment">Set this to true to require that the
		/// intersection point be on the second line segment.</param>
		/// <returns>True if an intersection is detected, false otherwise.</returns>
		public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 intersectionPoint)
		{
			return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
		}

		/// <summary>
		/// This method detects if two line segments intersect,
		/// and, if so, the point of intersection. 
		/// Note: If two line segments are coincident, then 
		/// no intersection is detected (there are actually
		/// infinite intersection points).
		/// </summary>
		/// <param name="point1">The first point of the first line segment.</param>
		/// <param name="point2">The second point of the first line segment.</param>
		/// <param name="point3">The first point of the second line segment.</param>
		/// <param name="point4">The second point of the second line segment.</param>
		/// <param name="intersectionPoint">This is set to the intersection
		/// point if an intersection is detected.</param>
		/// <returns>True if an intersection is detected, false otherwise.</returns>
		public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, out Vector2 intersectionPoint)
		{
			return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment: true, secondIsSegment: true, out intersectionPoint);
		}

		/// <summary>
		/// This method detects if two line segments intersect,
		/// and, if so, the point of intersection. 
		/// Note: If two line segments are coincident, then 
		/// no intersection is detected (there are actually
		/// infinite intersection points).
		/// </summary>
		/// <param name="point1">The first point of the first line segment.</param>
		/// <param name="point2">The second point of the first line segment.</param>
		/// <param name="point3">The first point of the second line segment.</param>
		/// <param name="point4">The second point of the second line segment.</param>
		/// <param name="intersectionPoint">This is set to the intersection
		/// point if an intersection is detected.</param>
		/// <returns>True if an intersection is detected, false otherwise.</returns>
		public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)
		{
			return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment: true, secondIsSegment: true, out intersectionPoint);
		}

		/// <summary>
		/// Get all intersections between a line segment and a list of vertices
		/// representing a polygon. The vertices reuse adjacent points, so for example
		/// edges one and two are between the first and second vertices and between the
		/// second and third vertices. The last edge is between vertex vertices.Count - 1
		/// and verts0. (ie, vertices from a Geometry or AABB)
		/// </summary>
		/// <param name="point1">The first point of the line segment to test</param>
		/// <param name="point2">The second point of the line segment to test.</param>
		/// <param name="vertices">The vertices, as described above</param>
		/// <param name="intersectionPoints">An list of intersection points. Any intersection points
		/// found will be added to this list.</param>
		public static void LineSegmentVerticesIntersect(ref Vector2 point1, ref Vector2 point2, Vertices vertices, ref List<Vector2> intersectionPoints)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, firstIsSegment: true, secondIsSegment: true, out var intersectionPoint))
				{
					intersectionPoints.Add(intersectionPoint);
				}
			}
		}

		/// <summary>
		/// Get all intersections between a line segment and an AABB. 
		/// </summary>
		/// <param name="point1">The first point of the line segment to test</param>
		/// <param name="point2">The second point of the line segment to test.</param>
		/// <param name="aabb">The AABB that is used for testing intersection.</param>
		/// <param name="intersectionPoints">An list of intersection points. Any intersection points found will be added to this list.</param>
		public static void LineSegmentAABBIntersect(ref Vector2 point1, ref Vector2 point2, AABB aabb, ref List<Vector2> intersectionPoints)
		{
			LineSegmentVerticesIntersect(ref point1, ref point2, aabb.Vertices, ref intersectionPoints);
		}
	}
}
