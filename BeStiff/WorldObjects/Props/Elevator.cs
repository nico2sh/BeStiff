using System;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Elevator : WorldObject, IShadowCaster
	{
		private const double timeInStop = 2000.0;

		private const float width = 2f;

		private const float height = 3.5f;

		private const float floorThick = 0.5f;

		private const float distanceToDecelerate = 2f;

		private Fixture fixtureRoof;

		private Fixture fixtureFloor;

		private PathMover mover;

		private GameSprite elevatorSprite;

		private GameSprite elevatorForeGroundSprite;

		private Light2D elevatorLight;

		private Sector sector;

		private Zone safeZone;

		public override float Mass => mainBody.Mass;

		public ShadowHull[] shadowHull { get; set; }

		public override Body MainBody => mainBody;

		public Vector2 CenterPosition
		{
			get
			{
				Vector2 position = mainBody.Position;
				position.X += 1f;
				position.Y += 1.75f;
				return position;
			}
		}

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("Elevator"))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			shadowHull = new ShadowHull[2];
			Vector2[] path = PathMover.BuildPath(worldScaledOgmoObject);
			float speed = obj.GetValue<OgmoNumberValue>("speed").Value;
			bool value = obj.GetValue<OgmoBooleanValue>("Light").Value;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Kinematic;
			mainBody.Position = worldScaledOgmoObject.Position;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(0f, 0.5f));
			vertices.Add(new Vector2(2f, 0.5f));
			vertices.Add(new Vector2(2f, 0f));
			vertices.Add(new Vector2(0f, 0f));
			vertices.Reverse(); // y was flipped for the y-down world; keep counter-clockwise winding
			PolygonShape shape = new PolygonShape(vertices, 10f);
			fixtureRoof = mainBody.CreateFixture(shape);
			fixtureRoof.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			Vector2[] points = new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(0f, 0.5f),
				ConvertUnits.ToDisplayUnits(2f, 0.5f),
				ConvertUnits.ToDisplayUnits(2f, 0f),
				ConvertUnits.ToDisplayUnits(0f, 0f)
			};
			shadowHull[0] = ShadowHull.CreateConvex(ref points);
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			GameElementsControl.Krypton.Hulls.Add(shadowHull[0]);
			vertices.Clear();
			vertices.Add(new Vector2(0f, 3.5f));
			vertices.Add(new Vector2(2f, 3.5f));
			vertices.Add(new Vector2(2f, 3f));
			vertices.Add(new Vector2(0f, 3f));
			vertices.Reverse();
			shape = new PolygonShape(vertices, 10f);
			fixtureFloor = mainBody.CreateFixture(shape);
			fixtureFloor.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			points = new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(0f, 3.5f),
				ConvertUnits.ToDisplayUnits(2f, 3.5f),
				ConvertUnits.ToDisplayUnits(2f, 3f),
				ConvertUnits.ToDisplayUnits(0f, 3f)
			};
			shadowHull[1] = ShadowHull.CreateConvex(ref points);
			shadowHull[1].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			GameElementsControl.Krypton.Hulls.Add(shadowHull[1]);
			sector = new Sector();
			sector.Load(base.Name, 2f, 3.5f, mainBody.Position);
			safeZone = new Zone(mainBody.Position + new Vector2(0.5f, 0f), 1f, 3.5f);
			if (value)
			{
				GameElementsControl.LoadSprite("elevator", "sprites\\objects\\elevator");
				elevatorSprite = GameElementsControl.GetSprite("elevator");
			}
			else
			{
				GameElementsControl.LoadSprite("elevatorNoLight", "sprites\\objects\\elevatorNoLight");
				elevatorSprite = GameElementsControl.GetSprite("elevatorNoLight");
			}
			GameElementsControl.LoadSprite("elevatorForeground", "sprites\\objects\\elevatorforeground");
			elevatorForeGroundSprite = GameElementsControl.GetSprite("elevatorForeground");
			elevatorLight = new Light2D();
			elevatorLight.Texture = LightTextureBuilder.CreatePointLight(GameElementsControl.ScreenManager.GraphicsDevice, 512);
			elevatorLight.Position = GameElementsControl.ConvertWorldToScreen(CenterPosition);
			elevatorLight.Color = Color.LightYellow;
			elevatorLight.Angle = 0f;
			elevatorLight.Range = 256f;
			if (value)
			{
				GameElementsControl.Krypton.Lights.Add(elevatorLight);
			}
			GameElementsControl.ScreenManager.AudioManager.LoadSound("elevatorDing", "audio\\noises\\elevatorDing");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("elevatorShaft", "audio\\noises\\elevatorShaft");
			mover = new PathMover(mainBody, path, speed, distanceToDecelerate, timeInStop, "elevatorShaft", startStopped: false);
			mover.Arrived += delegate
			{
				GameElementsControl.NoiseManager.AddNoise("elevatorDing", MainBody.Position);
			};
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public void AddExitPortal(Portal enterPortal)
		{
			Vector2 vector = mover.PathPoint(enterPortal.ActionToPerform.RefNumber);
			float num = ((Math.Sign((enterPortal.Position - vector).X) > 0) ? enterPortal.Width : 0f);
			PortalAction portalAction = new PortalAction("ExitElevator", new Vector2(enterPortal.Position.X + num, enterPortal.ActionToPerform.Side.Y), this);
			portalAction.RefNumber = enterPortal.ActionToPerform.RefNumber;
			Portal portal = new Portal(enterPortal.DestinationSector, enterPortal.OriginSector, mainBody.Position + new Vector2(0.5f, 0f), 1f, 3.5f, portalAction);
			sector.addPortal(portal);
		}

		private Vector2 lastPosition;

		public override void Update()
		{
			mover.Update();
			if (mainBody.Position == lastPosition)
			{
				return;
			}
			lastPosition = mainBody.Position;
			sector.Position = mainBody.Position;
			safeZone.Position = mainBody.Position + new Vector2(0.5f, 0f);
			elevatorLight.Position = GameElementsControl.ConvertWorldToScreen(CenterPosition);
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			shadowHull[1].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
		}

		public bool IsInside(ref Vector2 pos)
		{
			return safeZone.Contains(ref pos);
		}

		public int CurrentFloor()
		{
			return mover.CurrentFloor;
		}

		public override bool CanHanged()
		{
			return true;
		}

		public override void Draw()
		{
			elevatorSprite.Draw(GameElementsControl.ConvertWorldToScreen(CenterPosition));
		}

		public void DrawHull()
		{
			elevatorForeGroundSprite.Draw(GameElementsControl.ConvertWorldToScreen(CenterPosition));
		}
	}
}
