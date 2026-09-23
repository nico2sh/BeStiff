using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Be_Stiff.AI
{
	public class PathFindMap
	{
		private Dictionary<string, Sector> _sectors;

		private Dictionary<string, BreadCrumb> _brWorld;

		private MinHeap<BreadCrumb> _openList;

		public PathFindMap()
		{
			_sectors = new Dictionary<string, Sector>();
			_brWorld = new Dictionary<string, BreadCrumb>();
			_openList = new MinHeap<BreadCrumb>(256);
		}

		public PathFindMap(int numSectors)
		{
			_sectors = new Dictionary<string, Sector>(numSectors);
			_brWorld = new Dictionary<string, BreadCrumb>(numSectors);
			_openList = new MinHeap<BreadCrumb>(numSectors);
		}

		public float CheckClosestSector(ref Vector2 p1, ref Vector2 p2)
		{
			float num = (p1 - p2).Length();
			foreach (Sector value in _sectors.Values)
			{
				if (value.Rectangle.Intersects(ref p1, ref p2, out var inter))
				{
					float num2 = (p1 - inter).Length();
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return num;
		}

		public void AddSector(Sector sector)
		{
			if (_sectors.ContainsKey(sector.Name))
			{
				throw new Exception("A sector with that name already exists");
			}
			_sectors.Add(sector.Name, sector);
		}

		public Sector GetSectorAt(ref Vector2 pos)
		{
			foreach (Sector value in _sectors.Values)
			{
				if (value.IsInside(ref pos))
				{
					return value;
				}
			}
			return null;
		}

		public Sector GetSectorByName(string name)
		{
			if (_sectors.TryGetValue(name, out var value))
			{
				return value;
			}
			return null;
		}

		public void GetPathFromTo(ref Vector2 start, ref Vector2 end, ref Stack<Sector> retValue)
		{
			Sector start2 = GetSectorAt(ref start);
			Sector end2 = GetSectorAt(ref end);
			if (start2 == null || end2 == null)
			{
				retValue.Clear();
			}
			else
			{
				GetPathFromTo(ref start2, ref end2, ref retValue);
			}
		}

		public void GetPathFromTo(ref Sector start, ref Sector end, ref Stack<Sector> retValue)
		{
			retValue.Clear();
			if (start == end || start == null || end == null)
			{
				// No sector graph for one of the ends (test levels have incomplete
				// sector layers); no path.
				return;
			}
			_openList.Clear();
			_brWorld.Clear();
			BreadCrumb breadCrumb = new BreadCrumb(start);
			breadCrumb.cost = 0f;
			BreadCrumb breadcrumb = new BreadCrumb(end);
			_brWorld.Add(start.Name, breadCrumb);
			_openList.Add(breadCrumb);
			while (_openList.Count > 0)
			{
				breadCrumb = _openList.ExtractFirst();
				breadCrumb.onClosedList = true;
				foreach (Portal portal in breadCrumb.sector.Portals)
				{
					if (!portal.IsActive())
					{
						continue;
					}
					Sector destinationSector = portal.DestinationSector;
					if (destinationSector == null)
					{
						continue;
					}
					if (!_brWorld.TryGetValue(destinationSector.Name, out var value))
					{
						value = new BreadCrumb(destinationSector);
						_brWorld.Add(destinationSector.Name, value);
					}
					if (value.onClosedList)
					{
						continue;
					}
					float num = 0f;
					num += (breadCrumb.sector.Position - value.sector.Position).Length();
					float num2 = breadCrumb.cost + num + (end.Position - value.sector.Position).Length();
					if (num2 < value.cost)
					{
						value.cost = num2;
						value.next = breadCrumb;
					}
					if (!value.onOpenList)
					{
						if (value.Equals(breadcrumb))
						{
							value.next = breadCrumb;
							value.ToStack(ref retValue);
							return;
						}
						value.onOpenList = true;
						_openList.Add(value);
					}
				}
			}
		}

		public List<Portal> GetPortalsTo(Sector sector)
		{
			List<Portal> list = new List<Portal>();
			foreach (Sector value in _sectors.Values)
			{
				Portal portalFor = value.getPortalFor(sector);
				if (portalFor != null)
				{
					if (portalFor.ActionToPerform.Action == Actions.ExitElevator)
					{
						list.AddRange(GetPortalsTo(portalFor.OriginSector));
					}
					else
					{
						list.Add(portalFor);
					}
				}
			}
			return list;
		}
	}
}
