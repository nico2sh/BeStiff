using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.Levels
{
	public class Sector
	{
		private Zone rectangleZone;

		private List<Portal> portals;

		private string name;

		private Vector2 center;

		public string Name => name;

		public Zone Rectangle => rectangleZone;

		public Vector2 UpLeft => Position;

		public Vector2 downRight => Position + new Vector2(rectangleZone.Width, 0f - rectangleZone.Height);

		public List<Portal> Portals => portals;

		public Vector2 Position
		{
			get
			{
				return rectangleZone.Position;
			}
			set
			{
				foreach (Portal portal in Portals)
				{
					portal.MoveWithSector(value);
				}
				rectangleZone.Position = value;
			}
		}

		public Vector2 CenterPosition => rectangleZone.Center;

		public Sector()
		{
			portals = new List<Portal>();
		}

		public bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Sector"))
			{
				return false;
			}
			name = obj.GetValue<OgmoStringValue>("ID").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			_ = worldScaledOgmoObject.Position + new Vector2(0f, 0f - worldScaledOgmoObject.Height);
			_ = worldScaledOgmoObject.Position + new Vector2(worldScaledOgmoObject.Width, 0f);
			rectangleZone = new Zone(worldScaledOgmoObject.Position, worldScaledOgmoObject.Width, worldScaledOgmoObject.Height);
			center = rectangleZone.Center;
			GameElementsControl.PathFindMap.AddSector(this);
			return true;
		}

		public void Load(string n, float width, float height, Vector2 pos)
		{
			name = n;
			portals = new List<Portal>();
			rectangleZone = new Zone(pos, width, height);
			center = rectangleZone.Center;
			GameElementsControl.PathFindMap.AddSector(this);
		}

		public void addPortal(Portal portal)
		{
			portals.Add(portal);
		}

		public bool IsInside(ref Vector2 pos)
		{
			return rectangleZone.Contains(ref pos);
		}

		public Portal getPortalAt(ref Vector2 pos)
		{
			foreach (Portal portal in portals)
			{
				if (portal.isInside(ref pos))
				{
					return portal;
				}
			}
			return null;
		}

		public Portal getPortalFor(Sector sector)
		{
			foreach (Portal portal in portals)
			{
				if (portal.DestinationSector == sector)
				{
					return portal;
				}
			}
			return null;
		}

		public static float getDistBetween(Sector sector1, Sector sector2)
		{
			return (sector1.center - sector2.center).Length();
		}
	}
}
