using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Be_Stiff
{
	internal class Grenade : Projectile, IOneSidedAffected
	{
		private const float width = 0.4f;

		private bool mustExplode;

		private Explosive explosive;

		private string ownerName;

		private AnimatedSprite grenadeSprite;

		public WorldObject OneSidedIgnored => null;

		public Vector2 FloorPosition => mainBody.Position + new Vector2(0f, 0.2f); // y-down: bottom of the grenade

		public void OneSidedIgnore(WorldObject wo)
		{
		}

		public Grenade(string on)
		{
			ownerName = on;
			ProjectileType = ProjectileType.Grenade;
		}

		public bool IgnoreOneSide()
		{
			return false;
		}

		public override void Load()
		{
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Enabled = false;
			mainBody.AngularDamping = 10f;
			mainBody.Friction = 5f;
			CircleShape shape = new CircleShape(0.2f, 5f);
			Fixture fixture = mainBody.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.Grenade, this);
			fixture.CollidesWith &= ~Category.Cat3;
			fixture.CollisionCategories = Category.Cat3;
			active = false;
			base.Name = "Grenade-" + GameElementsControl.Counter;
			GameElementsControl.LoadSprite("grenade", "sprites\\guns\\ammo\\grenade");
			grenadeSprite = new AnimatedSprite(GameElementsControl.GetSprite("grenade"), 300.0, 2);
			explosive = new Explosive();
			GameElementsControl.ScreenManager.AudioManager.LoadSound("grenadeBounce", "audio\\noises\\grenadeBounce");
			GameElementsControl.AddWorldObject(this);
		}

		protected bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			if (fixtureB.UserData is WorldObjectData worldObjectData && worldObjectData.Object.Name == ownerName)
			{
				return false;
			}
			return true;
		}

		public override void CalculateImpactDamage(ref Contact contact, ref Manifold oldManifold, WorldObjectData wod)
		{
			if (CalculateRelativeSpeedFromContact(ref contact, ref oldManifold) > 0.5f)
			{
				GameElementsControl.NoiseManager.AddNoise("grenadeBounce", mainBody.Position);
			}
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			if (hitType == HitType.Pistol || hitType == HitType.Explosion)
			{
				mustExplode = true;
			}
		}

		public override bool Shoot(Vector2 position, float angle, Vector2 pretendedPosition)
		{
			if (base.Shoot(position, angle, pretendedPosition))
			{
				mainBody.Enabled = true;
				mainBody.LinearVelocity = Vector2.Zero;
				mainBody.Position = position;
				Vector2 impulse = Globals.CreateVector2(angle, 1f) * HeroPistol.GrenadeLaunchSpeed * Mass;
				mainBody.ApplyLinearImpulse(ref impulse);
				return true;
			}
			return false;
		}

		protected void Explode(Vector2 pos, float radius, float force)
		{
			Deactivate();
			explosive.Explode(pos, radius, force);
		}

		private void Deactivate()
		{
			active = false;
			mainBody.Enabled = false;
			mainBody.LinearVelocity = Vector2.Zero;
		}

		public override void Update()
		{
			if (!active)
			{
				return;
			}
			grenadeSprite.Update();
			if (mustExplode)
			{
				Explode(mainBody.Position, 5f, 20f);
				mustExplode = false;
			}
			else if (lastShotTime + 3000.0 <= GameElementsControl.CurrentTimeInMS)
			{
				grenadeSprite.SetTimeFrame(100.0);
				if (lastShotTime + 5000.0 <= GameElementsControl.CurrentTimeInMS)
				{
					mustExplode = true;
				}
			}
		}

		public override bool CanBeHooked()
		{
			return false;
		}

		public override void Draw()
		{
			if (active)
			{
				grenadeSprite.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation);
			}
		}
	}
}
