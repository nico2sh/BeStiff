using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	internal class WallCannon : WorldObject
	{
		private const float cannonRange = 20f;

		private FixedRevoluteJoint worldJoint1;

		private FixedRevoluteJoint worldJoint2;

		private Cannon cannon;

		private Explosive explosive;

		private float energy;

		private bool ignited;

		private bool hasToExplode;

		private bool exploded;

		private CannonStatus status;

		private Side sideScanning;

		private float scanSpeed = 5000f;

		private float aimSpeed = 1500f;

		private float angleLimit;

		private double timeDetectedHero;

		private Projectile ammo;

		private float shootFrequency;

		private double lastShot;

		private AnimatedSprite cannonBody;

		private CircleEmitter shootEmitter;

		private LineEmitter laserSight;

		private ConeEmitter smoke;

		private double smokeFrequency;

		private double timeToEmitSmoke;

		private Vector2 targetPosition;

		private WorldObject targetWorldObject;

		public override float Mass => mainBody.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			status = CannonStatus.Scanning;
			sideScanning = Side.Left;
			string text = obj.Name;
			if (!text.Equals("Cannon"))
			{
				return false;
			}
			scanSpeed = obj.GetValue<OgmoNumberValue>("ScanSpeed").Value;
			aimSpeed = obj.GetValue<OgmoNumberValue>("AimSpeed").Value;
			shootFrequency = obj.GetValue<OgmoNumberValue>("ShootFrequency").Value;
			float rotation = 0f - MathHelper.ToRadians(obj.Rotation);
			base.Name = "Cannon-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.Rotation = rotation;
			worldJoint1 = new FixedRevoluteJoint(mainBody, new Vector2(0.5f, -0.5f), mainBody.GetWorldPoint(new Vector2(0.5f, -0.5f)));
			worldJoint2 = new FixedRevoluteJoint(mainBody, new Vector2(-0.5f, -0.5f), mainBody.GetWorldPoint(new Vector2(-0.5f, -0.5f)));
			GameElementsControl.World.AddJoint(worldJoint1);
			GameElementsControl.World.AddJoint(worldJoint2);
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(0.5f, -0.5f));
			vertices.Add(new Vector2(0.4f, -0.3f));
			vertices.Add(new Vector2(0.1f, -0.2f));
			vertices.Add(new Vector2(-0.1f, -0.2f));
			vertices.Add(new Vector2(-0.4f, -0.3f));
			vertices.Add(new Vector2(-0.5f, -0.5f));
			PolygonShape shape = new PolygonShape(vertices, 10f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.Cannon, this);
			cannon = new Cannon();
			cannon.Rotation = mainBody.Rotation;
			GameElementsControl.LoadSprite("cannonBody", "sprites\\dangers\\cannonbody");
			cannonBody = new AnimatedSprite(GameElementsControl.GetSprite("cannonBody"), 1000.0, 2, 2, 1, 1);
			GameElementsControl.NoiseManager.LoadNoise("noiseMisileShoot", "thud", "misileshoot", 250.0, 5f);
			GameElementsControl.AddWorldObject(this);
			angleLimit = MathHelper.ToRadians(70f);
			lastShot = 0.0;
			ammo = new Misile();
			ammo.Load();
			shootEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getCannonShootSmokeEmmiter();
			laserSight = (LineEmitter)GameElementsControl.Particlesmanager.getLaserSightEmitter();
			smoke = (ConeEmitter)GameElementsControl.Particlesmanager.getSmokeCannonEmitter();
			smokeFrequency = 400.0;
			timeToEmitSmoke = 0.0;
			explosive = new Explosive();
			energy = 1000f;
			ignited = false;
			exploded = false;
			return true;
		}

		public override void Update()
		{
			cannon.Update(mainBody.GetWorldPoint(new Vector2(0f, -0.3f)));
			cannonBody.Update();
			if (energy <= 500f && !exploded)
			{
				if (energy <= 0f)
				{
					Explode();
				}
				else if (!ignited)
				{
					ignited = true;
				}
			}
			if (ignited)
			{
				timeToEmitSmoke -= GameElementsControl.LastFrameTimeInMS;
				if (timeToEmitSmoke <= 0.0)
				{
					timeToEmitSmoke += smokeFrequency;
					smoke.Direction = 0f - mainBody.Rotation - (float)Math.PI / 2f;
					smoke.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.Position));
				}
			}
			if (status == CannonStatus.Scanning)
			{
				MathHelper.ToDegrees((float)sideScanning * (angleLimit - 2f));
				if (AimToRelativeAngle((float)sideScanning * (angleLimit / 2f)))
				{
					sideScanning = (Side)(0 - sideScanning);
				}
			}
			else if (status == CannonStatus.Hunting)
			{
				Vector2 pos = GameElementsControl.Hero.Position;
				AimToPoint(ref pos);
				if (timeDetectedHero + 1000.0 < GameElementsControl.CurrentTimeInMS)
				{
					status = CannonStatus.Scanning;
				}
				Shoot();
			}
			setCrossHairPosition();
			Vector2 vector = targetPosition - cannon.CannonPosition;
			float num = GameElementsControl.ConvertWorldToScreen(vector).Length();
			laserSight.Length = num;
			laserSight.ReleaseQuantity = (int)(num / 10f) + 1;
			laserSight.Angle = vector.GetAngle();
			laserSight.Trigger(GameElementsControl.ConvertWorldToScreen((targetPosition + cannon.CannonPosition) / 2f));
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			switch (hitType)
			{
			case HitType.Pistol:
				energy -= 150f;
				GameElementsControl.Score.AddPoints(Globals.ScoreShootCannon);
				break;
			case HitType.Explosion:
			{
				float num = MathHelper.Clamp(hitDirection.Length() * 15f, 0f, 1000f);
				num *= (float)Globals.ScoreExplodeEnemy;
				energy -= hitDirection.Length() * 15f;
				GameElementsControl.Score.AddPoints((int)num);
				break;
			}
			}
			if (energy <= 300f)
			{
				smokeFrequency = energy / 2f + 100f;
			}
		}

		private void Explode()
		{
			exploded = true;
			explosive.Explode(mainBody.Position, 5f, 2f);
			GameElementsControl.World.RemoveBody(mainBody);
			base.Enabled = false;
		}

		private void Shoot()
		{
			if (lastShot + (double)shootFrequency < GameElementsControl.CurrentTimeInMS && !ammo.Active)
			{
				lastShot = GameElementsControl.CurrentTimeInMS;
				if (ammo.Shoot(cannon.CannonPosition, cannon.Rotation + (float)Math.PI / 2f, cannon.CannonPosition))
				{
					GameElementsControl.NoiseManager.AddVisualNoise("noiseMisileShoot", mainBody.Position);
					shootEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(cannon.CannonPosition));
				}
			}
		}

		private bool AimToRelativeAngle(float angle)
		{
			angle = MathHelper.Clamp(mainBody.Rotation - angle, mainBody.Rotation - angleLimit, mainBody.Rotation + angleLimit);
			float num = MathHelper.WrapAngle(cannon.Rotation);
			if ((double)Math.Abs(num - MathHelper.WrapAngle(angle)) > 0.05)
			{
				float num2 = (float)(Math.PI / (double)scanSpeed * GameElementsControl.LastFrameTimeInMS);
				float num3 = MathHelper.Clamp(MathHelper.WrapAngle(cannon.Rotation - angle), 0f - num2, num2);
				RotateCannon(num - num3);
				return false;
			}
			return true;
		}

		private bool AimToPoint(ref Vector2 pos)
		{
			float angle = 0f - (pos - cannon.Position).GetAngle();
			return AimTo(angle);
		}

		private bool AimTo(float angle)
		{
			angle -= (float)Math.PI / 2f;
			angle = MathHelper.Clamp(0f - MathHelper.WrapAngle(mainBody.Rotation - angle) + mainBody.Rotation, mainBody.Rotation - angleLimit, mainBody.Rotation + angleLimit);
			float num = MathHelper.WrapAngle(cannon.Rotation);
			if ((double)Math.Abs(num - MathHelper.WrapAngle(angle)) > 0.05)
			{
				float num2 = (float)(Math.PI / (double)aimSpeed * GameElementsControl.LastFrameTimeInMS);
				float num3 = MathHelper.Clamp(MathHelper.WrapAngle(cannon.Rotation - angle), 0f - num2, num2);
				RotateCannon(num - num3);
				return false;
			}
			return true;
		}

		protected void RotateCannon(float angle)
		{
			cannon.Rotation = angle;
		}

		private void setCrossHairPosition()
		{
			float x = cannon.Position.X + 20f * (float)Math.Sin(0f - cannon.Rotation);
			float y = cannon.Position.Y + 20f * (float)Math.Cos(0f - cannon.Rotation);
			Vector2 point = new Vector2(x, y);
			if (RayCastCallBacks.RayCastHookCrossHair(cannon.Position, point, base.Name, out targetWorldObject, out targetPosition))
			{
				if (targetWorldObject is Hero)
				{
					timeDetectedHero = GameElementsControl.CurrentTimeInMS;
					status = CannonStatus.Hunting;
				}
			}
			else
			{
				targetPosition = point;
			}
		}

		public override void Draw()
		{
			cannon.Draw();
			cannonBody.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation);
		}

		public override bool CanBeHooked()
		{
			return false;
		}
	}
}
