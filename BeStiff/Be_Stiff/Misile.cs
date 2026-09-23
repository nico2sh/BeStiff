using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	public class Misile : Projectile
	{
		private const float width = 0.5f;

		private Body body;

		private bool mustExplode;

		private Explosive explosive;

		private CircleEmitter trailEmitter;

		private double trailFrequency = 200.0;

		private double timeToEmitTrail;

		private GameSprite misileSprite;

		public override Body MainBody => body;

		public override float Mass => body.Mass;

		public Misile()
		{
			ProjectileType = ProjectileType.Misile;
		}

		public override void Load()
		{
			body = BodyFactory.CreateBody(GameElementsControl.World);
			body.BodyType = BodyType.Dynamic;
			body.IgnoreGravity = true;
			body.Enabled = false;
			CircleShape shape = new CircleShape(0.25f, 5f);
			Fixture fixture = body.CreateFixture(shape);
			fixture.UserData = new WorldObjectData(WorldObjectType.Misile, this);
			fixture.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fixture.OnCollision, new OnCollisionEventHandler(OnCollision));
			fixture.CollisionCategories = Category.Cat7;
			fixture.CollidesWith &= ~Category.Cat8;
			GameElementsControl.LoadSprite("misile", "sprites\\guns\\ammo\\misile");
			misileSprite = GameElementsControl.GetSprite("misile");
			active = false;
			base.Name = "Misile-" + GameElementsControl.Counter;
			explosive = new Explosive();
			trailEmitter = (CircleEmitter)GameElementsControl.Particlesmanager.getMisileTrailEmitter();
			GameElementsControl.AddWorldObject(this);
		}

		private bool OnCollision(Fixture fixture1, Fixture fixture2, Contact contact)
		{
			if (Globals.Collides(fixture2))
			{
				mustExplode = true;
			}
			return true;
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			mustExplode = true;
		}

		public override bool Shoot(Vector2 position, float angle, Vector2 pretendedPosition)
		{
			if (base.Shoot(position, angle, pretendedPosition))
			{
				body.Enabled = true;
				body.LinearVelocity = Vector2.Zero;
				body.Position = position;
				Vector2 impulse = Globals.CreateVector2(0f - angle, 1f);
				body.ApplyLinearImpulse(ref impulse);
				body.Rotation = angle;
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
			body.Enabled = false;
			body.LinearVelocity = Vector2.Zero;
		}

		public override void Update()
		{
			if (!active)
			{
				return;
			}
			if (mustExplode)
			{
				Explode(body.Position, 10f, 15f);
				mustExplode = false;
				return;
			}
			timeToEmitTrail -= GameElementsControl.LastFrameTimeInMS;
			if (timeToEmitTrail <= 0.0)
			{
				timeToEmitTrail += trailFrequency;
				trailEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(body.Position));
			}
			_ = body.LinearVelocity;
			Vector2 force = Vector2.Normalize(GameElementsControl.Hero.Position - body.Position) / (float)GameElementsControl.LastFrameTimeInMS * 100f;
			body.Rotation = body.LinearVelocity.GetAngle();
			body.ApplyForce(force);
		}

		public override void Draw()
		{
			if (active)
			{
				misileSprite.Draw(GameElementsControl.ConvertWorldToScreen(body.Position), body.Rotation);
			}
		}

		public override bool CanBeHooked()
		{
			return false;
		}
	}
}
