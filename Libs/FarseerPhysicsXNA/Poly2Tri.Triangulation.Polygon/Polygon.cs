using System;
using System.Collections.Generic;
using System.Linq;
using Poly2Tri.Triangulation.Delaunay;

namespace Poly2Tri.Triangulation.Polygon
{
	public class Polygon : Triangulatable
	{
		protected List<Polygon> _holes;

		protected PolygonPoint _last;

		protected List<TriangulationPoint> _points = new List<TriangulationPoint>();

		protected List<TriangulationPoint> _steinerPoints;

		protected List<DelaunayTriangle> _triangles;

		public IList<Polygon> Holes => _holes;

		public TriangulationMode TriangulationMode => TriangulationMode.Polygon;

		public IList<TriangulationPoint> Points => _points;

		public IList<DelaunayTriangle> Triangles => _triangles;

		/// <summary>
		/// Create a polygon from a list of at least 3 points with no duplicates.
		/// </summary>
		/// <param name="points">A list of unique points</param>
		public Polygon(IList<PolygonPoint> points)
		{
			if (points.Count < 3)
			{
				throw new ArgumentException("List has fewer than 3 points", "points");
			}
			if (points[0].Equals(points[points.Count - 1]))
			{
				points.RemoveAt(points.Count - 1);
			}
			_points.AddRange(points.Cast<TriangulationPoint>());
		}

		/// <summary>
		/// Create a polygon from a list of at least 3 points with no duplicates.
		/// </summary>
		/// <param name="points">A list of unique points.</param>
		public Polygon(IEnumerable<PolygonPoint> points)
			: this((points as IList<PolygonPoint>) ?? ((IList<PolygonPoint>)points.ToArray()))
		{
		}

		public Polygon()
		{
		}

		public void AddTriangle(DelaunayTriangle t)
		{
			_triangles.Add(t);
		}

		public void AddTriangles(IEnumerable<DelaunayTriangle> list)
		{
			_triangles.AddRange(list);
		}

		public void ClearTriangles()
		{
			if (_triangles != null)
			{
				_triangles.Clear();
			}
		}

		/// <summary>
		/// Creates constraints and populates the context with points
		/// </summary>
		/// <param name="tcx">The context</param>
		public void PrepareTriangulation(TriangulationContext tcx)
		{
			if (_triangles == null)
			{
				_triangles = new List<DelaunayTriangle>(_points.Count);
			}
			else
			{
				_triangles.Clear();
			}
			for (int i = 0; i < _points.Count - 1; i++)
			{
				tcx.NewConstraint(_points[i], _points[i + 1]);
			}
			tcx.NewConstraint(_points[0], _points[_points.Count - 1]);
			tcx.Points.AddRange(_points);
			if (_holes != null)
			{
				foreach (Polygon hole in _holes)
				{
					for (int j = 0; j < hole._points.Count - 1; j++)
					{
						tcx.NewConstraint(hole._points[j], hole._points[j + 1]);
					}
					tcx.NewConstraint(hole._points[0], hole._points[hole._points.Count - 1]);
					tcx.Points.AddRange(hole._points);
				}
			}
			if (_steinerPoints != null)
			{
				tcx.Points.AddRange(_steinerPoints);
			}
		}

		public void AddSteinerPoint(TriangulationPoint point)
		{
			if (_steinerPoints == null)
			{
				_steinerPoints = new List<TriangulationPoint>();
			}
			_steinerPoints.Add(point);
		}

		public void AddSteinerPoints(List<TriangulationPoint> points)
		{
			if (_steinerPoints == null)
			{
				_steinerPoints = new List<TriangulationPoint>();
			}
			_steinerPoints.AddRange(points);
		}

		public void ClearSteinerPoints()
		{
			if (_steinerPoints != null)
			{
				_steinerPoints.Clear();
			}
		}

		/// <summary>
		/// Add a hole to the polygon.
		/// </summary>
		/// <param name="poly">A subtraction polygon fully contained inside this polygon.</param>
		public void AddHole(Polygon poly)
		{
			if (_holes == null)
			{
				_holes = new List<Polygon>();
			}
			_holes.Add(poly);
		}

		/// <summary>
		/// Inserts newPoint after point.
		/// </summary>
		/// <param name="point">The point to insert after in the polygon</param>
		/// <param name="newPoint">The point to insert into the polygon</param>
		public void InsertPointAfter(PolygonPoint point, PolygonPoint newPoint)
		{
			int num = _points.IndexOf(point);
			if (num == -1)
			{
				throw new ArgumentException("Tried to insert a point into a Polygon after a point not belonging to the Polygon", "point");
			}
			newPoint.Next = point.Next;
			newPoint.Previous = point;
			point.Next.Previous = newPoint;
			point.Next = newPoint;
			_points.Insert(num + 1, newPoint);
		}

		/// <summary>
		/// Inserts list (after last point in polygon?)
		/// </summary>
		/// <param name="list"></param>
		public void AddPoints(IEnumerable<PolygonPoint> list)
		{
			foreach (PolygonPoint item in list)
			{
				item.Previous = _last;
				if (_last != null)
				{
					item.Next = _last.Next;
					_last.Next = item;
				}
				_last = item;
				_points.Add(item);
			}
			PolygonPoint polygonPoint = (PolygonPoint)_points[0];
			_last.Next = polygonPoint;
			polygonPoint.Previous = _last;
		}

		/// <summary>
		/// Adds a point after the last in the polygon.
		/// </summary>
		/// <param name="p">The point to add</param>
		public void AddPoint(PolygonPoint p)
		{
			p.Previous = _last;
			p.Next = _last.Next;
			_last.Next = p;
			_points.Add(p);
		}

		/// <summary>
		/// Removes a point from the polygon.
		/// </summary>
		/// <param name="p"></param>
		public void RemovePoint(PolygonPoint p)
		{
			PolygonPoint next = p.Next;
			PolygonPoint previous = p.Previous;
			previous.Next = next;
			next.Previous = previous;
			_points.Remove(p);
		}
	}
}
