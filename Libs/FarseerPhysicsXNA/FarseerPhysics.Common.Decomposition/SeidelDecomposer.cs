using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
	/// <summary>
	/// Convex decomposition algorithm based on Raimund Seidel's paper "A simple and fast incremental randomized
	/// algorithm for computing trapezoidal decompositions and for triangulating polygons"
	/// See also: "Computational Geometry", 3rd edition, by Mark de Berg et al, Chapter 6.2
	///           "Computational Geometry in C", 2nd edition, by Joseph O'Rourke
	/// </summary>
	public static class SeidelDecomposer
	{
		/// <summary>
		/// Decompose the polygon into several smaller non-concave polygon.
		/// </summary>
		/// <param name="vertices">The polygon to decompose.</param>
		/// <param name="sheer">The sheer to use. If you get bad results, try using a higher value. The default value is 0.001</param>
		/// <returns>A list of triangles</returns>
		public static List<Vertices> ConvexPartition(Vertices vertices, float sheer)
		{
			List<Point> list = new List<Point>(vertices.Count);
			foreach (Vector2 vertex in vertices)
			{
				list.Add(new Point(vertex.X, vertex.Y));
			}
			Triangulator triangulator = new Triangulator(list, sheer);
			List<Vertices> list2 = new List<Vertices>();
			foreach (List<Point> triangle in triangulator.Triangles)
			{
				Vertices vertices2 = new Vertices(triangle.Count);
				foreach (Point item in triangle)
				{
					vertices2.Add(new Vector2(item.X, item.Y));
				}
				list2.Add(vertices2);
			}
			return list2;
		}

		/// <summary>
		/// Decompose the polygon into several smaller non-concave polygon.
		/// </summary>
		/// <param name="vertices">The polygon to decompose.</param>
		/// <param name="sheer">The sheer to use. If you get bad results, try using a higher value. The default value is 0.001</param>
		/// <returns>A list of trapezoids</returns>
		public static List<Vertices> ConvexPartitionTrapezoid(Vertices vertices, float sheer)
		{
			List<Point> list = new List<Point>(vertices.Count);
			foreach (Vector2 vertex in vertices)
			{
				list.Add(new Point(vertex.X, vertex.Y));
			}
			Triangulator triangulator = new Triangulator(list, sheer);
			List<Vertices> list2 = new List<Vertices>();
			foreach (Trapezoid trapezoid in triangulator.Trapezoids)
			{
				Vertices vertices2 = new Vertices();
				List<Point> list3 = trapezoid.Vertices();
				foreach (Point item in list3)
				{
					vertices2.Add(new Vector2(item.X, item.Y));
				}
				list2.Add(vertices2);
			}
			return list2;
		}
	}
}
