using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OgmoXNA;
using ProjectMercury.Emitters;

namespace Be_Stiff
{
	internal class Barrel : WorldObject, IOneSidedWorldObject
	{
		private const double timeToExplode = 4000.0;

		private Fixture fixtureBarrel;

		private GameSprite spriteBarrel;

		private double timeForIgnition;

		private bool ignited;

		private bool hasToExplode;

		private bool exploded;

		private ConeEmitter smokeEmitter;

		private Vector2 smokePos;

		private float smokeAngle;

		private bool projectile;

		private bool setUpAsProjectile;

		private List<WorldObject> safeWorldObjects;

		private SpriteEffects spriteEffect;

		private Explosive explosive;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => !projectile;

		public bool IsProjectile => projectile;

		public override float Mass => mainBody.Mass;

		public Barrel()
		{
			hasToExplode = false;
			exploded = false;
			ignited = false;
		}

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			string text = obj.Name;
			if (!text.Equals("Barrel"))
			{
				return false;
			}
			base.Name = "Barrel-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-0.5f, -0.75f));
			TheVertices.Add(new Vector2(0.5f, -0.75f));
			TheVertices.Add(new Vector2(0.5f, 0.75f));
			TheVertices.Add(new Vector2(-0.5f, 0.75f));
			PolygonShape shape = new PolygonShape(TheVertices, 10f);
			fixtureBarrel = mainBody.CreateFixture(shape);
			fixtureBarrel.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			Fixture fixture = fixtureBarrel;
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(AfterBarrelCollision));
			fixtureBarrel.Restitution = 0f;
			int num = GameElementsControl.Random.Next(1, 5);
			GameElementsControl.LoadSprite("barrel" + num, "sprites\\objects\\barrel" + num);
			spriteBarrel = GameElementsControl.GetSprite("barrel" + num);
			SpriteEffects spriteEffects = SpriteEffects.None;
			int num2 = GameElementsControl.Random.Next(2);
			if (num2 == 1)
			{
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			spriteEffect = spriteEffects;
			safeWorldObjects = new List<WorldObject>(8);
			explosive = new Explosive();
			GameElementsControl.AddWorldObject(this);
			smokeEmitter = (ConeEmitter)GameElementsControl.Particlesmanager.getSmokeBarrelEmitter();
			smokePos = Vector2.Zero;
			return true;
		}

		private void Ignite(Vector2 worldPos)
		{
			timeForIgnition = 4000.0;
			ignited = true;
			if (worldPos == Vector2.Zero)
			{
				smokePos.X = 0f;
				smokePos.Y = 0.75f;
				smokeAngle = (float)Math.PI / 2f;
				return;
			}
			Vector2 localPoint = mainBody.GetLocalPoint(worldPos);
			if (localPoint.X < 0f)
			{
				smokeAngle = (float)Math.PI;
				smokePos.X = -0.5f;
			}
			else
			{
				smokeAngle = 0f;
				smokePos.X = 0.5f;
			}
			smokePos.Y = localPoint.Y;
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
			if (exploded)
			{
				return;
			}
			switch (hitType)
			{
			case HitType.Pistol:
				if (!ignited)
				{
					Ignite(position);
				}
				else
				{
					timeForIgnition -= 1000.0;
				}
				break;
			case HitType.Explosion:
			{
				float num = hitDirection.Length();
				if (!ignited)
				{
					if (num > 100f)
					{
						Ignite(position);
						timeForIgnition = 4000.0 - (double)(num * 10f);
					}
				}
				else
				{
					timeForIgnition -= num * 10f;
				}
				break;
			}
			case HitType.Kick:
			case HitType.Blunt:
				break;
			}
		}

		protected void Explode(Vector2 pos, float radius, float maxForce)
		{
			Deactivate();
			explosive.Explode(pos, radius, maxForce);
		}

		private void Deactivate()
		{
			mainBody.Enabled = false;
			GameElementsControl.World.RemoveBody(mainBody);
		}

		public override void Update()
		{
			if (exploded)
			{
				return;
			}
			if (hasToExplode)
			{
				Explode(mainBody.Position, 7f, 20f);
				hasToExplode = false;
				exploded = true;
				ignited = false;
				return;
			}
			if (ignited)
			{
				smokeEmitter.Direction = smokeAngle + MathHelper.WrapAngle(mainBody.Rotation);
				smokeEmitter.Trigger(GameElementsControl.ConvertWorldToScreen(mainBody.GetWorldPoint(smokePos)));
				timeForIgnition -= GameElementsControl.LastFrameTimeInMS;
				if (timeForIgnition <= 0.0)
				{
					hasToExplode = true;
				}
			}
			if (setUpAsProjectile)
			{
				ContactEdge contactEdge = mainBody.ContactList;
				if (contactEdge != null)
				{
					do
					{
						foreach (Fixture fixture in contactEdge.Other.FixtureList)
						{
							if (fixture.UserData is WorldObjectData worldObjectData && worldObjectData.Type != WorldObjectType.CollisionWorldObject && !safeWorldObjects.Contains(worldObjectData.Object))
							{
								safeWorldObjects.Add(worldObjectData.Object);
							}
						}
						contactEdge = contactEdge.Next;
					}
					while (contactEdge != null);
				}
				setUpAsProjectile = false;
				projectile = true;
			}
			if (!projectile && mainBody.LinearVelocity.Length() > 5f)
			{
				setUpAsProjectile = true;
			}
			if (projectile && mainBody.LinearVelocity.Length() < 5f)
			{
				projectile = false;
				safeWorldObjects.Clear();
			}
		}

		private void AfterBarrelCollision(Fixture f1, Fixture f2, Contact contact)
		{
			float num = CalculateMaxImpulse(contact);
			if (num >= 1000f)
			{
				hasToExplode = true;
			}
		}

		public bool ConditionalContacts(WorldObject wo)
		{
			if (projectile)
			{
				if (safeWorldObjects.Contains(wo))
				{
					return false;
				}
				return true;
			}
			return true;
		}

		public override bool CanSeeThrough(bool alert)
		{
			return alert;
		}

		public override void Draw()
		{
			if (!exploded)
			{
				spriteBarrel.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), mainBody.Rotation, spriteEffect);
			}
		}
	}
}
