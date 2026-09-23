using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff
{
	public class Portal
	{
		private Sector fromSector;

		private Sector toSector;

		private Zone rectangleZone;

		private PortalAction actionToPerform;

		private Vector2 relativePositionFromSector;

		private bool active;

		public float Width => rectangleZone.Width;

		public float Height => rectangleZone.Height;

		public Vector2 Position
		{
			get
			{
				return rectangleZone.Position;
			}
			set
			{
				rectangleZone.Position = value;
			}
		}

		public Sector DestinationSector => toSector;

		public Sector OriginSector => fromSector;

		public PortalAction ActionToPerform => actionToPerform;

		public Vector2 Center => rectangleZone.Center;

		public Portal(Sector s1, Sector s2, Vector2 pos, float width, float height, PortalAction atp)
		{
			_ = pos + new Vector2(width, height);
			rectangleZone = new Zone(pos, width, height);
			fromSector = s1;
			toSector = s2;
			relativePositionFromSector = pos - fromSector.Position;
			actionToPerform = atp;
			active = true;
			fromSector.addPortal(this);
		}

		public Portal()
		{
		}

		public bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Substring(0, 6).Equals("Portal"))
			{
				return false;
			}
			string text = obj.Name.Substring(7);
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			rectangleZone = new Zone(worldScaledOgmoObject.Position, worldScaledOgmoObject.Width, worldScaledOgmoObject.Height);
			fromSector = GameElementsControl.PathFindMap.GetSectorByName(obj.GetValue<OgmoStringValue>("From").Value);
			toSector = GameElementsControl.PathFindMap.GetSectorByName(obj.GetValue<OgmoStringValue>("To").Value);
			relativePositionFromSector = worldScaledOgmoObject.Position - fromSector.Position;
			string text2 = text;
			string value = obj.GetValue<OgmoStringValue>("To").Value;
			switch (text2)
			{
			case "Elevator":
			{
				WorldObject worldObjectByName2 = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName2 is Elevator elevator)
				{
					ConvertUnits.ToSimUnits(obj.GetValue<OgmoNumberValue>("PositionX").Value, 0f, out var simUnits5);
					actionToPerform = new PortalAction(text2, simUnits5, worldObjectByName2);
					actionToPerform.RefNumber = (int)obj.GetValue<OgmoNumberValue>("Floor").Value;
					elevator.AddExitPortal(this);
				}
				break;
			}
			case "Platform":
			{
				WorldObject worldObjectByName = GameElementsControl.GetWorldObjectByName(value);
				if (worldObjectByName is MovingPlatformH movingPlatformH)
				{
					ConvertUnits.ToSimUnits(obj.GetValue<OgmoNumberValue>("PositionX").Value, 0f, out var simUnits4);
					actionToPerform = new PortalAction(text2, simUnits4, worldObjectByName);
					actionToPerform.RefNumber = (int)obj.GetValue<OgmoNumberValue>("Floor").Value;
					movingPlatformH.AddExitPortal(this);
				}
				break;
			}
			case "Walk":
			{
				ConvertUnits.ToSimUnits(obj.GetValue<OgmoNumberValue>("PositionX").Value, 0f, out var simUnits3);
				actionToPerform = new PortalAction(text2, simUnits3, GameElementsControl.GetWorldObjectByName(obj.GetValue<OgmoStringValue>("Obstacle").Value));
				break;
			}
			case "Jump":
			{
				ConvertUnits.ToSimUnits(obj.GetValue<OgmoNumberValue>("PositionX").Value, obj.GetValue<OgmoNumberValue>("PositionY").Value, out var simUnits2);
				actionToPerform = new PortalAction(text2, simUnits2, GameElementsControl.GetWorldObjectByName(obj.GetValue<OgmoStringValue>("Obstacle").Value));
				break;
			}
			case "JumpRelative":
			{
				ConvertUnits.ToSimUnits(obj.GetValue<OgmoNumberValue>("PositionX").Value, obj.GetValue<OgmoNumberValue>("PositionY").Value, out var simUnits);
				actionToPerform = new PortalAction(text2, simUnits, GameElementsControl.GetWorldObjectByName(obj.GetValue<OgmoStringValue>("Obstacle").Value));
				break;
			}
			case "JumpDown":
			{
				Vector2 zero = Vector2.Zero;
				actionToPerform = new PortalAction(text2, zero, GameElementsControl.GetWorldObjectByName(obj.GetValue<OgmoStringValue>("Obstacle").Value));
				break;
			}
			}
			active = true;
			fromSector.addPortal(this);
			return true;
		}

		public void MoveWithSector(Vector2 newSectorPosition)
		{
			rectangleZone.Position = newSectorPosition + relativePositionFromSector;
		}

		public bool IsActive()
		{
			active = actionToPerform.IsFeasible();
			return active;
		}

		public void SetActionObject(WorldObject wo)
		{
			actionToPerform.Object = wo;
			if (!(wo is Elevator))
			{
				active = false;
			}
		}

		public bool isInside(ref Vector2 pos)
		{
			return rectangleZone.Contains(ref pos);
		}

		public float GetDist(ref Vector2 pos)
		{
			if (rectangleZone.Contains(ref pos))
			{
				return 0f;
			}
			rectangleZone.GetDistance(ref pos, out var distance);
			return distance;
		}
	}
}
