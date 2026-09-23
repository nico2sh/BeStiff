using System;
using System.Text;

namespace Poly2Tri.Triangulation.Delaunay.Sweep
{
	/// @author Thomas Åhlen (thahlen@gmail.com)
	public class AdvancingFront
	{
		public AdvancingFrontNode Head;

		protected AdvancingFrontNode Search;

		public AdvancingFrontNode Tail;

		public AdvancingFront(AdvancingFrontNode head, AdvancingFrontNode tail)
		{
			Head = head;
			Tail = tail;
			Search = head;
			AddNode(head);
			AddNode(tail);
		}

		public void AddNode(AdvancingFrontNode node)
		{
		}

		public void RemoveNode(AdvancingFrontNode node)
		{
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (AdvancingFrontNode advancingFrontNode = Head; advancingFrontNode != Tail; advancingFrontNode = advancingFrontNode.Next)
			{
				stringBuilder.Append(advancingFrontNode.Point.X).Append("->");
			}
			stringBuilder.Append(Tail.Point.X);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// MM:  This seems to be used by LocateNode to guess a position in the implicit linked list of AdvancingFrontNodes near x
		///      Removed an overload that depended on this being exact
		/// </summary>
		private AdvancingFrontNode FindSearchNode(double x)
		{
			return Search;
		}

		/// <summary>
		/// We use a balancing tree to locate a node smaller or equal to given key value
		/// </summary>
		public AdvancingFrontNode LocateNode(TriangulationPoint point)
		{
			return LocateNode(point.X);
		}

		private AdvancingFrontNode LocateNode(double x)
		{
			AdvancingFrontNode advancingFrontNode = FindSearchNode(x);
			if (x < advancingFrontNode.Value)
			{
				while ((advancingFrontNode = advancingFrontNode.Prev) != null)
				{
					if (x >= advancingFrontNode.Value)
					{
						Search = advancingFrontNode;
						return advancingFrontNode;
					}
				}
			}
			else
			{
				while ((advancingFrontNode = advancingFrontNode.Next) != null)
				{
					if (x < advancingFrontNode.Value)
					{
						Search = advancingFrontNode.Prev;
						return advancingFrontNode.Prev;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// This implementation will use simple node traversal algorithm to find a point on the front
		/// </summary>
		public AdvancingFrontNode LocatePoint(TriangulationPoint point)
		{
			double x = point.X;
			AdvancingFrontNode advancingFrontNode = FindSearchNode(x);
			double x2 = advancingFrontNode.Point.X;
			if (x == x2)
			{
				if (point != advancingFrontNode.Point)
				{
					if (point == advancingFrontNode.Prev.Point)
					{
						advancingFrontNode = advancingFrontNode.Prev;
					}
					else
					{
						if (point != advancingFrontNode.Next.Point)
						{
							throw new Exception("Failed to find Node for given afront point");
						}
						advancingFrontNode = advancingFrontNode.Next;
					}
				}
			}
			else if (x < x2)
			{
				while ((advancingFrontNode = advancingFrontNode.Prev) != null && point != advancingFrontNode.Point)
				{
				}
			}
			else
			{
				while ((advancingFrontNode = advancingFrontNode.Next) != null && point != advancingFrontNode.Point)
				{
				}
			}
			Search = advancingFrontNode;
			return advancingFrontNode;
		}
	}
}
