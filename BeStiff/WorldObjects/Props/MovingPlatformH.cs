using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Be_Stiff.WorldObjects.Props
{
	/// <summary>
	/// Horizontal moving platform. Characters ride on top of it, so it carries
	/// its own path-finding sector for enemies.
	/// </summary>
	internal class MovingPlatformH : MovingPlatform
	{
		private const float SectorHeight = 2f;

		private Sector sector;

		private Zone safeZone;

		protected override string ObjectName => "MovingPlatformH";

		protected override string SpriteSuffix => "H";

		protected override SpriteEffects FarBorderFlip => SpriteEffects.FlipHorizontally;

		private float Width => size.X;

		public Vector2 CenterPosition
		{
			get
			{
				Vector2 position = mainBody.Position;
				position.X += Width / 2f;
				position.Y += 1f;
				return position;
			}
		}

		protected override Vector2 MeasureSize(WorldScaledOgmoObject obj)
		{
			return new Vector2(obj.Width, FloorThick);
		}

		protected override Vector2[] HullPoints()
		{
			return new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(0f, FloorThick),
				ConvertUnits.ToDisplayUnits(Width, FloorThick),
				ConvertUnits.ToDisplayUnits(Width, 0f),
				ConvertUnits.ToDisplayUnits(0f, 0f)
			};
		}

		protected override void DecorOffsets(out Vector2 pivot1, out Vector2 pivot2, out Vector2 border1, out Vector2 border2)
		{
			pivot1 = new Vector2(0.25f, 0.25f);
			pivot2 = new Vector2(Width - 0.25f, 0.25f);
			border1 = new Vector2(0f, 0.25f);
			border2 = new Vector2(Width, 0.25f);
		}

		protected override void OnLoaded()
		{
			sector = new Sector();
			sector.Load(base.Name, Width, SectorHeight, SectorPosition);
			safeZone = new Zone(SafeZonePosition, Width / 2f, SectorHeight);
		}

		protected override void OnMoved()
		{
			sector.Position = SectorPosition;
			safeZone.Position = SafeZonePosition;
		}

		private Vector2 SectorPosition => mainBody.Position + new Vector2(0f, -SectorHeight);

		private Vector2 SafeZonePosition => mainBody.Position + new Vector2(Width / 4f, -SectorHeight);

		public void AddExitPortal(Portal enterPortal)
		{
			Vector2 side = enterPortal.Center - new Vector2(0f, enterPortal.Height / 2f);
			PortalAction portalAction = new PortalAction("ExitPlatform", side, this);
			portalAction.RefNumber = enterPortal.ActionToPerform.RefNumber;
			Portal portal = new Portal(enterPortal.DestinationSector, enterPortal.OriginSector, SafeZonePosition, Width / 2f, SectorHeight, portalAction);
			sector.addPortal(portal);
		}

		public bool IsInside(ref Vector2 pos)
		{
			return safeZone.Contains(ref pos);
		}
	}
}
