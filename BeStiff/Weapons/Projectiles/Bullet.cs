using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff.Weapons.Projectiles
{
	internal class Bullet : Projectile
	{
		private const float speed = 1f;

		private const float damage = 28f;

		private GameSprite bulletSprite;

		private Vector2 lastPos;

		private Vector2 pos;

		private Vector2 direction;

		private float bulletRot;

		private ConeEmitter impactEmitter;

		private LineEmitter trailEmitter;

		private CircleEmitter dustEmitter;

		private CircleEmitter shootEmitter;

		private Vector2[] points;

		private WorldObject[] worldObjects;

		private Body[] bodies;

		private string ownerName;

		public Bullet(string on)
		{
			ownerName = on;
			base.Name = "bullet-" + on + "-" + GameElementsControl.Counter;
			ProjectileType = ProjectileType.Bullet;
		}

		public override void Load()
		{
			GameElementsControl.LoadSprite("bullet", "sprites\\guns\\ammo\\bullet");
			bulletSprite = GameElementsControl.GetSprite("bullet");
			GameElementsControl.ScreenManager.AudioManager.LoadSound("ricochet", "audio\\noises\\ricochet");
			GameElementsControl.NoiseManager.LoadNoise("noiseBang", "bang", "gunshot", 250.0, 15f);
			trailEmitter = (LineEmitter)GameElementsControl.Particlesmanager.getBulletTrailEmitter();
			impactEmitter = (ConeEmitter)GameElementsControl.Particlesmanager.getHitSparkEmitter();
			dustEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getHitDustEmitter();
			shootEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getGunSmokeEmitter();
			GameElementsControl.AddWorldObject(this);
		}

		public override bool Shoot(Vector2 position, float angle, Vector2 pretendedPosition)
		{
			if (base.Shoot(position, angle, pretendedPosition))
			{
				lastPos = position;
				pos = position;
				direction = VectorUtil.CreateVector2(angle, 1f);
				shootEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(pretendedPosition));
				GameElementsControl.NoiseManager.AddVisualNoise("noiseBang", pretendedPosition);
				return true;
			}
			return false;
		}

		public override void Update()
		{
			if (active)
			{
				Vector2 point = lastPos;
				pos = lastPos + direction * 1f;
				Vector2 point2 = pos;
				if (GetImpact(point, point2))
				{
					active = false;
				}
				bulletRot = (lastPos - pos).GetAngle();
				Vector2 vector = GameElementsControl.ConvertWorldToScreen(lastPos - pos);
				trailEmitter.Angle = bulletRot;
				trailEmitter.Length = vector.Length();
				trailEmitter.Trigger(GameElementsControl.ConvertWorldToScreen((lastPos + pos) / 2f));
				lastPos = pos;
				if (lastShotTime + 300.0 < GameElementsControl.CurrentTimeInMS)
				{
					active = false;
				}
			}
		}

		public override void Draw()
		{
			if (active)
			{
				bulletSprite.Draw(GameElementsControl.ConvertWorldToScreen(pos), bulletRot);
			}
		}

		private bool GetImpact(Vector2 point1, Vector2 point2)
		{
			bool result = false;
			if (point1 != point2)
			{
				int num = RayCastCallBacks.RayCastBullet(point1, point2, ownerName, out worldObjects, out bodies, out points);
				for (int i = 0; i < num; i++)
				{
					Vector2 point3 = points[i];
					dustEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(points[i]));
					GameElementsControl.NoiseManager.AddNoise("ricochet", points[i]);
					Vector2 impulse = direction * 20f;
					bodies[i].ApplyLinearImpulse(ref impulse, ref point3);
					WorldObject worldObject = worldObjects[i];
					worldObject.Hit(direction * 28f, points[i], HitType.Pistol);
					if (worldObject.CanStopBullets())
					{
						impactEmitter.Direction = (-direction).GetAngle();
						impactEmitter.ReleaseRotation = (-direction).GetAngle();
						impactEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(points[i]));
						result = true;
						pos = points[i];
					}
				}
			}
			return result;
		}
	}
}
