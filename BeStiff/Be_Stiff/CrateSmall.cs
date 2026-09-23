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

namespace Be_Stiff
{
	internal class CrateSmall : WorldObject, IOneSidedWorldObject
	{
		private Fixture fixtureCrate;

		private GameSprite spriteCrate;

		private bool projectile;

		private bool setUpAsProjectile;

		private List<WorldObject> safeWorldObjects;

		private SpriteEffects spriteEffect;

		public Vertices TheVertices { get; set; }

		public bool ActiveOneSide => !projectile;

		public bool IsProjectile => projectile;

		public override float Mass => mainBody.Mass;

		public override bool LoadFromOgmo(OgmoObject obj)
		{
			string text = obj.Name;
			if (!text.Equals("CrateSmall"))
			{
				return false;
			}
			base.Name = "Crate-" + GameElementsControl.Counter;
			WorldScaledOgmoObject worldScaledOgmoObject = new WorldScaledOgmoObject(obj);
			mainBody = BodyFactory.CreateBody(GameElementsControl.World);
			mainBody.BodyType = BodyType.Dynamic;
			mainBody.Position = worldScaledOgmoObject.Position;
			TheVertices = new Vertices();
			TheVertices.Add(new Vector2(-0.75f, -0.75f));
			TheVertices.Add(new Vector2(0.75f, -0.75f));
			TheVertices.Add(new Vector2(0.75f, 0.75f));
			TheVertices.Add(new Vector2(-0.75f, 0.75f));
			PolygonShape shape = new PolygonShape(TheVertices, 3f);
			fixtureCrate = mainBody.CreateFixture(shape);
			fixtureCrate.UserData = new WorldObjectData(WorldObjectType.OneSide, this);
			Fixture fixture = fixtureCrate;
			fixture.AfterCollision = (AfterCollisionEventHandler)Delegate.Combine(fixture.AfterCollision, new AfterCollisionEventHandler(AfterBarrelCollision));
			fixtureCrate.Restitution = 0f;
			GameElementsControl.LoadSprite("crateSmall", "sprites\\objects\\cratesmall");
			spriteCrate = GameElementsControl.GetSprite("crateSmall");
			SpriteEffects spriteEffects = SpriteEffects.None;
			int num = GameElementsControl.Random.Next(2);
			if (num == 1)
			{
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			spriteEffect = spriteEffects;
			safeWorldObjects = new List<WorldObject>(8);
			GameElementsControl.AddWorldObject(this);
			return true;
		}

		public override void Hit(Vector2 hitDirection, Vector2 position, HitType hitType)
		{
		}

		private void Deactivate()
		{
			mainBody.Enabled = false;
			GameElementsControl.World.RemoveBody(mainBody);
		}

		public override void Update()
		{
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
			_ = 1000f;
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
			spriteCrate.Draw(GameElementsControl.ConvertWorldToScreen(mainBody.Position), 0f - mainBody.Rotation, spriteEffect);
		}
	}
}
