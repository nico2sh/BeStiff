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

namespace Be_Stiff
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

		private Vector2[] path;

		private float speed;

		private float deceleration;

		private int nextPathPosition;

		private int currentPathPosition;

		private int pathForward;

		private double stopTime;

		private double estimatedArriveTime;

		private bool stopped;

		private int elevatorSoundIndex;

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
			path = new Vector2[worldScaledOgmoObject.Nodes.Count() + 1];
			ref Vector2 reference = ref path[0];
			reference = worldScaledOgmoObject.Position;
			speed = obj.GetValue<OgmoNumberValue>("speed").Value;
			deceleration = speed * speed / 4f;
			bool value = obj.GetValue<OgmoBooleanValue>("Light").Value;
			for (int i = 0; i < worldScaledOgmoObject.Nodes.Count(); i++)
			{
				ref Vector2 reference2 = ref path[i + 1];
				reference2 = worldScaledOgmoObject.Nodes[i].Position;
			}
			currentPathPosition = 0;
			nextPathPosition = 1;
			pathForward = 1;
			stopTime = 0.0;
			estimatedArriveTime = 0.0;
			stopped = false;
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
			elevatorSoundIndex = -1;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("elevatorDing", "audio\\noises\\elevatorDing");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("elevatorShaft", "audio\\noises\\elevatorShaft");
			estimatedArriveTime = ((path[currentPathPosition] - path[nextPathPosition]).Length() - 4f) / speed;
			estimatedArriveTime += 2.0 * Math.Sqrt(4f / deceleration);
			estimatedArriveTime = GameElementsControl.CurrentTimeInMS + estimatedArriveTime * 1000.0 + 2000.0;
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public void AddExitPortal(Portal enterPortal)
		{
			Vector2 vector = path[enterPortal.ActionToPerform.RefNumber];
			float num = ((Math.Sign((enterPortal.Position - vector).X) > 0) ? enterPortal.Width : 0f);
			PortalAction portalAction = new PortalAction("ExitElevator", new Vector2(enterPortal.Position.X + num, enterPortal.ActionToPerform.Side.Y), this);
			portalAction.RefNumber = enterPortal.ActionToPerform.RefNumber;
			Portal portal = new Portal(enterPortal.DestinationSector, enterPortal.OriginSector, mainBody.Position + new Vector2(0.5f, 0f), 1f, 3.5f, portalAction);
			sector.addPortal(portal);
		}

		private int debugFrame;

		public override void Update()
		{
			if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null && base.Name == "Elevator2" && debugFrame < 12)
			{
				for (var edge = mainBody.ContactList; edge != null; edge = edge.Next)
				{
					var other = edge.Other;
					string owner = "?";
					foreach (var f in other.FixtureList) if (f.UserData is WorldObjectData w && w.Object != null) owner = w.Object.GetType().Name + " '" + w.Object.Name + "'";
					Console.Error.WriteLine($"  Elevator2 f{debugFrame} contact with {owner} type={other.BodyType} pos={other.Position} vel={other.LinearVelocity} touching={edge.Contact.IsTouching()} manifoldPts={edge.Contact.Manifold.PointCount}");
				}
			}
			if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null && base.Name == "Elevator2" && debugFrame++ < 12)
				Console.Error.WriteLine($"Elevator2 f{debugFrame} pos={mainBody.Position} vel={mainBody.LinearVelocity} stopped={stopped} cur={currentPathPosition} next={nextPathPosition} pathLen={path.Length} joints={(mainBody.JointList != null)} contacts={(mainBody.ContactList != null)} time={GameElementsControl.CurrentTimeInMS} arrive={estimatedArriveTime}");
			if (stopTime + 2000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				if (MoveToPoint(path[nextPathPosition], path[currentPathPosition]))
				{
					int num = path.Count() - 1;
					int num2 = currentPathPosition + pathForward;
					if (num2 < 0 || num2 > num)
					{
						pathForward = -pathForward;
					}
					nextPathPosition = (int)MathHelper.Clamp(currentPathPosition + pathForward, 0f, num);
				}
				else
				{
					stopped = false;
					if (elevatorSoundIndex != -1)
					{
						float num3 = MathHelper.Clamp(1f - (GameElementsControl.Hero.Position - MainBody.Position).Length() / NoiseManager.MaxDistanceToHear, 0f, 1f);
						GameElementsControl.ScreenManager.AudioManager.SoundLoopVolume(elevatorSoundIndex, num3 * num3);
					}
				}
			}
			else
			{
				stopped = true;
			}
			elevatorLight.Position = GameElementsControl.ConvertWorldToScreen(CenterPosition);
			shadowHull[0].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			shadowHull[1].Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
		}

		private bool MoveToPoint(Vector2 destPoint, Vector2 originPoint)
		{
			if (GameElementsControl.LastFrameTimeInMS <= 0.0)
			{
				// Time is frozen (e.g. the "get ready" box); the speed maths below
				// divides by the frame time and would produce NaN velocities.
				return false;
			}
			float num = (mainBody.Position - destPoint).Length();
			float num2 = (mainBody.Position - originPoint).Length();
			Vector2 vector = VectorUtil.SafeNormalize(destPoint - mainBody.Position);
			_ = (double)(deceleration * 1000f) / GameElementsControl.LastFrameTimeInMS;
			if (num > 0f)
			{
				if (elevatorSoundIndex == -1)
				{
					elevatorSoundIndex = GameElementsControl.ScreenManager.AudioManager.PlaySoundLoop("elevatorShaft");
				}
				float num3 = mainBody.LinearVelocity.Length();
				float num4 = (float)((double)(num * 1000f) / GameElementsControl.LastFrameTimeInMS);
				if (!(num < 2f))
				{
					num3 = ((num2 < 2f) ? ((num2 != 0f) ? (mainBody.LinearVelocity.Length() + deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f)) : (deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f))) : ((!(num3 > num4)) ? speed : num4));
				}
				else
				{
					num3 = mainBody.LinearVelocity.Length() - deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f);
					if (num3 < 0f)
					{
						num3 = num4;
					}
				}
				sector.Position = mainBody.Position;
				safeZone.Position = mainBody.Position + new Vector2(0.5f, 0f);
				if (Environment.GetEnvironmentVariable("BESTIFF_DUMP") != null && (float.IsNaN(num3) || float.IsInfinity(num3) || float.IsNaN(vector.X)))
					Console.Error.WriteLine($"Elevator '{base.Name}' bad velocity: num={num} num2={num2} num3={num3} num4={num4} vector={vector} dest={destPoint} origin={originPoint} pos={mainBody.Position} vel={mainBody.LinearVelocity} speed={speed} decel={deceleration} frameMs={GameElementsControl.LastFrameTimeInMS}");
				mainBody.LinearVelocity = vector * num3;
				return false;
			}
			if (currentPathPosition != nextPathPosition)
			{
				stopped = true;
				stopTime = estimatedArriveTime;
				estimatedArriveTime = ((path[currentPathPosition] - path[nextPathPosition]).Length() - 4f) / speed;
				estimatedArriveTime += 2.0 * Math.Sqrt(4f / deceleration);
				estimatedArriveTime = stopTime + estimatedArriveTime * 1000.0 + 2000.0;
				currentPathPosition = nextPathPosition;
				mainBody.LinearVelocity = Vector2.Zero;
				if (elevatorSoundIndex != -1)
				{
					GameElementsControl.ScreenManager.AudioManager.StopSoundLoop(elevatorSoundIndex);
					elevatorSoundIndex = -1;
				}
				GameElementsControl.NoiseManager.AddNoise("elevatorDing", MainBody.Position);
			}
			return true;
		}

		public bool IsInside(ref Vector2 pos)
		{
			return safeZone.Contains(ref pos);
		}

		public int CurrentFloor()
		{
			if (!stopped)
			{
				return -1;
			}
			return currentPathPosition;
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
