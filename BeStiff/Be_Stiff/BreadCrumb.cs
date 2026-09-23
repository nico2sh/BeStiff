using System;
using System.Collections.Generic;
using System.Linq;

namespace Be_Stiff
{
	public class BreadCrumb : IComparable<BreadCrumb>
	{
		public Sector sector;

		public BreadCrumb next;

		public float cost = 2.1474836E+09f;

		public bool onClosedList;

		public bool onOpenList;

		public BreadCrumb(Sector sector)
		{
			this.sector = sector;
		}

		public BreadCrumb()
		{
		}

		public void ClearAndSet(Sector sector)
		{
			this.sector = sector;
			next = null;
			cost = 2.1474836E+09f;
			onClosedList = false;
			onOpenList = false;
		}

		public BreadCrumb(Sector sector, BreadCrumb parent)
		{
			this.sector = sector;
			next = parent;
		}

		public void ToStack(ref Stack<Sector> path)
		{
			path.ToList();
			BreadCrumb breadCrumb = this;
			while (breadCrumb.next != null)
			{
				path.Push(breadCrumb.sector);
				breadCrumb = breadCrumb.next;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is BreadCrumb)
			{
				return ((BreadCrumb)obj).sector == sector;
			}
			return false;
		}

		public bool Equals(BreadCrumb breadcrumb)
		{
			return breadcrumb.sector == sector;
		}

		public override int GetHashCode()
		{
			return sector.GetHashCode();
		}

		public int CompareTo(BreadCrumb other)
		{
			return cost.CompareTo(other.cost);
		}
	}
}
