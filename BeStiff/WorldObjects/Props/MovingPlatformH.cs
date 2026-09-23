using System;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Krypton;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using OgmoXNA.Values;

namespace Be_Stiff.WorldObjects.Props
{
	internal class MovingPlatformH : WorldObject, IShadowCaster
	{
		private const float sectorHeight = 2f;

		private const float floorThick = 0.5f;

		private Rectangle rectangle;

		private Fixture fixture;

		private Vector2[] path;

		private float speed;

		private float deceleration;

		private int nextPathPosition;

		private int currentPathPosition;

		private int pathForward;

		private double stopTime;

		private double estimatedArriveTime;

		private bool stopped;

		private float width;

		private int platformSoundIndex;

		private GameSprite platformSprite;

		private GameSprite borderSprite;

		private GameSprite pivotSprite;

		private Vector2 pivotPosition1;

		private Vector2 pivotPosition2;

		private Vector2 borderPosition1;

		private Vector2 borderPosition2;

		private float pivotRotation;

		private ShadowHull shadowHull;

		private float distanceToDecelerate;

		private double timeInStop;

		private Sector sector;

		private Zone safeZone;

		public override float Mass => mainBody.Mass;

		public override Body MainBody => mainBody;

		public Vector2 CenterPosition
		{
			get
			{
				Vector2 position = mainBody.Position;
				position.X += width / 2f;
				position.Y += 1f;
				return position;
			}
		}

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			if (!obj.Name.Equals("MovingPlatformH"))
			{
				return false;
			}
			base.Name = obj.GetValue<OgmoStringValue>("ID").Value;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			MathHelper.ToRadians(worldScaledOgmoObject.Rotation);
			path = new Vector2[worldScaledOgmoObject.Nodes.Count() + 1];
			ref Vector2 reference = ref path[0];
			reference = worldScaledOgmoObject.Position;
			speed = obj.GetValue<OgmoNumberValue>("speed").Value;
			distanceToDecelerate = obj.GetValue<OgmoNumberValue>("decelerateDistance").Value;
			timeInStop = obj.GetValue<OgmoNumberValue>("timeInStop").Value;
			deceleration = speed * speed / (2f * distanceToDecelerate);
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
			stopped = true;
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Kinematic;
			mainBody.Position = worldScaledOgmoObject.Position;
			width = worldScaledOgmoObject.Width;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(0f, 0f));
			vertices.Add(new Vector2(0f, 0.5f));
			vertices.Add(new Vector2(width, 0.5f));
			vertices.Add(new Vector2(width, 0f));
			vertices.Reverse(); // y was flipped for the y-down world; keep counter-clockwise winding
			PolygonShape shape = new PolygonShape(vertices, 10f);
			fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.CollisionWorldObject, this);
			fixture.Restitution = 0.1f;
			fixture.Friction = 1f;
			Vector2[] points = new Vector2[4]
			{
				ConvertUnits.ToDisplayUnits(0f, 0.5f),
				ConvertUnits.ToDisplayUnits(width, 0.5f),
				ConvertUnits.ToDisplayUnits(width, 0f),
				ConvertUnits.ToDisplayUnits(0f, 0f)
			};
			shadowHull = ShadowHull.CreateConvex(ref points);
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			GameElementsControl.Krypton.Hulls.Add(shadowHull);
			sector = new Sector();
			sector.Load(base.Name, width, 2f, mainBody.Position + new Vector2(0f, -2f));
			safeZone = new Zone(mainBody.Position + new Vector2(width / 4f, -2f), width / 2f, 2f);
			rectangle = new Rectangle(0, 1, obj.Width, obj.Height);
			GameElementsControl.LoadSprite("movingPlatformH", "sprites\\objects\\movingPlatformh");
			GameElementsControl.LoadSprite("doorPivot", "sprites\\objects\\doorpivot");
			GameElementsControl.LoadSprite("movingPlatformBorderH", "sprites\\objects\\movingPlatformborderh");
			platformSprite = GameElementsControl.GetSprite("movingPlatformH");
			pivotSprite = GameElementsControl.GetSprite("doorPivot");
			borderSprite = GameElementsControl.GetSprite("movingPlatformBorderH");
			pivotRotation = 0f;
			pivotPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0.25f, 0.25f));
			pivotPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width - 0.25f, 0.25f));
			borderPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0f, 0.25f));
			borderPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width, 0.25f));
			platformSoundIndex = -1;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("platformMoving", "audio\\noises\\platformmoving");
			estimatedArriveTime = ((path[currentPathPosition] - path[nextPathPosition]).Length() - distanceToDecelerate * 2f) / speed;
			estimatedArriveTime += 2.0 * Math.Sqrt(2f * distanceToDecelerate / deceleration);
			estimatedArriveTime = GameElementsControl.CurrentTimeInMS + estimatedArriveTime * 1000.0 + timeInStop;
			GameElementsControl.AddShadowCasterWorldObject(this);
			return true;
		}

		public void AddExitPortal(Portal enterPortal)
		{
			Vector2 vector = path[enterPortal.ActionToPerform.RefNumber];
			if (Math.Sign((enterPortal.Position - vector).X) > 0)
			{
				_ = enterPortal.Width;
			}
			Vector2 side = enterPortal.Center - new Vector2(0f, enterPortal.Height / 2f);
			PortalAction portalAction = new PortalAction("ExitPlatform", side, this);
			portalAction.RefNumber = enterPortal.ActionToPerform.RefNumber;
			Portal portal = new Portal(enterPortal.DestinationSector, enterPortal.OriginSector, mainBody.Position + new Vector2(width / 4f, -2f), width / 2f, 2f, portalAction);
			sector.addPortal(portal);
		}

		public override void Update()
		{
			if (stopTime + timeInStop <= GameElementsControl.CurrentTimeInMS)
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
					if (platformSoundIndex != -1)
					{
						float num3 = MathHelper.Clamp(1f - (GameElementsControl.Hero.Position - MainBody.Position).Length() / NoiseManager.MaxDistanceToHear, 0f, 1f);
						GameElementsControl.ScreenManager.AudioManager.SoundLoopVolume(platformSoundIndex, num3 * num3);
					}
				}
			}
			else
			{
				stopped = true;
			}
			shadowHull.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
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
				if (platformSoundIndex == -1)
				{
					platformSoundIndex = GameElementsControl.ScreenManager.AudioManager.PlaySoundLoop("platformMoving");
				}
				float num3 = mainBody.LinearVelocity.Length();
				float num4 = (float)((double)(num * 1000f) / GameElementsControl.LastFrameTimeInMS);
				if (!(num < distanceToDecelerate))
				{
					num3 = ((num2 < distanceToDecelerate) ? ((num2 != 0f) ? (mainBody.LinearVelocity.Length() + deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f)) : (deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f))) : ((!(num3 > num4)) ? speed : num4));
				}
				else
				{
					num3 = mainBody.LinearVelocity.Length() - deceleration * ((float)GameElementsControl.LastFrameTimeInMS / 1000f);
					if (num3 < 0f)
					{
						num3 = num4;
					}
				}
				if (vector.X > 0f || vector.Y > 0f)
				{
					pivotRotation += MathHelper.ToRadians(num3);
				}
				else if (vector.X < 0f || vector.Y < 0f)
				{
					pivotRotation -= MathHelper.ToRadians(num3);
				}
				sector.Position = mainBody.Position + new Vector2(0f, -2f);
				safeZone.Position = mainBody.Position + new Vector2(width / 4f, -2f);
				mainBody.LinearVelocity = vector * num3;
				pivotPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0.25f, 0.25f));
				pivotPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width - 0.25f, 0.25f));
				borderPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0f, 0.25f));
				borderPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width, 0.25f));
				return false;
			}
			if (currentPathPosition != nextPathPosition)
			{
				stopTime = estimatedArriveTime;
				stopped = true;
				estimatedArriveTime = ((path[currentPathPosition] - path[nextPathPosition]).Length() - distanceToDecelerate * 2f) / speed;
				estimatedArriveTime += 2.0 * Math.Sqrt(2f * distanceToDecelerate / deceleration);
				estimatedArriveTime = stopTime + estimatedArriveTime * 1000.0 + timeInStop;
				currentPathPosition = nextPathPosition;
				mainBody.LinearVelocity = Vector2.Zero;
				if (platformSoundIndex != -1)
				{
					GameElementsControl.ScreenManager.AudioManager.StopSoundLoop(platformSoundIndex);
					platformSoundIndex = -1;
				}
			}
			pivotPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0.25f, 0.25f));
			pivotPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width - 0.25f, 0.25f));
			borderPosition1 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(0f, 0.25f));
			borderPosition2 = GameElementsControl.ConvertWorldToScreen(mainBody.Position + new Vector2(width, 0.25f));
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
		}

		public void DrawHull()
		{
			platformSprite.DrawNoOrigin(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation, rectangle);
			pivotSprite.Draw(pivotPosition1, pivotRotation);
			pivotSprite.Draw(pivotPosition2, pivotRotation);
			borderSprite.Draw(borderPosition1, 0f, SpriteEffects.None);
			borderSprite.Draw(borderPosition2, 0f, SpriteEffects.FlipHorizontally);
		}
	}
}
