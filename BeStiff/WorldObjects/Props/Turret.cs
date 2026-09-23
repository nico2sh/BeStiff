using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using OgmoXNA;
using OgmoXNA.Values;
using ProjectMercury.Emitters;
using SKAnimation;

namespace Be_Stiff.WorldObjects.Props
{
	internal class Turret : WorldObject
	{
		private const float cannonRange = 20f;

		private Skeleton skeleton;

		private ChildBone cannonBone;

		private Vector2 targetPosition;

		private WorldObject targetWorldObject;

		private Explosive explosive;

		private float energy;

		private bool ignited;

		private bool hasToExplode;

		private bool exploded;

		private CannonStatus status;

		private float aimSpeed = 1500f;

		private double timeDetectedHero;

		private bool preparing;

		private Projectile ammo;

		private float shootFrequency;

		private double lastShot;

		private CircleEmitter shootEmitter;

		private ConeEmitter smoke;

		private double smokeFrequency;

		private double timeToEmitSmoke;

		public override float Mass => mainBody.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			status = CannonStatus.Scanning;
			string text = obj.Name;
			if (!text.Equals("Turret"))
			{
				return false;
			}
			aimSpeed = obj.GetValue<OgmoNumberValue>("AimSpeed").Value;
			shootFrequency = obj.GetValue<OgmoNumberValue>("ShootFrequency").Value;
			float rotation = 0f - MathHelper.ToRadians(obj.Rotation);
			base.Name = "Turret-" + GameElementsControl.Counter;
			preparing = false;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			mainBody.Rotation = rotation;
			Vertices vertices = new Vertices();
			vertices.Add(new Vector2(0.5f, -0.65f));
			vertices.Add(new Vector2(0.5f, 0.65f));
			vertices.Add(new Vector2(-0.5f, 0.65f));
			vertices.Add(new Vector2(-0.5f, -0.65f));
			PolygonShape shape = new PolygonShape(vertices, 500f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.Cannon, this);
			mainBody.CollisionCategories = Category.Cat2;
			mainBody.CollidesWith = Category.Cat1 | Category.Cat3 | Category.Cat4 | Category.Cat5 | Category.Cat6 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31;
			GameElementsControl.ScreenManager.AudioManager.LoadSound("turretArm", "audio\\noises\\turretarm");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("turretDisarm", "audio\\noises\\turretdisarm");
			BoneReader boneReader = new BoneReader();
			skeleton = boneReader.CreateFromFile(GameElementsControl.ScreenManager.Content, "skeletons\\turret\\turretskeleton", "skeletons\\turret\\turretanimations", "sprites\\dangers\\turret\\", GameElementsControl.World, ConvertUnits.DisplayToSimUnitsRatio, mainBody);
			skeleton.SetUserData(new WorldObjectData(WorldObjectType.Human, this));
			skeleton.Angle = (float)Math.PI;
			cannonBone = skeleton.GetBoneByName("cannon");
			cannonBone.IgnoreAnimation = IgnoreAnimationType.Angle;
			GameElementsControl.AddWorldObject(this);
			lastShot = 0.0;
			ammo = new Bullet(base.Name);
			ammo.Load();
			shootEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getCannonShootSmokeEmmiter();
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
			skeleton.Position = GameElementsControl.ConvertWorldToScreen(mainBody.Position);
			skeleton.Angle = mainBody.Rotation + (float)Math.PI;
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
			skeleton.Update(GameElementsControl.LastFrameTimeInMS);
			if (status == CannonStatus.Scanning)
			{
				skeleton.SetAnimation("STAND");
				float num = (GameElementsControl.Hero.Position - mainBody.Position).Length();
				if (num < 10f)
				{
					SetCrossHairPosition();
				}
			}
			else
			{
				if (status != CannonStatus.Hunting)
				{
					return;
				}
				skeleton.SetAnimation("READY");
				if (preparing)
				{
					if (timeDetectedHero + 500.0 < GameElementsControl.CurrentTimeInMS)
					{
						cannonBone.IgnoreAnimation = IgnoreAnimationType.Angle;
						preparing = false;
					}
					return;
				}
				Vector2 pos = GameElementsControl.Hero.Position;
				if (AimToPoint(ref pos))
				{
					Shoot();
				}
				SetCrossHairPosition();
				if (timeDetectedHero + (double)shootFrequency + 1000.0 < GameElementsControl.CurrentTimeInMS)
				{
					GameElementsControl.NoiseManager.AddNoise("turretDisarm", mainBody.Position);
					status = CannonStatus.Scanning;
					cannonBone.IgnoreAnimation = IgnoreAnimationType.None;
					preparing = false;
				}
			}
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
			skeleton.Dispose(GameElementsControl.World);
			GameElementsControl.World.RemoveBody(mainBody);
			base.Enabled = false;
		}

		private void Shoot()
		{
			if (lastShot + (double)shootFrequency < GameElementsControl.CurrentTimeInMS && !ammo.Active)
			{
				Vector2 vector = GameElementsControl.ConvertScreenToWorld(cannonBone.AbsolutePosition);
				lastShot = GameElementsControl.CurrentTimeInMS;
				if (ammo.Shoot(vector, cannonBone.AbsoluteAngle, vector))
				{
					shootEmitter.Trigger(cannonBone.GetPositionAtPercentage(1f));
				}
			}
		}

		private bool AimToPoint(ref Vector2 pos)
		{
			Vector2 vector = GameElementsControl.ConvertScreenToWorld(cannonBone.AbsolutePosition);
			float angle = (pos - vector).GetAngle();
			return AimTo(angle);
		}

		private bool AimTo(float angle)
		{
			float num = MathHelper.WrapAngle(cannonBone.AbsoluteAngle);
			if ((double)Math.Abs(num - MathHelper.WrapAngle(angle)) > 0.05)
			{
				float num2 = (float)(Math.PI / (double)aimSpeed * GameElementsControl.LastFrameTimeInMS);
				float num3 = MathHelper.Clamp(MathHelper.WrapAngle(cannonBone.AbsoluteAngle - angle), 0f - num2, num2);
				RotateCannon(num - num3);
				return false;
			}
			return true;
		}

		protected void RotateCannon(float angle)
		{
			cannonBone.SetAbsoluteAngle(angle, limitByOffset: false);
		}

		private void SetCrossHairPosition()
		{
			Vector2 point = GameElementsControl.ConvertScreenToWorld(cannonBone.AbsolutePosition);
			Vector2 position = GameElementsControl.Hero.Position;
			if (RayCastCallBacks.RayCastHookCrossHair(point, position, base.Name, out targetWorldObject, out targetPosition))
			{
				if (targetWorldObject is Hero)
				{
					timeDetectedHero = GameElementsControl.CurrentTimeInMS;
					if (status == CannonStatus.Scanning)
					{
						GameElementsControl.NoiseManager.AddNoise("turretArm", mainBody.Position);
						preparing = true;
						status = CannonStatus.Hunting;
					}
				}
			}
			else
			{
				targetPosition = position;
			}
		}

		public override void Draw()
		{
			skeleton.Draw(GameElementsControl.ScreenManager.SpriteBatch);
		}

		public override bool CanBeHooked()
		{
			return false;
		}
	}
}
