using FarseerPhysics.Common.Decomposition.CDT;

namespace Poly2Tri.Triangulation
{
	/// @author Thomas Åhlén, thahlen@gmail.com
	public class TriangulationUtil
	{
		public static double EPSILON = 1E-12;

		/// <summary>
		///   Requirements:
		/// 1. a,b and c form a triangle.
		/// 2. a and d is know to be on opposite side of bc
		/// <code>
		///                a
		///                +
		///               / \
		///              /   \
		///            b/     \c
		///            +-------+ 
		///           /    B    \  
		///          /           \ 
		/// </code>
		///    Facts:
		///  d has to be in area B to have a chance to be inside the circle formed by a,b and c
		///  d is outside B if orient2d(a,b,d) or orient2d(c,a,d) is CW
		///  This preknowledge gives us a way to optimize the incircle test
		/// </summary>
		/// <param name="pa">triangle point, opposite d</param>
		/// <param name="pb">triangle point</param>
		/// <param name="pc">triangle point</param>
		/// <param name="pd">point opposite a</param>
		/// <returns>true if d is inside circle, false if on circle edge</returns>
		public static bool SmartIncircle(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
		{
			double x = pd.X;
			double y = pd.Y;
			double num = pa.X - x;
			double num2 = pa.Y - y;
			double num3 = pb.X - x;
			double num4 = pb.Y - y;
			double num5 = num * num4;
			double num6 = num3 * num2;
			double num7 = num5 - num6;
			if (num7 <= 0.0)
			{
				return false;
			}
			double num8 = pc.X - x;
			double num9 = pc.Y - y;
			double num10 = num8 * num2;
			double num11 = num * num9;
			double num12 = num10 - num11;
			if (num12 <= 0.0)
			{
				return false;
			}
			double num13 = num3 * num9;
			double num14 = num8 * num4;
			double num15 = num * num + num2 * num2;
			double num16 = num3 * num3 + num4 * num4;
			double num17 = num8 * num8 + num9 * num9;
			double num18 = num15 * (num13 - num14) + num16 * num12 + num17 * num7;
			return num18 > 0.0;
		}

		public static bool InScanArea(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
		{
			double x = pd.X;
			double y = pd.Y;
			double num = pa.X - x;
			double num2 = pa.Y - y;
			double num3 = pb.X - x;
			double num4 = pb.Y - y;
			double num5 = num * num4;
			double num6 = num3 * num2;
			double num7 = num5 - num6;
			if (num7 <= 0.0)
			{
				return false;
			}
			double num8 = pc.X - x;
			double num9 = pc.Y - y;
			double num10 = num8 * num2;
			double num11 = num * num9;
			double num12 = num10 - num11;
			if (num12 <= 0.0)
			{
				return false;
			}
			return true;
		}

		/// Forumla to calculate signed area
		/// Positive if CCW
		/// Negative if CW
		/// 0 if collinear
		/// A[P1,P2,P3]  =  (x1*y2 - y1*x2) + (x2*y3 - y2*x3) + (x3*y1 - y3*x1)
		///              =  (x1-x3)*(y2-y3) - (y1-y3)*(x2-x3)
		public static Orientation Orient2d(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc)
		{
			double num = (pa.X - pc.X) * (pb.Y - pc.Y);
			double num2 = (pa.Y - pc.Y) * (pb.X - pc.X);
			double num3 = num - num2;
			if (num3 > 0.0 - EPSILON && num3 < EPSILON)
			{
				return Orientation.Collinear;
			}
			if (num3 > 0.0)
			{
				return Orientation.CCW;
			}
			return Orientation.CW;
		}
	}
}
